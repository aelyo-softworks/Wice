using DirectN;

namespace Wice
{
    public class SolidColorBrush : Brush
    {
        public SolidColorBrush(_D3DCOLORVALUE color)
        {
            Color = color;
        }

        public _D3DCOLORVALUE Color { get; }

        protected internal override IComObject<ID2D1Brush> GetBrush(RenderContext context) => context.CreateSolidColorBrush(Color);

        public override bool Equals(Brush other) => other is SolidColorBrush brush && Color == brush.Color;
        public override int GetHashCode() => Color.GetHashCode();
        public override string ToString() => Color.ToString();

        public static implicit operator SolidColorBrush(_D3DCOLORVALUE color) => new SolidColorBrush(color);
    }
}
