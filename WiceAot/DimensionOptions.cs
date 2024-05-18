namespace Wice;

[Flags]
public enum DimensionOptions
{
    Manual = 0x0,
    Width = 0x1,
    Height = 0x2,
    WidthAndHeight = Width | Height,
}
