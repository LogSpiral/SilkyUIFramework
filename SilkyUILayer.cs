using static Terraria.UI.InterfaceScaleType;

namespace SilkyUIFramework;

/// <summary>
/// Game UI Layer
/// </summary>
public class SilkyUILayer : GameInterfaceLayer
{
    public SilkyUI SilkyUI { get; }

    public event Action PreDraw;
    public event Action PostDraw;

    public SilkyUILayer(SilkyUI silkyUI, string name, InterfaceScaleType scaleType,
        Action onPreDraw = null, Action onPostDraw = null) : base(name, scaleType)
    {
        SilkyUI = silkyUI;
        PreDraw += onPreDraw;
        PostDraw += onPostDraw;
    }

    public override bool DrawSelf()
    {
        var transformMatrix = ScaleType switch
        {
            InterfaceScaleType.Game => Main.GameViewMatrix.ZoomMatrix,
            UI => Main.UIScaleMatrix,
            None or _ => Matrix.Identity,
        };

        SilkyUI.TransformMatrix = transformMatrix;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred,
            null, null, null, null, null, transformMatrix);

        PreDraw?.Invoke();
        SilkyUI.DrawUI();
        PostDraw?.Invoke();
        return true;
    }
}