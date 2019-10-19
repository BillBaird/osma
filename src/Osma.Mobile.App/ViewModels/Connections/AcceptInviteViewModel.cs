using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AgentFramework.Core.Contracts;
using AgentFramework.Core.Messages.Connections;
using AgentFramework.Core.Exceptions;
using AgentFramework.Core.Extensions;
using AgentFramework.Core.Models;
using AgentFramework.Core.Utils;
using Autofac;
using Hyperledger.Indy.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class AcceptInviteViewModel : ABaseViewModel
    {
        private readonly IProvisioningService _provisioningService;
        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;
        private readonly ICustomAgentContextProvider _contextProvider;
        private readonly IEventAggregator _eventAggregator;
        private static readonly String GENERIC_CONNECTION_REQUEST_FAILURE_MESSAGE = "Failed to accept invite!";

        private ConnectionInvitationMessage _invite;

        public AcceptInviteViewModel(IUserDialogs userDialogs,
                                     INavigationService navigationService,
                                     IProvisioningService provisioningService,
                                     IConnectionService connectionService,
                                     IMessageService messageService,
                                     ICustomAgentContextProvider contextProvider,
                                     IEventAggregator eventAggregator)
                                     : base("Accept Invitiation", userDialogs, navigationService)
        {
            _provisioningService = provisioningService;
            _connectionService = connectionService;
            _contextProvider = contextProvider;
            _messageService = messageService;
            _contextProvider = contextProvider;
            _eventAggregator = eventAggregator;
        }

        public override Task InitializeAsync(object navigationData)
        {
            if (navigationData is ConnectionInvitationMessage invite)
            {
                InviteTitle = $"Trust {invite.Label}?";
                InviterUrl = invite.ImageUrl;
                InviteContents = $"{invite.Label} would like to establish a pairwise DID connection with you. This will allow secure communication between you and {invite.Label}.";
                _invite = invite;
            }
            return base.InitializeAsync(navigationData);
        }

        private async Task CreateConnection(IAgentContext context, ConnectionInvitationMessage invite)
        {
            var logger = App.Container.Resolve<ILogger<AcceptInviteViewModel>>();
            var fmt = new JsonSerializerSettings {Formatting = Formatting.Indented};
            logger.LogTrace($"context = {context.ToJson(fmt)}");
            logger.LogTrace($"_invite = {_invite.ToJson(fmt)}");

            var provisioningRecord = await _provisioningService.GetProvisioningAsync(context.Wallet);
            logger.LogTrace($"provisioningRecord = {provisioningRecord.ToJson(new JsonSerializerSettings{Formatting = Formatting.Indented})}");
            
            var isEndpointUriAbsent = provisioningRecord.Endpoint.Uri == null;
            var (msg, rec) = await _connectionService.CreateRequestAsync(context, _invite);
            msg.Label = DeviceInfo.Name + " osma agent";
            logger.LogTrace($"Connection Message (msg) being Sent = {msg.ToJson(fmt)}");
            logger.LogTrace($"ConnectionRecord (rec) being saved = {rec.ToJson(fmt)}");
            
            var rsp = await _messageService.SendAsync(context.Wallet, msg, rec, _invite.RecipientKeys.First(), isEndpointUriAbsent);
            logger.LogTrace($"rsp = {rsp.ToJson(fmt)}");
            if (isEndpointUriAbsent)
            {
                var response = rsp.GetMessage<ConnectionResponseMessage>();
                logger.LogTrace($"response = {response.ToJson(fmt)}");
                await _connectionService.ProcessResponseAsync(context, response, rec);
            }
        }

        #region Bindable Commands
        public ICommand AcceptInviteCommand => new Command(async () =>
        {
            var loadingDialog = DialogService.Loading("Processing");

            var context = await _contextProvider.GetContextAsync();

            if (context == null || _invite == null)
            {
                loadingDialog.Hide();
                DialogService.Alert("Failed to decode invite!");
                return;
            }

            String errorMessage = String.Empty;
            try
            {
                await CreateConnection(context, _invite);
            }
            catch (AgentFrameworkException agentFrameworkException)
            {
                errorMessage = agentFrameworkException.Message;
            }
            catch (Exception) //TODO more granular error protection
            {
                errorMessage = GENERIC_CONNECTION_REQUEST_FAILURE_MESSAGE;
            }

            _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ConnectionsUpdated });

            if (loadingDialog.IsShowing)
                loadingDialog.Hide();

            if (!String.IsNullOrEmpty(errorMessage))
                DialogService.Alert(errorMessage);

            await NavigationService.PopModalAsync();
        });

        public ICommand RejectInviteCommand => new Command(async () => await NavigationService.PopModalAsync());

        #endregion

        #region Bindable Properties
        private string _inviteTitle;
        public string InviteTitle
        {
            get => _inviteTitle;
            set => this.RaiseAndSetIfChanged(ref _inviteTitle, value);
        }

        private string _inviteContents = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua";
        public string InviteContents
        {
            get => _inviteContents;
            set => this.RaiseAndSetIfChanged(ref _inviteContents, value);
        }

        private string _inviterUrl;
        public string InviterUrl
        {
            get => _inviterUrl;
            set => this.RaiseAndSetIfChanged(ref _inviterUrl, value);
        }
        #endregion
    }
}
