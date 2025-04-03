namespace SilkyUIFramework;

/// <summary>
/// 模糊效果处理系统，继承自ModSystem
/// 负责创建和管理模糊渲染目标，并在UI层最底层添加模糊效果
/// </summary>
internal class BlurMakeSystem : ModSystem
{
    public static bool EnableBlur { get; set; } = true;
    public static float BlurZoomMultiplierDenominator
    {
        get; set => field = Math.Max(value, 1f);
    } = 2f;

    /// <summary>
    /// 模糊迭代次数
    /// </summary>
    public static int BlurIterationCount { get; set; } = 3;

    /// <summary>
    /// 模糊偏移乘数
    /// </summary>
    public static float IterationOffsetMultiplier { get; set; } = 2f;
    public static BlurMixingNumber BlurMixingNumber { get; set; } = BlurMixingNumber.Three;

    /// <summary>
    /// 静态模糊渲染目标，用于存储模糊后的画面
    /// </summary>
    public static RenderTarget2D BlurRenderTarget { get; private set; }
    public override void Unload() => BlurRenderTarget?.Dispose();

    /// <summary>
    /// 修改界面层的方法，在界面层列表最前面插入模糊层
    /// </summary>
    /// <param name="layers">游戏界面层列表</param>
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (!EnableBlur) return;

        var width = (int)Math.Ceiling(Main.screenTarget.Width / BlurZoomMultiplierDenominator);
        var height = (int)Math.Ceiling(Main.screenTarget.Height / BlurZoomMultiplierDenominator);
        // 如果模糊渲染目标为空或尺寸不匹配，则重新创建
        if (BlurRenderTarget is null || BlurRenderTarget.Width != width || BlurRenderTarget.Height != height)
        {
            BlurRenderTarget?.Dispose(); // 释放旧的渲染目标
            // 创建新的渲染目标，使用颜色表面格式，无深度缓冲
            BlurRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        // 在层列表最前面插入模糊层
        layers.Insert(0, new BlurMakeLayer(BlurRenderTarget));
    }

    /// <summary>
    /// 模糊效果层实现类
    /// </summary>
    public class BlurMakeLayer(RenderTarget2D blurRenderTarget) : GameInterfaceLayer("BlurMakeLayer", InterfaceScaleType.UI)
    {
        // 模糊渲染目标属性
        public RenderTarget2D BlurRenderTarget { get; } = blurRenderTarget;

        /// <summary>
        /// 绘制方法，实现模糊效果
        /// </summary>
        /// <returns>总是返回 true，表示绘制成功</returns>
        public override bool DrawSelf()
        {
            var batch = Main.spriteBatch;
            batch.End(); // 结束当前批处理

            var device = Main.graphics.GraphicsDevice;

            var original = device.GetRenderTargets();
            device.SetRenderTarget(BlurRenderTarget);

            batch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Matrix.Identity);
            batch.Draw(Main.screenTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, BlurRenderTarget.Size() / Main.screenTarget.Size(), 0, 0f);
            batch.End();

            original.RestoreRenderTargets(device);

            BlurHelper.KawaseBlur(BlurRenderTarget, BlurIterationCount, BlurZoomMultiplierDenominator, BlurMixingNumber);

            // 重新开始批处理，为后续绘制做准备
            batch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Matrix.Identity);

            return true;
        }
    }
}