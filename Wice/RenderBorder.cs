using System.ComponentModel;
using System.Linq;
using DirectN;

namespace Wice
{
    public abstract class RenderBorder : RenderVisual, IOneChildParent
    {
        protected override BaseObjectCollection<Visual> CreateChildren() => new BaseObjectCollection<Visual>(1);

        [Browsable(false)]
        public Visual Child
        {
            get => Children?.FirstOrDefault();
            set
            {
                var child = Child;
                if (child == value)
                    return;

                if (child != null)
                {
                    Children.Remove(child);
                }
                Children.Add(value);
            }
        }

        protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint)
        {
            var child = Child;
            if (child != null)
            {
                child.Measure(constraint);
                return child.DesiredSize;
            }

            return base.MeasureCore(constraint);
        }

        protected override void ArrangeCore(D2D_RECT_F finalRect)
        {
            var child = Child;
            if (child != null)
            {
                child.Arrange(new D2D_RECT_F(finalRect.Size));
            }
        }
    }
}
