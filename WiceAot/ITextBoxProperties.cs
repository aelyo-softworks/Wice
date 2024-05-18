namespace Wice;

public interface ITextBoxProperties
{
    D2D1_TEXT_ANTIALIAS_MODE AntiAliasingMode { get; set; }
    D2D1_DRAW_TEXT_OPTIONS DrawOptions { get; set; }
    TextRenderingParameters TextRenderingParameters { get; set; }
}
