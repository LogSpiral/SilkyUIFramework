namespace SilkyUIFramework.Elements;

public abstract partial class BaseBody
{
    public virtual bool UseRenderTarget { get; set; } = false;
    public virtual float Opacity { get; set => field = Math.Clamp(value, 0f, 1f); } = 1f;

    public Matrix RenderTargetMatrix;

    public virtual bool EnableBlur { get; set; } = false;
    public virtual Bounds BlurBounds => Bounds;
    public virtual Vector4 BlurBorderRadius => BorderRadius;

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (UseRenderTarget) UseRenderTargetDraw(gameTime, spriteBatch);
        else base.HandleDraw(gameTime, spriteBatch);
    }

    protected virtual void UseRenderTargetDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var device = Main.graphics.GraphicsDevice;

        var backBufferWidth = device.PresentationParameters.BackBufferWidth;
        var backBufferHeight = device.PresentationParameters.BackBufferHeight;
        var uiRenderTarget = RenderTargetPool.Instance.Rent(backBufferWidth, backBufferHeight);

        RuntimeSafeHelper.SafeInvoke(delegate
        {
            spriteBatch.End();

            var original = device.GetRenderTargets(); device.SetRenderTarget(uiRenderTarget); device.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, SilkyUI.TransformMatrix);

            base.HandleDraw(gameTime, spriteBatch); spriteBatch.End(); device.RestoreRenderTargets(original);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, RenderTargetMatrix);
            spriteBatch.Draw(uiRenderTarget, Vector2.Zero, null, Color.White * Opacity, 0f, Vector2.Zero, Vector2.One, 0, 0);
        });

        RenderTargetPool.Instance.Return(uiRenderTarget);
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (EnableBlur && BlurMakeSystem.BlurAvailable && !Main.gameMenu)
        {
            if (BlurMakeSystem.SingleBlur)
            {
                spriteBatch.End(); BlurMakeSystem.KawaseBlur();
                spriteBatch.Begin(0, null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, SilkyUI.TransformMatrix);
            }

            var scale = Main.UIScale; var bounds = BlurBounds;

            var borderRadius = BlurBorderRadius * scale;
            var position = bounds.Position * scale; var size = bounds.Size * scale;

            SDFRectangle.SampleVersion(BlurMakeSystem.BlurRenderTarget, position, size, borderRadius, Matrix.Identity);
        }

        base.Draw(gameTime, spriteBatch);
    }
}