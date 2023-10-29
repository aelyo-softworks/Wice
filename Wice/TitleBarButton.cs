using System;
using System.ComponentModel;
using DirectN;
using Wice.Utilities;

namespace Wice
{
    public class TitleBarButton : ButtonBase
    {
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

            // this is a starting point so it's not zero but ultimate size should be computed by TitleBar
            Path.Width = 10;
            Path.Height = 10;
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

        protected override void OnArranged(object sender, EventArgs e)
        {
            base.OnArranged(sender, e);
            var size = (Path.ArrangedRect - Path.Margin).Size;
            var geoSize = size.height;
            var geoSource = Application.Current.ResourceManager.GetTitleBarButtonGeometrySource(ButtonType, geoSize);
            if (geoSource.Equals(_lastGeometrySource2D))
                return;

            Path.GeometrySource2D = geoSource.GetIGeometrySource2();
            _lastGeometrySource2D = geoSource;
        }

        protected override void OnAttachedToComposition(object sender, EventArgs e)
        {
            base.OnAttachedToComposition(sender, e);
            Path.Shape.StrokeThickness = 1f;
            Path.StrokeBrush = Compositor.CreateColorBrush(_D3DCOLORVALUE.Black.ToColor());
        }
    }
}

