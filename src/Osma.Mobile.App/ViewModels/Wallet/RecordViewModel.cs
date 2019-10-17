using Acr.UserDialogs;
using AgentFramework.Core.Extensions;
using AgentFramework.Core.Models.Records;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Utilities;
using ReactiveUI;

namespace Osma.Mobile.App.ViewModels.Wallet
{
    public class RecordViewModel : ABaseViewModel
    {
        private static JsonSerializerSettings fmt = new JsonSerializerSettings {Formatting = Formatting.Indented};
        
        private readonly RecordBase _record;

        public RecordViewModel(IUserDialogs userDialogs,
            INavigationService navigationService,
            RecordBase record) 
            : base(
                nameof(RecordViewModel),
                userDialogs,
                navigationService
            )
        {
            _record = record;
            RecordType = _record.TypeName;
            Json = _record.ToJson(fmt);
            JsonTags = record.TagContents().AsJsonKeyValueObject();
        }
        
        #region Bindable Properties
        private string _recordType;
        public string RecordType
        {
            get => _recordType;
            set => this.RaiseAndSetIfChanged(ref _recordType, value);
        }

        private string _json;
        public string Json
        {
            get => _json;
            set => this.RaiseAndSetIfChanged(ref _json, value);
        }

        private string _jsonTags;
        public string JsonTags
        {
            get => _jsonTags;
            set => this.RaiseAndSetIfChanged(ref _jsonTags, value);
        }
        #endregion
    }
}