namespace SilkyUIFramework.Graphics2D;

public enum BlurType { One, Two, Three, Four, Five }

public static class BlurHelper
{
    public static float[] GenerateGeometricSequence(int n, float radio)
    {
        if (n <= 0) return [];

        float[] result = new float[n];
        result[0] = 1;

        for (int i = 1; i < n; i++)
        {
            result[i] = result[i - 1] * radio;
        }

        return result;
    }

    public static void KawaseBlur(RenderTarget2D renderTarget,
        int iterationCount, float radio, BlurType blurType = BlurType.Three)
    {
        KawaseBlur(renderTarget, GenerateGeometricSequence(iterationCount, radio), blurType);
    }

    public static void KawaseBlur(RenderTarget2D renderTarget,
        float[] offsets, BlurType blurType = BlurType.Three)
    {
        if (offsets.Length == 0) return;

        var effect = ModAsset.BlurEffect.Value;
        if (effect == null) return;

        var device = Main.graphics.GraphicsDevice;
        var batch = Main.spriteBatch;

        var original = device.GetRenderTargets();
        var records = original.RecordUsage();

        batch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Matrix.Identity);

        var renderTargetSwap = RenderTargetPool.Instance.Rent(renderTarget.Width, renderTarget.Height);

        ModAsset.BlurEffect.Value.Parameters["uPixelSize"].SetValue(Vector2.One / new Vector2(renderTarget.Width, renderTarget.Height));

        SelectBlurEffectPasses(blurType, out var blurX, out var blurY);

        for (int i = 0; i < offsets.Length; i++)
        {
            device.SetRenderTarget(renderTargetSwap);

            effect.Parameters["uBlurRadius"].SetValue(offsets[i]);
            blurX.Apply();
            batch.Draw(renderTarget, Vector2.Zero, null, Color.White);

            device.SetRenderTarget(renderTarget);

            blurY.Apply();
            batch.Draw(renderTargetSwap, Vector2.Zero, null, Color.White);
        }

        batch.End();

        if (records is null)
        {
            device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            device.SetRenderTarget(null);
            device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;
        }
        else
        {
            device.SetRenderTargets(original);
            records.RestoreUsage();
        }

        RenderTargetPool.Instance.Return(renderTargetSwap);
    }

    public static void SelectBlurEffectPasses(BlurType blurType, out EffectPass blurX, out EffectPass blurY)
    {
        var effect = ModAsset.BlurEffect.Value;
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
    }
}
