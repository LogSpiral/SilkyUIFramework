namespace SilkyUIFramework.Helper;

public static class MatrixHelper
{
    /// <summary>
    /// <see cref="SpriteBatch"/> 矩阵变换为 SDF 矩阵
    /// </summary>
    public static void Transform2SDFMatrix(ref Matrix matrix)
    {
        GetRenderTargetSize(out var width, out var height);

        matrix *= Matrix.CreateScale(2f / width, -2f / height, 1f);
        matrix *= Matrix.CreateTranslation(-1 + matrix.M41 * 2f / width, 1 - matrix.M42 * 2f / height, 0f);

        //matrix.M11 *= 2f / width;
        //matrix.M22 *= -2f / height;
        //matrix.M41 = -1 + matrix.M41 * 2f / width;
        //matrix.M42 = 1 - matrix.M42 * 2f / height;
    }

    /// <summary>
    /// 获取当前渲染目标的大小
    /// </summary>
    public static void GetRenderTargetSize(out int width, out int height)
    {
        var graphicsDevice = Main.graphics.GraphicsDevice;

        var renderTargets = graphicsDevice.GetRenderTargets();

        if (renderTargets.Length > 0 && renderTargets[0].RenderTarget is RenderTarget2D renderTarget)
        {
            width = renderTarget.Width;
            height = renderTarget.Height;
        }
        else
        {
            width = graphicsDevice.PresentationParameters.BackBufferWidth;
            height = graphicsDevice.PresentationParameters.BackBufferHeight;
        }
    }
}
