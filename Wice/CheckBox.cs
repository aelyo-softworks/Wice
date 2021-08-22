using System;
using System.ComponentModel;
using DirectN;

namespace Wice
{
    public class CheckBox : StateButton
    {
        public CheckBox()
        {
            AddState(new StateButtonState(false, CreateChild) { EqualsFunc = (s, o) => !Conversions.ChangeType<bool>(o) });
            AddState(new StateButtonState(true, CreateChild) { EqualsFunc = (s, o) => Conversions.ChangeType<bool>(o) });
            Value = false;
        }

        [Category(CategoryBehavior)]
        public new bool Value { get => (bool)base.Value; set => base.Value = value; }

        public static Visual CreateDefaultTrueVisual()
        {
            var box = new Box();

            var path = new Path
            {
                StrokeThickness = Application.CurrentTheme.BorderSize / 2,
            };
            
            box.Arranged += (s, e) =>
            {
                var geoSource = Application.Current.ResourceManager.GetCheckButtonGeometrySource(box.ArrangedRect.Width, box.ArrangedRect.Height);
                path.GeometrySource2D = geoSource.GetIGeometrySource2();
            };

            box.AttachedToComposition += (s, e) =>
            {
                box.RenderBrush = box.Compositor.CreateColorBrush(Application.CurrentTheme.SelectedColor);
                path.StrokeBrush = box.Compositor.CreateColorBrush(Application.CurrentTheme.UnselectedColor);
            };

            box.Children.Add(path);
#if DEBUG
            box.Name = nameof(CheckBox) + ".true";
#endif
            return box;
        }

        public static Visual CreateDefaultFalseVisual()
        {
            var rect = new Rectangle
            {
                StrokeThickness = Application.CurrentTheme.BorderSize,
            };

            rect.AttachedToComposition += (s, e) =>
            {
                rect.StrokeBrush = rect.Compositor.CreateColorBrush(Application.CurrentTheme.BorderColor);
            };
#if DEBUG
            rect.Name = nameof(CheckBox) + ".false";
#endif

            return rect;
        }

        protected virtual Visual CreateChild(StateButton box, EventArgs e, StateButtonState state) => true.Equals(state.Value) ? CreateDefaultTrueVisual() : CreateDefaultFalseVisual();
    }
}
