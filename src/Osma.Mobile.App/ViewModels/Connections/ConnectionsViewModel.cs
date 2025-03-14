﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AgentFramework.Core.Contracts;
using AgentFramework.Core.Messages.Connections;
using AgentFramework.Core.Utils;
using Autofac;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Extensions;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.ViewModels.CreateInvitation;
using ReactiveUI;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class ConnectionsViewModel : ABaseViewModel
    {
        private readonly IConnectionService _connectionService;
        private readonly ICustomAgentContextProvider _agentContextProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILifetimeScope _scope;

        public ConnectionsViewModel(IUserDialogs userDialogs,
                                    INavigationService navigationService,
                                    IConnectionService connectionService,
                                    ICustomAgentContextProvider agentContextProvider,
                                    IEventAggregator eventAggregator,
                                    ILifetimeScope scope) :
                                    base("Connections", userDialogs, navigationService)
        {
            _connectionService = connectionService;
            _agentContextProvider = agentContextProvider;
            _eventAggregator = eventAggregator;
            _scope = scope;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await RefreshConnections();

            _eventAggregator.GetEventByType<ApplicationEvent>()
                            .Where(_ => _.Type == ApplicationEventType.ConnectionsUpdated)
                            .Subscribe(async _ => await RefreshConnections());

            await base.InitializeAsync(navigationData);
        }


        public async Task RefreshConnections()
        {
            RefreshingConnections = true;

            var context = await _agentContextProvider.GetContextAsync();
            var records = await _connectionService.ListAsync(context);

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
            if (DeviceInfo.DeviceType == DeviceType.Physical)
                await ScanPhysicalInviteAsync();
            else
                await ScanTextInviteAsync();
        }

        private async Task ScanPhysicalInviteAsync()
        {
            var expectedFormat = ZXing.BarcodeFormat.QR_CODE;
            var opts = new ZXing.Mobile.MobileBarcodeScanningOptions{ PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat }};

            var scannerPage = new ZXingScannerPage(opts);
            scannerPage.OnScanResult += (result) => {
                scannerPage.IsScanning = false;

                ConnectionInvitationMessage invitation;

                try
                {
                    invitation = MessageUtils.DecodeMessageFromUrlFormat<ConnectionInvitationMessage>(result.Text);
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

        private async Task ScanTextInviteAsync()
        {
            await NavigationService.NavigateToAsync<TextInviteViewModel>();
        }

        public async Task SelectConnection(ConnectionViewModel connection) => await NavigationService.NavigateToAsync(connection);

        #region Bindable Command
        public ICommand RefreshCommand => new Command(async () => await RefreshConnections());

        public ICommand ScanInviteCommand => new Command(async () => await ScanInvite());

        public ICommand CreateInvitationCommand => new Command(async () => await NavigationService.NavigateToAsync<CreateInvitationViewModel>());

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
