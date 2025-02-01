namespace SilkyUIFramework.Extensions;

public static class SpriteBatchExtensions
{
    /// <summary>
    /// SpriteSortMode       - 精灵排序模式<br/>
    /// BlendState           - 混合状态<br/>
    /// SamplerState         - 采样器状态<br/>
    /// DepthStencilState    - 深度模板状态<br/>
    /// RasterizerState      - 光栅化状态 (目前知道的的作用是裁剪 Begin-End 所有贴图)<br/>
    /// Effect               - 着色器<br/>
    /// Matrix               - 矩阵
    /// </summary>
    public static void ReBegin(this SpriteBatch sb, Effect effect, Matrix matrix)
    {
        sb.End();
        sb.Begin(0, sb.GraphicsDevice.BlendState, sb.GraphicsDevice.SamplerStates[0],
            sb.GraphicsDevice.DepthStencilState, sb.GraphicsDevice.RasterizerState, effect, matrix);
    }
}
