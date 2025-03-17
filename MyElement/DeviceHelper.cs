namespace SilkyUIFramework.MyElement;

public static class DeviceHelper
{
    public static Size GetViewportSizeByUIScale()
    {
        return Main.graphics.GraphicsDevice.Viewport.Bounds.Size() / Main.UIScale;
    }
}
