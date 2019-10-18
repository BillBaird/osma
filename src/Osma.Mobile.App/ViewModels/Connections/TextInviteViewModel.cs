using System;
using System.ComponentModel;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using AgentFramework.Core.Extensions;
using AgentFramework.Core.Messages.Connections;
using AgentFramework.Core.Utils;
using Newtonsoft.Json;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Utilities;
using ReactiveUI;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class TextInviteViewModel : ABaseViewModel
    {
        public TextInviteViewModel(IUserDialogs userDialogs, INavigationService navigationService) 
            : base(nameof(TextInviteViewModel), userDialogs, navigationService)
        {
        }
        
        public ICommand InputTextCommand => new Command(async () => await TextInviteAsync());

        private async Task TextInviteAsync()
        {
            ConnectionInvitationMessage invitation;

            try
            {
                invitation = MessageUtils.DecodeMessageFromUrlFormat<ConnectionInvitationMessage>(InputText);
            }
            catch (Exception)
            {
                DialogService.Alert("Invalid invitation!");
                Device.BeginInvokeOnMainThread(async () => await NavigationService.PopModalAsync());
                return;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                await NavigationService.NavigateBackAsync();
                await NavigationService.NavigateToAsync<AcceptInviteViewModel>(invitation, NavigationType.Modal);
            });
        }

        private string inputText;
        public string InputText
        {
            get => inputText;
            set
            {
                if (inputText != value)
                {
                    inputText = value;
                    UpdateJson();
                }
            }
        }
        
        private string _json;
        public string Json
        {
            get => _json;
            set => this.RaiseAndSetIfChanged(ref _json, value);
        }

        private void UpdateJson()
        {
            try
            {
                var startMsg = InputText.Head(@"/?c_i=") + @"/?c_i=";
                var invitation = MessageUtils.DecodeMessageFromUrlFormat<ConnectionInvitationMessage>(InputText);
                Json = $"{startMsg}\n\n{invitation.ToJson(new JsonSerializerSettings {Formatting = Formatting.Indented})}";
            }
            catch (Exception)
            {
                Json = $"Invalid ConnectionInvitation Message:\n\n{InputText}";
            }
        }
    }
}