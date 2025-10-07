namespace SilkyUIFramework;

public partial class SilkyUIManager
{
    private SilkyUIGroup GlobalSilkyUIGroup { get; set; }

    /// <summary>
    /// 扫描到的所有 Global UI 的 <see cref="Type"/>
    /// </summary>
    private List<Type> GlobalUIBodyTypesRegistry { get; } = [];

    public void RegisterGlobalUI(IEnumerable<Type> types)
    {
        if (_isRegistrationCompleted) return;

        GlobalUIBodyTypesRegistry.AddRange(types);
    }

    public void InitializeGlobalUI()
    {
        if (Main.netMode == NetmodeID.Server) return;

        GlobalSilkyUIGroup = ServiceProvider.GetRequiredService<SilkyUIGroup>();

        foreach (var type in GlobalUIBodyTypesRegistry)
        {
            var silkyUI = ServiceProvider.GetRequiredService<SilkyUI>();
            var body = ServiceProvider.GetRequiredService(type) as BaseBody;

            silkyUI.Priority = type.GetCustomAttribute<RegisterGlobalUIAttribute>()!.Priority;
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
        CurrentSilkyUIGroup?.UpdateUI(gameTime);

        CurrentSilkyUIGroup = null;
    }

    public void DrawGlobalUI(GameTime gameTime)
    {
        if (Main.netMode == NetmodeID.Server) return;

        GlobalSilkyUIGroup?.Draw(gameTime);
    }
}