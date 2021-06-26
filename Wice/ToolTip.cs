using System;
using System.ComponentModel;
using DirectN;
using Windows.UI.Composition;

namespace Wice
{
    public class ToolTip : PopupWindow, IContentParent
    {
        public ToolTip()
        {
            PlacementMode = PlacementMode.Mouse;
            MeasureToContent = DimensionOptions.WidthAndHeight;
            FrameSize = 0;

            // this should be transparent too
            Content = CreateContent();
            if (Content == null)
                throw new InvalidOperationException();

            Children.Add(Content);

            // we need a margin for the drop shadow so we need to force 1 child only
            Content.Margin = Application.CurrentTheme.ToolTipBaseSize;
            Content.RenderShadow = CreateShadow(); // may be null if overriden

            VerticalOffset = Application.CurrentTheme.ToolTipVerticalOffset * NativeWindow.GetAccessibilityCursorSize();
        }

        [Browsable(false)]
        public Visual Content { get; }
        
        protected override int MaxChildrenCount => 1;
        protected override bool HasCaret => false;

        protected virtual Visual CreateContent()
        {
            var canvas = new Canvas();
            canvas.MeasureToContent = DimensionOptions.WidthAndHeight;
            return canvas;
        }

        public override bool Show(SW command = SW.SW_SHOWNOACTIVATE) => base.Show(command);
        protected override MA OnMouseActivate(IntPtr parentWindowHandle, int mouseMessage, HT hitTest) => MA.MA_NOACTIVATE;
        protected override void ExtendFrame(IntPtr handle)
        {
            // don't extend frame
            //base.ExtendFrame(handle);
        }

        protected virtual CompositionShadow CreateShadow()
        {
            if (Compositor == null)
                throw new InvalidOperationException();

            var shadow = Compositor.CreateDropShadow();
            shadow.BlurRadius = Application.CurrentTheme.ToolTipShadowBlurRadius;
            return shadow;
        }

        protected override PlacementParameters CreatePlacementParameters()
        {
            var parameters = base.CreatePlacementParameters();
            var offset = Content.Margin;
            parameters.HorizontalOffset -= offset.left;
            parameters.VerticalOffset -= offset.top;
            return parameters;
        }
    }
}
