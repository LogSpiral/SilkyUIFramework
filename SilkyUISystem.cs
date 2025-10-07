using SilkyUIFramework.Bootstrap;

namespace SilkyUIFramework;

public partial class SilkyUISystem : ModSystem
{
    public static SilkyUISystem Instance => ModContent.GetInstance<SilkyUISystem>();

    public static IServiceProvider ServiceProvider { get; private set; }

    public SilkyUIManager SilkyUIManager { get; private set; }

    private UIAssemblyScanner _assemblyScanner;

    public override void Load()
    {
        _assemblyScanner = new UIAssemblyScanner();
        ServiceProvider = UIDependencyRegistrar.BuildServiceProvider(_assemblyScanner.GetAllTypes());

        SilkyUIManager = ServiceProvider.GetRequiredService<SilkyUIManager>();
    }

    public override void Unload()
    {
        _assemblyScanner = null;
        ServiceProvider = null;
    }

    public override void PostSetupContent()
    {
        UIBootstrapper.RegisterUI(SilkyUIManager, _assemblyScanner.GetAllTypes());
        SilkyUIManager.InitializeGlobalUI();
    }

    public override void UpdateUI(GameTime gameTime)
        => SilkyUIManager.UpdateUI(gameTime);

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        => SilkyUIManager.ModifyInterfaceLayers(layers);
}

public class SilkyUIPlayer : ModPlayer
{
    public override void OnEnterWorld()
    {
        if (SilkyUISystem.Instance.SilkyUIManager is not { } manager) return;

        foreach (var (layerNode, group) in manager.GameUILayerGroups)
        {
            group.Clear();

            if (!manager.GameUILayerBodyTypesRegistry.TryGetValue(layerNode, out var types)) continue;

            foreach (var type in types)
            {
                var silkyUI = SilkyUISystem.ServiceProvider.GetRequiredService<SilkyUI>();

                silkyUI.Priority = type.GetCustomAttribute<RegisterUIAttribute>()!.Priority;
                silkyUI.SetBody(SilkyUISystem.ServiceProvider.GetRequiredService(type) as BaseBody);

                group.Add(silkyUI);
            }
        }
    }
}