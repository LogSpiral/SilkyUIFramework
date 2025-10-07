using MonoMod.Cil;

namespace SilkyUIFramework.Hooks;

internal class UIHookInstaller : ILoadable
{
    public void Load(Mod mod)
    {
        On_Main.DoUpdate_HandleInput += (orig, self) =>
        {
            orig(self);
            PlayerInput.SetZoom_UI();
            SilkyUISystem.Instance?.SilkyUIManager?.UpdateGlobalUI(Main.gameTimeCache);
            PlayerInput.SetZoom_World();
        };

        IL_Main.DrawMenu += il =>
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchCall<Main>(nameof(Main.DrawThickCursor)))) return;
            if (!c.TryGotoPrev(MoveType.Before, i => i.MatchLdcI4(0))) return;
            c.EmitDelegate(() => SilkyUISystem.Instance?.SilkyUIManager?.DrawGlobalUI(Main.gameTimeCache));
        };
    }

    public void Unload() { }
}
