using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Osma.Mobile.App.Views.Components
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecordCell : ViewCell
    {
        public RecordCell()
        {
            InitializeComponent();
        }
        
        public static readonly BindableProperty RecordTypeProperty =
            BindableProperty.Create(nameof(RecordType), typeof(string), typeof(RecordCell), "", propertyChanged: RecordTypePropertyChanged);
        
        public string RecordType
        {
            get => (string)GetValue(RecordTypeProperty);
            set => SetValue(RecordTypeProperty, value);
        }

        static void RecordTypePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            RecordCell cell = (RecordCell)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                cell.RecordTypeLabel.Text = newValue?.ToString();
            });
        }
        
        public static readonly BindableProperty JsonProperty =
            BindableProperty.Create(nameof(Json), typeof(string), typeof(RecordCell), "", propertyChanged: JsonPropertyChanged);

        public string Json
        {
            get => (string)GetValue(JsonProperty);
            set => SetValue(JsonProperty, value);
        }

        static void JsonPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            RecordCell cell = (RecordCell)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                cell.JsonLabel.Text = newValue?.ToString();
            });
        }
        
        public static readonly BindableProperty JsonTagsProperty =
            BindableProperty.Create(nameof(JsonTags), typeof(string), typeof(RecordCell), "", propertyChanged: JsonTagsPropertyChanged);
        
        public string JsonTags
        {
            get => (string)GetValue(JsonTagsProperty);
            set => SetValue(JsonTagsProperty, value);
        }

        static void JsonTagsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            RecordCell cell = (RecordCell)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                cell.JsonTagsLabel.Text = newValue?.ToString();
            });
        }
    }
}