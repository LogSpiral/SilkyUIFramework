namespace SilkyUIFramework;

public partial class SilkyUIManager
{
    public SilkyUIGroup GlobalSilkyUIGroup { get; private set; }

    public List<Type> GlobalBodyTypes { get; } = [];

    /// <summary> 注册全局 UI </summary>
    public void RegisterGlobalUI(Type bodyType)
    {
        Logger.Info($"Register Global UI: \"{bodyType.Name}\"");

        GlobalBodyTypes.Add(bodyType);
    }

    public void InitializeGlobalUI()
    {
        if (Main.netMode == NetmodeID.Server) return;

        GlobalSilkyUIGroup = ServiceProvider.GetService<SilkyUIGroup>();

        foreach (var type in GlobalBodyTypes)
        {
            var silkyUI = ServiceProvider.GetRequiredService<SilkyUI>();
            var body = ServiceProvider.GetRequiredService(type) as BaseBody;

            silkyUI.Priority = type.GetCustomAttribute<RegisterGlobalUIAttribute>().Priority;
            silkyUI.SetBody(body);

            GlobalSilkyUIGroup.Add(silkyUI);
        }
    }

    public void UpdateGlobalUI(GameTime gameTime)
    {
        if (Main.netMode == NetmodeID.Server) return;

        MouseHoverGroup = null;
        MouseFocusGroup = null;

        CurrentSilkyUIGroup = GlobalSilkyUIGroup;
        CurrentSilkyUIGroup?.Order();
        CurrentSilkyUIGroup?.UpdateUI(gameTime);

        CurrentSilkyUIGroup = null;
    }

    public void DrawGlobalUI(GameTime gameTime)
    {
        if (Main.netMode == NetmodeID.Server) return;

        GlobalSilkyUIGroup?.Order();
        GlobalSilkyUIGroup?.Draw(gameTime);
    }
}
