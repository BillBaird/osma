using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AgentFramework.Core.Contracts;
using AgentFramework.Core.Models.Records;
using Autofac;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Extensions;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Wallet
{
    public class WalletViewModel : ABaseViewModel
    {
        private readonly IWalletService _walletService;
        private readonly IWalletRecordService _recordService;
        private readonly ICustomAgentContextProvider _agentContextProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILifetimeScope _scope;

        public WalletViewModel(IUserDialogs userDialogs,
                                       INavigationService navigationService,
                                       IWalletService walletService,
                                       IWalletRecordService recordService,
                                       ICustomAgentContextProvider agentContextProvider,
                                       IEventAggregator eventAggregator,
                                       ILifetimeScope scope) :
                                       base("Wallet",
                                           userDialogs,
                                           navigationService)
        {
            _walletService = walletService;
            _recordService = recordService;
            _agentContextProvider = agentContextProvider;
            _eventAggregator = eventAggregator;
            _scope = scope;
        }
        
        public override async Task InitializeAsync(object navigationData)
        {
            await RefreshWallet();

            _eventAggregator.GetEventByType<ApplicationEvent>()
                .Where(_ => _.Type == ApplicationEventType.ConnectionsUpdated)
                .Subscribe(async _ => await RefreshWallet());

            await base.InitializeAsync(navigationData);
        }

        public async Task RefreshWallet()
        {
            RefreshingRecords = true;

            var context = await _agentContextProvider.GetContextAsync();

            var wallet = context.Wallet;
            
            var provisioningRecords = await _recordService.SearchAsync<ProvisioningRecord>(wallet, null, null, 100);
            var connectionRecords = await _recordService.SearchAsync<ConnectionRecord>(wallet, null, null, 100);

            IList<RecordViewModel> recordVms = new List<RecordViewModel>();
            foreach (var record in provisioningRecords)
            {
                var connection = _scope.Resolve<RecordViewModel>(new NamedParameter("record", record));
                recordVms.Add(connection);
            }
            foreach (var record in connectionRecords)
            {
                var connection = _scope.Resolve<RecordViewModel>(new NamedParameter("record", record));
                recordVms.Add(connection);
            }

            //TODO need to compare with the currently displayed connections rather than disposing all of them
            Records.Clear();
            Records.InsertRange(recordVms);

            HasRecords = recordVms.Any();
            this.RaisePropertyChanged(nameof(Records));

            RefreshingRecords = false;
        }
        
        #region Bindable Command
        public ICommand RefreshCommand => new Command(async () => await RefreshWallet());
        #endregion

        #region Bindable Properties
        private RangeEnabledObservableCollection<RecordViewModel> _records = new RangeEnabledObservableCollection<RecordViewModel>();
        public RangeEnabledObservableCollection<RecordViewModel> Records
        {
            get => _records;
            set => this.RaiseAndSetIfChanged(ref _records, value);
        }

        private bool _hasRecords;
        public bool HasRecords
        {
            get => _hasRecords;
            set => this.RaiseAndSetIfChanged(ref _hasRecords, value);
        }

        private bool _refreshingRecords;
        public bool RefreshingRecords
        {
            get => _refreshingRecords;
            set => this.RaiseAndSetIfChanged(ref _refreshingRecords, value);
        }
        #endregion
    }
}