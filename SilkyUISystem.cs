using log4net;
using Terraria.ModLoader.Core;

namespace SilkyUIFramework;

public partial class SilkyUISystem : ModSystem
{
    public static SilkyUISystem Instance => ModContent.GetInstance<SilkyUISystem>();

    public ILog Logger { get; private set; }

    /// <summary>
    /// 服务提供者，用于依赖注入
    /// </summary>
    public static IServiceProvider ServiceProvider { get; private set; }

    public static IMouseMenu GetRequiredService<IMouseMenu>()
    {
        return ServiceProvider.GetRequiredService<IMouseMenu>();
    }

    public IEnumerable<Assembly> Assemblies { get; private set; }

    public SilkyUIManager SilkyUIManager { get; private set; }

    public override void PostSetupContent()
    {
        foreach (var data in Assemblies.Select(assembly => (Assembly: assembly, Types: AssemblyManager.GetLoadableTypes(assembly))))
        {
            ScanAndRegisterGameUI(data.Assembly, data.Types);
            ScanAndRegisterGloablUI(data.Assembly, data.Types);
        }

        SilkyUIManager.InitializeGlobalUI();
    }

    private void ScanAndRegisterGameUI(Assembly assembly, Type[] types)
    {
        Logger.Info($"Scan Game User Interface in {assembly.FullName}");

        foreach (var type in types.Where(type => type.IsSubclassOf(typeof(BasicBody))))
        {
            if (type.GetCustomAttribute<RegisterUIAttribute>() is { } attribute)
            {
                SilkyUIManager.RegisterUI(type, attribute.LayerNode);
            }
        }
    }

    private void ScanAndRegisterGloablUI(Assembly assembly, Type[] types)
    {
        Logger.Info($"Scan Global User Interface in {assembly.FullName}");

        foreach (var type in types.Where(type => type.IsSubclassOf(typeof(BasicBody))))
        {
            if (type.GetCustomAttribute<RegisterGlobalUIAttribute>() != null)
            {
                SilkyUIManager.RegisterGlobalUI(type);
            }
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        SilkyUIManager.UpdateUI(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        SilkyUIManager.ModifyInterfaceLayers(layers);
    }
}

public class SilkyUIPlayer : ModPlayer
{
    // 每次进入游戏时，重新创建 UI 实例
    public override void OnEnterWorld()
    {
        if (SilkyUISystem.Instance.SilkyUIManager is not { } manager) return;

        foreach (var (layerNode, group) in manager.SilkyUIGroups)
        {
            group.Clear();

            if (!manager.SilkyUITypes.TryGetValue(layerNode, out var types)) continue;

            foreach (var type in types)
            {
                var silkyUI = SilkyUISystem.ServiceProvider.GetRequiredService<SilkyUI>();

                silkyUI.Priority = type.GetCustomAttribute<RegisterUIAttribute>().Priority;
                silkyUI.SetBody(SilkyUISystem.ServiceProvider.GetRequiredService(type) as BasicBody);

                group.Add(silkyUI);
            }
        }
    }
}