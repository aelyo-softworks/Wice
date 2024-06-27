using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using DirectN;
using Wice.Utilities;

namespace Wice
{
    public class TitleBarButton : ButtonBase
    {
        private const float _strokeThickness = 1;

        private TitleBarButtonType _buttonType;
        private GeometrySource2D _lastGeometrySource2D;

        public TitleBarButton()
        {
            Path = CreatePath();
            if (Path == null)
                throw new InvalidOperationException();

#if DEBUG
            Path.Name = nameof(Path);
#endif
            Children.Add(Path);
        }

        [Category(CategoryLayout)]
        public Path Path { get; }

        [Category(CategoryBehavior)]
        public TitleBarButtonType ButtonType
        {
            get => _buttonType;
            set
            {
                if (_buttonType == value)
                    return;

                _buttonType = value;
                Path.Name = nameof(Path) + _buttonType;
            }
        }

        protected virtual Path CreatePath() => new Path();

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            var window = Window;
            if (window != null)
            {
                var dpiSize = GetDpiAdjustedCaptionButtonSize(window);
                var height = dpiSize.height / 3;
                Path.Height = height;
                Path.Width = height;
            }

            var size = base.MeasureCore(constraint);
            return size;
        }

        protected override void OnArranged(object sender, EventArgs e)
        {
            base.OnArranged(sender, e);
            var size = Path.ArrangedRect;
            var geoSize = size.Height;
            var geoSource = Application.Current.ResourceManager.GetTitleBarButtonGeometrySource(ButtonType, geoSize);
            if (geoSource.Equals(_lastGeometrySource2D))
                return;

            Path.GeometrySource2D = geoSource.GetIGeometrySource2();
            _lastGeometrySource2D = geoSource;
        }

        protected override void OnAttachedToComposition(object sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            Path.Shape.StrokeThickness = _strokeThickness;
            Path.StrokeBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Black.ToColor());
        }

        internal static tagSIZE GetDpiAdjustedCaptionButtonSize(Window window)
        {
            var bounds = new tagRECT();
            // this is dpi-adjusted but only when called *after* some message like SHOWWINDOW or NCPAINT (not sure)
            WindowsFunctions.DwmGetWindowAttribute(window.Handle, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_BUTTON_BOUNDS, ref bounds, Marshal.SizeOf<tagRECT>());
            var height = bounds.Height;
            height = AdjustForMonitorDpi(window, height, true);
            if (_strokeThickness % 2 == 1 && height % 2 == 1)
            {
                height++;
            }

            // we have 3 buttons. not sure this is always ok...
            var width = (bounds.Width - 1) / 3;
            width = AdjustForMonitorDpi(window, width, true);
            return new tagSIZE(width, height);
        }

        private static int AdjustForMonitorDpi(Window window, int value, bool reduce = false)
        {
            var dpi = window.Monitor?.EffectiveDpi.width;
            if (!dpi.HasValue)
            {
                dpi = (uint)window.Dpi;
            }

            if (dpi == 96)
                return value;

            if (reduce)
                return (int)(value * 96 / dpi.Value);

            return (int)(value * dpi.Value / 96);
        }
    }
}

