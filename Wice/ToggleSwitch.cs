using System;
using System.ComponentModel;
using System.Numerics;
using DirectN;
using Wice.Utilities;
using Windows.UI.Composition;

namespace Wice
{
    public class ToggleSwitch : ButtonBase, IValueable
    {
        public static VisualProperty ValueProperty = VisualProperty.Add<bool>(typeof(ToggleSwitch), nameof(Value), VisualPropertyInvalidateModes.Render);
        public static VisualProperty OnButtonBrushProperty = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OnButtonBrush), VisualPropertyInvalidateModes.Render);
        public static VisualProperty OffButtonBrushProperty = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OffButtonBrush), VisualPropertyInvalidateModes.Render);
        public static VisualProperty OnPathBrushProperty = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OnPathBrush), VisualPropertyInvalidateModes.Render);
        public static VisualProperty OffPathBrushProperty = VisualProperty.Add<CompositionBrush>(typeof(ToggleSwitch), nameof(OffPathBrush), VisualPropertyInvalidateModes.Render);

        private readonly Canvas _canvas = new Canvas();
        private readonly Path _path;
        private readonly Ellipse _button = new Ellipse();
        private EventHandler<ValueEventArgs> _valueChanged;

        event EventHandler<ValueEventArgs> IValueable.ValueChanged { add { UIExtensions.AddEvent(ref _valueChanged, value); } remove { UIExtensions.RemoveEvent(ref _valueChanged, value); } }
        public event EventHandler<ValueEventArgs<bool>> ValueChanged;

        bool IValueable.CanChangeValue { get => IsEnabled; set => IsEnabled = value; }
        object IValueable.Value => Value;
        bool IValueable.TrySetValue(object value)
        {
            if (value is bool b)
            {
                Value = b;
                return true;
            }

            if (Conversions.TryChangeType(value, out b))
            {
                Value = b;
                return true;
            }
            return false;
        }

        public ToggleSwitch()
        {
            Width = Application.CurrentTheme.BoxSize * 2;
            Height = Application.CurrentTheme.BoxSize;

            _path = new Path();

            _button.HorizontalAlignment = Alignment.Near;

            _canvas.Arranged += (s, e) =>
            {
                _path.GeometrySource2D = Application.Current.ResourceManager.GetToggleSwitchGeometrySource(_canvas.ArrangedRect.Width - _path.StrokeThickness, _canvas.ArrangedRect.Height - _path.StrokeThickness, _path.StrokeThickness / 2);

                var radius = _canvas.ArrangedRect.Height / 2 - _path.StrokeThickness * 2.5f;
                _button.Radius = new Vector2(radius, radius);

                var thickness = 0f;
                var ratio = Application.CurrentTheme.ToggleBorderRatio;
                if (ratio <= 0)
                {
                    thickness = 0;
                }
                else
                {
                    thickness = _button.Height * ratio;
                }

                _path.StrokeThickness = Math.Max(0.5f, thickness);
            };

            _canvas.Children.Add(_path);
            _canvas.Children.Add(_button);

#if DEBUG
            _canvas.Name = nameof(ToggleSwitch) + nameof(_canvas);
            _path.Name = nameof(_path);
            _button.Name = nameof(_button);
#endif
            Child = _canvas;
            var on = WindowsUtilities.LoadString("shell32.dll", 50225);
            var off = WindowsUtilities.LoadString("shell32.dll", 50224);

            ToolTipContentCreator = tt => Window.CreateDefaultToolTipContent(tt, Value ? on : off);
        }

        [Category(CategoryBehavior)]
        public bool Value { get => (bool)GetPropertyValue(ValueProperty); set => SetPropertyValue(ValueProperty, value); }

        [Category(CategoryRender)]
        public CompositionBrush OnButtonBrush { get => (CompositionBrush)GetPropertyValue(OnButtonBrushProperty); set => SetPropertyValue(OnButtonBrushProperty, value); }

        [Category(CategoryRender)]
        public CompositionBrush OffButtonBrush { get => (CompositionBrush)GetPropertyValue(OffButtonBrushProperty); set => SetPropertyValue(OffButtonBrushProperty, value); }

        [Category(CategoryRender)]
        public CompositionBrush OnPathBrush { get => (CompositionBrush)GetPropertyValue(OnPathBrushProperty); set => SetPropertyValue(OnPathBrushProperty, value); }

        [Category(CategoryRender)]
        public CompositionBrush OffPathBrush { get => (CompositionBrush)GetPropertyValue(OffPathBrushProperty); set => SetPropertyValue(OffPathBrushProperty, value); }

        protected override void OnClick(object sender, EventArgs e)
        {
            Value = !Value;
            base.OnClick(sender, e);
        }

        protected override void Render()
        {
            if (Value)
            {
                _path.StrokeBrush = null;
                _path.FillBrush = OnPathBrush ?? Compositor.CreateColorBrush(Application.CurrentTheme.SelectedColor);
                _button.FillBrush = OnButtonBrush ?? Compositor.CreateColorBrush(Application.CurrentTheme.UnselectedColor);
            }
            else
            {
                var path = OffPathBrush;
                var button = OffButtonBrush;
                if (path == null)
                {
                    if (button == null)
                    {
                        button = Compositor.CreateColorBrush(Application.CurrentTheme.BorderColor);
                    }
                    path = button;
                }
                else if (button == null)
                {
                    button = path;
                }

                _path.StrokeBrush = path;
                _path.FillBrush = null;
                _button.FillBrush = button;
            }
            base.Render();
        }

        protected virtual void OnValueChanged(object sender, ValueEventArgs<bool> e)
        {
            ValueChanged?.Invoke(sender, e);
            _valueChanged?.Invoke(sender, e);
        }

        protected override bool SetPropertyValue(BaseObjectProperty property, object value, BaseObjectSetOptions options = null)
        {
            if (!base.SetPropertyValue(property, value, options))
                return false;

            if (property == ValueProperty)
            {
                _button.HorizontalAlignment = (bool)value ? Alignment.Far : Alignment.Near;
                OnValueChanged(this, new ValueEventArgs<bool>((bool)value));
            }
            else if (property == HeightProperty)
            {
                // ellipse has no size by itself, if we want to be able to use HorizontalAlignment, we need to set it
                _button.Width = (float)value;
                _button.Height = (float)value;
            }
            return true;
        }
    }
}
