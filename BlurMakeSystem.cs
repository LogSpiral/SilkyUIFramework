namespace SilkyUIFramework;

/// <summary>
/// 模糊效果处理系统，继承自ModSystem
/// 负责创建和管理模糊渲染目标，并在UI层最底层添加模糊效果
/// </summary>
internal class BlurMakeSystem : ModSystem
{
    public static int IterationCount { get; set; }
    public static float IterationOffsetMultiplier { get; set; }
    public static BlurType BlendType { get; set; }

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
        // 获取当前屏幕尺寸
        var size = new Vector2(Main.screenTarget.Width, Main.screenTarget.Height);

        // 如果模糊渲染目标为空或尺寸不匹配，则重新创建
        if (BlurRenderTarget is null || BlurRenderTarget.Width != size.X || BlurRenderTarget.Height != size.Y)
        {
            BlurRenderTarget?.Dispose(); // 释放旧的渲染目标
            // 创建新的渲染目标，使用颜色表面格式，无深度缓冲
            BlurRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, (int)size.X, (int)size.Y, false, SurfaceFormat.Color, DepthFormat.None);
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

            // 记录当前渲染目标状态
            var original = device.GetRenderTargets();

            // 设置模糊渲染目标并开始绘制
            device.SetRenderTarget(BlurRenderTarget);
            batch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Matrix.Identity);
            // 将当前屏幕内容绘制到模糊渲染目标
            batch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            batch.End();

            // 恢复之前的渲染目标状态
            if (original is null || original.Length == 0)
            {
                // 如果没有记录，则设置为空渲染目标
                device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
                device.SetRenderTarget(null);
                device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;
            }
            else
            {
                var records = original.RecordUsage();
                device.SetRenderTargets(original);
                records.RestoreUsage();
            }

            // 应用Kawase模糊算法，迭代4次，模糊强度2f，使用Three模糊类型
            BlurHelper.KawaseBlur(BlurRenderTarget, 3, 2f, BlurType.Three);

            // 重新开始批处理，为后续绘制做准备
            batch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Matrix.Identity);
            batch.Draw(BlurRenderTarget, Vector2.Zero, Color.White * 0.75f);

            return true;
        }
    }
}