using System.Threading.Tasks;
using Acr.UserDialogs;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.ViewModels.Account;
using Osma.Mobile.App.ViewModels.Connections;
using Osma.Mobile.App.ViewModels.CreateInvitation;
using Osma.Mobile.App.ViewModels.Credentials;
using Osma.Mobile.App.ViewModels.Wallet;
using ReactiveUI;

namespace Osma.Mobile.App.ViewModels
{
    public class MainViewModel : ABaseViewModel
    {
        public MainViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService,
            ConnectionsViewModel connectionsViewModel,
            CredentialsViewModel credentialsViewModel,
            AccountViewModel accountViewModel,
            WalletViewModel walletViewModel,
            CreateInvitationViewModel createInvitationViewModel
        ) : base(
                nameof(MainViewModel),
                userDialogs,
                navigationService
        )
        {
            Connections = connectionsViewModel;
            Credentials = credentialsViewModel;
            Account = accountViewModel;
            Wallet = walletViewModel;
            CreateInvitation = createInvitationViewModel;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await Connections.InitializeAsync(null);
            await Credentials.InitializeAsync(null);
            await Account.InitializeAsync(null);
            await Wallet.InitializeAsync(null);
            await CreateInvitation.InitializeAsync(null);
            await base.InitializeAsync(navigationData);
        }

        #region Bindable Properties
        private ConnectionsViewModel _connections;
        public ConnectionsViewModel Connections
        {
            get => _connections;
            set => this.RaiseAndSetIfChanged(ref _connections, value);
        }

        private CredentialsViewModel _credentials;
        public CredentialsViewModel Credentials
        {
            get => _credentials;
            set => this.RaiseAndSetIfChanged(ref _credentials, value);
        }

        private AccountViewModel _account;
        public AccountViewModel Account
        {
            get => _account;
            set => this.RaiseAndSetIfChanged(ref _account, value);
        }

        private WalletViewModel _wallet;
        public WalletViewModel Wallet
        {
            get => _wallet;
            set => this.RaiseAndSetIfChanged(ref _wallet, value);
        }

        private CreateInvitationViewModel _createInvitation;
        public CreateInvitationViewModel CreateInvitation
        {
            get => _createInvitation;
            set => this.RaiseAndSetIfChanged(ref _createInvitation, value);
        }
        #endregion
    }
}
