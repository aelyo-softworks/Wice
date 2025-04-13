using System;
using System.Linq;
using DirectN;
using Wice.Utilities;

namespace Wice.Samples.Gallery.Samples.Misc.DragDrop
{
    public class DropTargetSample : Sample
    {
        public override string Description => "A visual that will display the paths of the files that will be drag & dropped onto it.";

        public override void Layout(Visual parent)
        {
            var stack = new Stack { Orientation = Orientation.Horizontal };
            Dock.SetDockType(stack, DockType.Top); // remove from display
            parent.Children.Add(stack); // remove from display

            var box = new Border
            {
                RenderBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Green.ToColor()),
                Width = 400, // remove from display
                Height = 200, // remove from display
                AllowDrop = true, // enable the visual as a drop target
            };
            stack.Children.Add(box);

            var defaultText = "Drag and drop files here";
            var tb = new TextBox
            {
                Text = defaultText,
                HorizontalAlignment = Alignment.Center, // remove from display
                VerticalAlignment = Alignment.Center, // remove from display
                WordWrapping = DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WHOLE_WORD, // remove from display
            };
            box.Child = tb;
            // remove from display
            var info = new Stack { Orientation = Orientation.Vertical }; // remove from display
            stack.Children.Add(info); // remove from display
                                      // remove from display
            var type = new TextBox { VerticalAlignment = Alignment.Near, Padding = 10 }; // remove from display
            info.Children.Add(type); // remove from display
                                     // remove from display
            var point = new TextBox { VerticalAlignment = Alignment.Near, Padding = 10 }; // remove from display
            info.Children.Add(point); // remove from display
                                      // remove from display
            var flags = new TextBox { VerticalAlignment = Alignment.Near, Padding = 10 }; // remove from display
            info.Children.Add(flags); // remove from display
                                      // remove from display
            box.DragDrop += (s, e) =>
            {
                switch (e.Type)
                {
                    case DragDropEventType.Leave:
                        tb.Text = defaultText;
                        type.Text = string.Empty; // remove from display
                        point.Text = string.Empty; // remove from display
                        flags.Text = string.Empty; // remove from display
                        break;

                    case DragDropEventType.Enter:
                    case DragDropEventType.Over:
                    case DragDropEventType.Drop:
                    default:
                        type.Text = $"Operation: {e.Type}"; // remove from display
                        point.Text = $"Point: {e.Point.x} {e.Point.y}"; // remove from display
                        flags.Text = $"Keys: {e.KeyFlags}"; // remove from display
                        if (e.DataObject != null)
                        {
                            var dataObject = new System.Windows.Forms.DataObject(e.DataObject);
                            if (dataObject.ContainsFileDropList())
                            {
                                tb.Text = string.Join(Environment.NewLine, dataObject.GetFileDropList().Cast<string>());
                            }
                        }
                        break;
                }
            };
        }
    }
}