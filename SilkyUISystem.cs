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
        foreach (var types in Assemblies.Select(AssemblyManager.GetLoadableTypes))
        {
            ScanUI(types);
        }

        SilkyUIManager.InitializeGlobalUI();
    }

    private void ScanUI(Type[] types)
    {
        Logger.Info($"开始扫描游戏 UI");
        foreach (var type in types.Where(type => type.IsSubclassOf(typeof(BasicBody))))
        {
            if (type.GetCustomAttribute<RegisterUIAttribute>() is { } attribute)
            {
                SilkyUIManager.RegisterUI(type, attribute.LayerNode);
            }
        }

        Logger.Info("开始扫描全局 UI");
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

        foreach (var (layerNode, group) in manager.SilkyUILayerNodes)
        {
            group.Clear();

            if (!manager.SilkyUIType.TryGetValue(layerNode, out var types)) continue;

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