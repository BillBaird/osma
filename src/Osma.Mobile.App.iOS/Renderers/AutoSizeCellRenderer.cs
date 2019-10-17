using Osma.Mobile.App.iOS.Renderers;
using Osma.Mobile.App.Views.Components;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly:ExportRenderer(typeof(RecordCell),   typeof(AutoSizeCellRenderer))]
[assembly:ExportRenderer(typeof(DetailedCell), typeof(AutoSizeCellRenderer))]
namespace Osma.Mobile.App.iOS.Renderers
{
    // Inspired by https://peterfoot.net/2017/12/08/listview-adventures-auto-sizing-uneven-rows/
    // https://github.com/inthehand/InTheHand.Forms/blob/master/InTheHand.Forms/InTheHand.Forms.Platform.iOS/AutoViewCellRenderer.cs
    // Note that this also has the side effect of keeping fields from not redrawing when the view is refreshed.  This is
    // a Xamarin bug so its nice to have this workaround.
    public class AutoSizeCellRenderer : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            if (item is ViewCell vc)
            {
                var sr = vc.View.Measure(tv.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

                if (vc.Height != sr.Request.Height)
                {
                    vc.ForceUpdateSize();

                    sr = vc.View.Measure(tv.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
                    vc.Height = sr.Request.Height;
                }
            }

            return base.GetCell(item, reusableCell, tv);
        }
    }
}