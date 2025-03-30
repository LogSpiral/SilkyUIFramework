namespace SilkyUIFramework.Graphics2D;

public enum BlurType { One, Two, Three, Four, Five }

public static class BlurHelper
{
    public static void KawaseBlur(RenderTarget2D renderTarget,
        float maxOffset, float increment = 1, BlurType blurType = BlurType.Three)
    {
        if (maxOffset < increment) return;

        var effect = ModAsset.BlurEffect.Value;
        if (effect == null) return;

        var device = Main.graphics.GraphicsDevice;
        var batch = Main.spriteBatch;

        batch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Matrix.Identity);

        var renderTargetSwap = RenderTargetPool.Instance.Get(renderTarget.Width, renderTarget.Height);

        ModAsset.BlurEffect.Value.Parameters["uPixelSize"].SetValue(Vector2.One / new Vector2(renderTarget.Width, renderTarget.Height));


        EffectPass blurX;
        EffectPass blurY;

        switch (blurType)
        {
            default:
            case BlurType.One:
                blurX = effect.CurrentTechnique.Passes["BlurX1"];
                blurY = effect.CurrentTechnique.Passes["BlurY1"];
                break;
            case BlurType.Two:
                blurX = effect.CurrentTechnique.Passes["BlurX2"];
                blurY = effect.CurrentTechnique.Passes["BlurY2"];
                break;
            case BlurType.Three:
                blurX = effect.CurrentTechnique.Passes["BlurX3"];
                blurY = effect.CurrentTechnique.Passes["BlurY3"];
                break;
            case BlurType.Four:
                blurX = effect.CurrentTechnique.Passes["BlurX4"];
                blurY = effect.CurrentTechnique.Passes["BlurY4"];
                break;
            case BlurType.Five:
                blurX = effect.CurrentTechnique.Passes["BlurX5"];
                blurY = effect.CurrentTechnique.Passes["BlurY5"];
                break;
        }

        for (float i = 0; i < maxOffset; i = MathF.Round(i + increment, 2))
        {
            device.SetRenderTarget(renderTargetSwap);
            effect.Parameters["uBlurRadius"].SetValue(maxOffset - i);
            blurX.Apply();
            batch.Draw(renderTarget, Vector2.Zero, null, Color.White);

            device.SetRenderTarget(renderTarget);

            blurY.Apply();
            batch.Draw(renderTargetSwap, Vector2.Zero, null, Color.White);
        }

        batch.End();

        RenderTargetPool.Instance.Return(renderTargetSwap);
    }
}
