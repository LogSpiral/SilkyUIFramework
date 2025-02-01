namespace SilkyUIFramework;

// ReSharper disable once ClassNeverInstantiated.Global
public class SilkyUISystem : ModSystem
{
    public static SilkyUISystem Instance => ModContent.GetInstance<SilkyUISystem>();

    public override void PostSetupContent() => Initialize();

    public override void UpdateUI(GameTime gameTime) =>
        SilkyUIManager.Instance.UpdateUI(gameTime);

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) =>
        SilkyUIManager.Instance.ModifyInterfaceLayers(layers);

    private bool _initialized;

    /// <summary> 初始化: 从所有 MOD 中查找带有 AutoloadUI 特性的 BasicBody 的子类 </summary>
    private void Initialize()
    {
        if (_initialized) return;

        var assemblies = ModLoader.Mods.Select(mod => mod.Code);

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types.Where(type => type.IsSubclassOf(typeof(BasicBody))))
            {
                if (type.GetCustomAttribute<AutoloadUIAttribute>() is not { } attribute) continue;
                SilkyUIManager.Instance.RegisterUI(type, attribute);
            }
        }

        _initialized = true;
    }
}

public class SilkyUIPlayer : ModPlayer
{
    public override void OnEnterWorld() => SetupSilkyUI();

    private static void SetupSilkyUI()
    {
        if (SilkyUIManager.Instance is not { } manager) return;

        foreach (var ui in manager.SilkyUILayerNodes.SelectMany(uis => uis.Value))
        {
            if (!manager.BasicBodyTypes.TryGetValue(ui, out var type)) continue;
            ui.SetBasicBody(Activator.CreateInstance(type) as BasicBody);
        }
    }
}