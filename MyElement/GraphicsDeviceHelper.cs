namespace SilkyUIFramework.MyElement;

public static class GraphicsDeviceHelper
{
    public static Size GetViewportSizeByUIScale() =>
        Main.graphics.GraphicsDevice.Viewport.Bounds.Size() / Main.UIScale;
}
