using Terraria.ModLoader.Core;

namespace SilkyUIFramework;

// ReSharper disable once ClassNeverInstantiated.Global
public class SilkyUISystem : ModSystem
{
    public static SilkyUISystem Instance => ModContent.GetInstance<SilkyUISystem>();

    public override void PostSetupContent() => InitializeSystem();

    public override void UpdateUI(GameTime gameTime)
    {
        SilkyUIManager.Instance.UpdateUI(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        SilkyUIManager.Instance.ModifyInterfaceLayers(layers);
    }

    private bool _initialized;

    private void InitializeSystem()
    {
        if (_initialized) return;

        // 获取所有 Mod 的程序集
        var assemblies = ModLoader.Mods.Select(mod => mod.Code);

        foreach (var types in assemblies.Select(AssemblyManager.GetLoadableTypes))
        {
            foreach (var type in types.Where(type => type.IsSubclassOf(typeof(BasicBody))))
            {
                if (type.GetCustomAttribute<RegisterUIAttribute>() is { } attribute)
                {
                    SilkyUIManager.Instance.RegisterUI(type, attribute);
                }
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
            if (!manager.BasicBodyMappingTable.TryGetValue(ui, out var type)) continue;
            ui.SetBasicBody(Activator.CreateInstance(type) as BasicBody);
        }
    }
}