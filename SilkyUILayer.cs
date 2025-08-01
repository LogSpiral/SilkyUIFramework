namespace SilkyUIFramework;

public class SilkyUILayer(SilkyUIGroup silkyUIGroup, SilkyUI silkyUI, string name, InterfaceScaleType scaleType) : GameInterfaceLayer(name, scaleType)
{
    public SilkyUIGroup SilkyUIGroup { get; } = silkyUIGroup;
    public SilkyUI SilkyUI { get; } = silkyUI;

    public override bool DrawSelf()
    {
        SilkyUI.TransformMatrix = ScaleType switch
        {
            InterfaceScaleType.Game => Main.GameViewMatrix.ZoomMatrix,
            InterfaceScaleType.UI => Main.UIScaleMatrix,
            InterfaceScaleType.None or _ => Matrix.Identity,
        };

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred,
            null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, SilkyUI.TransformMatrix);

        SilkyUIGroup.CurrentUI = SilkyUI;
        SilkyUI.Draw(Main.gameTimeCache, Main.spriteBatch);
        SilkyUIGroup.CurrentUI = null;
        return true;
    }
}