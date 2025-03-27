namespace SilkyUIFramework.BasicElements;

public abstract partial class BasicBody
{
    public virtual bool UseRenderTarget { get; set; } = false;
    public virtual float Opacity { get => field; set => field = Math.Clamp(value, 0f, 1f); } = 1f;

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var pool = RenderTargetPool.Instance;
        var device = Main.graphics.GraphicsDevice;

        // 不使用独立画布
        if (!UseRenderTarget)
        {
            base.HandleDraw(gameTime, spriteBatch);
            return;
        }

        var original = device.GetRenderTargets();
        var usageRecords = original.RecordUsage();
        var lastRenderTargetUsage = device.PresentationParameters.RenderTargetUsage;
        device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

        // 画布大小计算
        var canvasSize = GraphicsDeviceHelper.GetBackBufferSize();
        var uiRenderTarget = pool.Get((int)canvasSize.Width, (int)canvasSize.Height);
        try
        {
            device.SetRenderTarget(uiRenderTarget);
            device.Clear(Color.Transparent);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred,
                null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, SilkyUI.TransformMatrix);

            base.HandleDraw(gameTime, spriteBatch);

            spriteBatch.End();

            device.SetRenderTargets(original);
            usageRecords.Restore();

            spriteBatch.Begin();
            spriteBatch.Draw(uiRenderTarget, Vector2.Zero, null,
                Color.White * Opacity, 0f, Vector2.Zero, Vector2.One, 0, 0);

            device.PresentationParameters.RenderTargetUsage = lastRenderTargetUsage;
        }
        catch (Exception e)
        {
            SilkyUIFramework.Instance.Logger.Error($"{GetType().FullName} error: {e}");
            throw;
        }
        finally
        {
            pool.Return(uiRenderTarget);
        }
    }
}
