using System;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Collections.ListBox
{
    public class ListBoxSample : Sample
    {
        public override string Description => "A list box using a complex data source and a custom item rendering.";
        public override int SortOrder => 1;

        public override void Layout(Visual parent)
        {
            var lb = new Wice.ListBox();
            lb.RenderBrush = Compositor.CreateColorBrush(D3DCOLORVALUE.White.ToColor());
            parent.Children.Add(lb);
            Dock.SetDockType(lb, DockType.Top); // remove from display

            lb.DataBinder = new DataBinder
            {
                // called when an item visual (container) is being created
                DataItemVisualCreator = (ctx) =>
                {
                    var tb = new TextBox();
                    tb.Height = 20;
                    ctx.DataVisual = tb;
                },
                // called when an item visual (container) is being bound to the data
                DataItemVisualBinder = (ctx) =>
                {
                    var tb = (TextBox)ctx.DataVisual;
                    var data = (Tuple<string, string>)ctx.Data;
                    tb.Text = data.Item1;
                    tb.SetSolidColor(D3DCOLORVALUE.FromName(data.Item1));
                    tb.BackgroundColor = D3DCOLORVALUE.FromName(data.Item2);
                }
            };

            // set the list box's data source
            lb.DataSource = new[] {
                new Tuple<string, string>("blue", "white"),
                new Tuple<string, string>("white", "black"),
                new Tuple<string, string>("red", "white")
            };
        }
    }
}
