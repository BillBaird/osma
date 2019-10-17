using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using AgentFramework.Core.Contracts;
using Autofac;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Services.Interfaces;

namespace Osma.Mobile.App.ViewModels.Wallet
{
    public class WalletViewModel : ABaseViewModel
    {
        private readonly IWalletService _walletService;
        private readonly IConnectionService _connectionService;
        private readonly ICustomAgentContextProvider _agentContextProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILifetimeScope _scope;

        public WalletViewModel(IUserDialogs userDialogs,
                                       INavigationService navigationService,
                                       IWalletService walletService,
                                       IConnectionService connectionService,
                                       ICustomAgentContextProvider agentContextProvider,
                                       IEventAggregator eventAggregator,
                                       ILifetimeScope scope) :
                                       base("Wallet",
                                           userDialogs,
                                           navigationService)
        {
            _walletService = walletService;
            _connectionService = connectionService;
            _agentContextProvider = agentContextProvider;
            _eventAggregator = eventAggregator;
            _scope = scope;
        }
        
        public override async Task InitializeAsync(object navigationData)
        {
            await RefreshWalletContents();

//            _eventAggregator.GetEventByType<ApplicationEvent>()
//                .Where(_ => _.Type == ApplicationEventType.ConnectionsUpdated)
//                .Subscribe(async _ => await RefreshWalletContents());

            await base.InitializeAsync(navigationData);
        }

        public async Task RefreshWalletContents()
        {
//            RefreshingConnections = true;
//
//            var context = await _agentContextProvider.GetContextAsync();
//            var records = await _connectionService.ListAsync(context);
//
//            IList<ConnectionViewModel> connectionVms = new List<ConnectionViewModel>();
//            foreach (var record in records)
//            {
//                var connection = _scope.Resolve<ConnectionViewModel>(new NamedParameter("record", record));
//                connectionVms.Add(connection);
//            }
//
//            //TODO need to compare with the currently displayed connections rather than disposing all of them
//            Connections.Clear();
//            Connections.InsertRange(connectionVms);
//            HasConnections = connectionVms.Any();
//
//            RefreshingConnections = false;
        }
    }
}