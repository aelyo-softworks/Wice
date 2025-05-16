namespace Wice.Tests;

public class Border2 : Border
{
    protected override void RenderCore(RenderContext context)
    {
        base.RenderCore(context);
        var rc = RelativeRenderRect;
        rc = rc.Deflate(2);
        Application.Trace(this + " rc:" + rc);
        context.DeviceContext.Object.DrawRectangle(ref rc, context.CreateSolidColorBrush(_D3DCOLORVALUE.Black).Object, 5, null);
    }
}
