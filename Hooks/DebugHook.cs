using Terraria.Map;

namespace SilkyUIFramework.Hooks;

#if false
internal class DebugHook : ModSystem
{
    public override void Load()
    {
        On_Main.Update += On_Main_Update;

        On_Main.DrawInterface += On_Main_DrawInterface;

        On_Main.DrawPlayerChatBubbles += On_Main_DrawPlayerChatBubbles;

        On_Main.DoDraw += On_Main_DoDraw;
    }

    private void On_Main_DoDraw(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
    {
        orig(self, gameTime);

        if (SilkyUIFramework.Instance is not { } suiFramework) return;

        //suiFramework.Logger.Info($"gameMenu {Main.gameMenu}");
        //suiFramework.Logger.Info(Main.mapFullscreen);
    }

    private void On_Main_DrawPlayerChatBubbles(On_Main.orig_DrawPlayerChatBubbles orig, Main self)
    {
        orig(self);

        if (SilkyUIFramework.Instance is not { } suiFramework) return;
    }

    private void On_Main_DrawInterface(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
    {
        orig(self, gameTime);

        if (SilkyUIFramework.Instance is not { } suiFramework) return;
    }

    private void On_Main_Update(On_Main.orig_Update orig, Main self, GameTime gameTime)
    {
        orig(self, gameTime);

        if (SilkyUIFramework.Instance is not { } suiFramework) return;
    }

    public override void PostDrawFullscreenMap(ref string mouseText)
    {
        base.PostDrawFullscreenMap(ref mouseText);
    }

    public override void PreDrawMapIconOverlay(IReadOnlyList<IMapLayer> layers, MapOverlayDrawContext mapOverlayDrawContext)
    {
        base.PreDrawMapIconOverlay(layers, mapOverlayDrawContext);
    }
}

#endif