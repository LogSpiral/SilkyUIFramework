using Microsoft.Xna.Framework.Graphics;
using static Terraria.UI.InterfaceScaleType;

namespace SilkyUIFramework;

/// <summary>
/// Game UI Layer
/// </summary>
public class SilkyUILayer : GameInterfaceLayer
{
    public SilkyUIManager SilkyUIManager { get; }
    public SilkyUI SilkyUI { get; }

    public SilkyUILayer(SilkyUIManager silkyUIManager, SilkyUI silkyUI, string name, InterfaceScaleType scaleType) : base(name, scaleType)
    {
        SilkyUIManager = silkyUIManager;
        SilkyUI = silkyUI;
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
            null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, transformMatrix);

        SilkyUIManager.CurrentSilkyUI = SilkyUI;
        SilkyUI.Draw(Main.gameTimeCache, Main.spriteBatch);
        SilkyUIManager.CurrentSilkyUI = null;
        return true;
    }
}