namespace SilkyUIFramework.BasicElements;

public abstract partial class BasicBody
{
    /// <summary>
    /// 不会影响鼠标位置的判定, 所以只建议用于开关 UI 的动画
    /// </summary>
    public virtual bool UseRenderTarget { get; set; } = false;
    public virtual float Opacity { get => field; set => field = Math.Clamp(value, 0f, 1f); } = 1f;

    public Matrix RenderTargetMatrix;

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {

        SDFRectangle.SampleVersion(BlurMakeSystem.BlurRenderTarget, Bounds.Position * 2f, Bounds.Size * 2f, BorderRadius * 2f, Matrix.Identity);
        base.Draw(gameTime, spriteBatch);
    }

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (UseRenderTarget)
        {
            UseRenderTargetDraw(gameTime, spriteBatch);
            return;
        }

        base.HandleDraw(gameTime, spriteBatch);
    }

    protected virtual void UseRenderTargetDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var device = Main.graphics.GraphicsDevice;

        var backBufferWidth = device.PresentationParameters.BackBufferWidth;
        var backBufferHeight = device.PresentationParameters.BackBufferHeight;
        var uiRenderTarget = RenderTargetPool.Instance.Rent(backBufferWidth, backBufferHeight);

        try
        {
            spriteBatch.End();

            var original = device.GetRenderTargets();

            var lastRenderTargetUsage = device.PresentationParameters.RenderTargetUsage;
            device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            var usageRecords = original.RecordUsage();

            device.SetRenderTarget(uiRenderTarget);
            device.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred,
                null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, SilkyUI.TransformMatrix);

            base.HandleDraw(gameTime, spriteBatch);

            spriteBatch.End();

            device.SetRenderTargets(original);
            usageRecords.RestoreUsage();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, RenderTargetMatrix);
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
            RenderTargetPool.Instance.Return(uiRenderTarget);
        }
    }
}
