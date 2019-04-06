﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Autofac;
using Poc.Mobile.App.Extensions;
using Poc.Mobile.App.Services;
using Poc.Mobile.App.Services.Interfaces;
using Poc.Mobile.App.Services.Utils;
using ReactiveUI;
using Streetcred.Sdk.Contracts;
using Streetcred.Sdk.Messages.Connections;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Poc.Mobile.App.ViewModels.Connections
{
    public class ConnectionsViewModel : ABaseViewModel
    {
        private readonly IConnectionService _connectionService;
        private readonly IAgentContextService _agentContextService;
        private readonly ILifetimeScope _scope;

        public ConnectionsViewModel(IUserDialogs userDialogs,
                                    INavigationService navigationService,
                                    IConnectionService connectionService,
                                    IAgentContextService agentContextService,
                                    ILifetimeScope scope) :
                                    base("My Connections", userDialogs, navigationService)
        {
            _connectionService = connectionService;
            _agentContextService = agentContextService;
            _scope = scope;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await RefreshConnections();
            await base.InitializeAsync(navigationData);
        }


        public async Task RefreshConnections()
        {
            RefreshingConnections = true;

            var context = await _agentContextService.GetContextAsync();
            var records = await _connectionService.ListAsync(context.Wallet);

            #if DEBUG
            var exampleRecord = new Streetcred.Sdk.Models.Records.ConnectionRecord
            {
                ConnectionId = Guid.NewGuid().ToString().ToLowerInvariant(),
                Alias = new Streetcred.Sdk.Models.Connections.ConnectionAlias {
                    Name = "Example Connection",
                    ImageUrl = "https://placehold.it/300x300"
                },
                MyDid = "sov:7N2DqXEPRG7wbqJvJL3diU",
                State = Streetcred.Sdk.Models.Records.ConnectionState.Connected,
                TheirDid = "sov:KNWvuaPtWtL8fgaArBeKr1",
            };
            records.Add(exampleRecord);
            #endif

            IList<ConnectionViewModel> connectionVms = new List<ConnectionViewModel>();
            foreach (var record in records)
            {
                var connection = _scope.Resolve<ConnectionViewModel>(new NamedParameter("record", record));
                connectionVms.Add(connection);
            }


            //TODO need to compare with the currently displayed connections rather than disposing all of them
            Connections.Clear();
            Connections.InsertRange(connectionVms);
            HasConnections = connectionVms.Any();

            RefreshingConnections = false;
        }

        public async Task ScanInvite()
        {
            var expectedFormat = ZXing.BarcodeFormat.QR_CODE;
            var opts = new ZXing.Mobile.MobileBarcodeScanningOptions{ PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat }};

            var scannerPage = new ZXingScannerPage(opts);
            scannerPage.OnScanResult += (result) => {
                scannerPage.IsScanning = false;

                ConnectionInvitationMessage invitation;

                try
                {
                    invitation = InvitationUtils.DecodeInvite(result.Text);
                }
                catch (Exception)
                {
                    DialogService.Alert("Invalid invitation!");
                    Device.BeginInvokeOnMainThread(async () => await NavigationService.PopModalAsync());
                    return;
                }

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await NavigationService.PopModalAsync();
                    await NavigationService.NavigateToAsync<AcceptInviteViewModel>(invitation, NavigationType.Modal);
                });
            };

            await NavigationService.NavigateToAsync((Page)scannerPage, NavigationType.Modal);
        }

        public async Task SelectConnection(ConnectionViewModel connection) => await NavigationService.NavigateToAsync(connection, null, NavigationType.Modal);

        #region Bindable Command
        public ICommand RefreshCommand => new Command(async () => await RefreshConnections());

        public ICommand ScanInviteCommand => new Command(async () => await ScanInvite());

        public ICommand SelectConnectionCommand => new Command<ConnectionViewModel>(async (connection) =>
        {
            if (connection != null)
                await SelectConnection(connection);
        });
        #endregion

        #region Bindable Properties
        private RangeEnabledObservableCollection<ConnectionViewModel> _connections = new RangeEnabledObservableCollection<ConnectionViewModel>();
        public RangeEnabledObservableCollection<ConnectionViewModel> Connections
        {
            get => _connections;
            set => this.RaiseAndSetIfChanged(ref _connections, value);
        }

        private bool _hasConnections;
        public bool HasConnections
        {
            get => _hasConnections;
            set => this.RaiseAndSetIfChanged(ref _hasConnections, value);
        }

        private bool _refreshingConnections;
        public bool RefreshingConnections
        {
            get => _refreshingConnections;
            set => this.RaiseAndSetIfChanged(ref _refreshingConnections, value);
        }
        #endregion
    }
}
