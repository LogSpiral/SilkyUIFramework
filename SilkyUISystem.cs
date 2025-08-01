using log4net;
using MonoMod.Cil;
using Terraria.ModLoader.Core;

namespace SilkyUIFramework;

public class SilkyUISystem : ModSystem
{
    public static SilkyUISystem Instance => ModContent.GetInstance<SilkyUISystem>();

    public ILog Logger { get; private set; }

    /// <summary>
    /// 服务提供者，用于依赖注入
    /// </summary>
    public static ServiceProvider ServiceProvider { get; private set; }

    public IEnumerable<Assembly> Assemblies { get; private set; }

    public SilkyUIManager SilkyUIManager { get; private set; }

    #region Load Unload

    private void On_Main_DoUpdate_HandleInput(On_Main.orig_DoUpdate_HandleInput orig, Main self)
    {
        orig(self);
        PlayerInput.SetZoom_UI();
        SilkyUIManager?.UpdateGlobalUI(Main.gameTimeCache);
        PlayerInput.SetZoom_Unscaled();
    }

    private void IL_Main_DrawMenu(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.Before, i => i.MatchCall<Main>(nameof(Main.DrawThickCursor))))
        {
            return;
        }

        if (!c.TryGotoPrev(MoveType.Before, i => i.MatchLdcI4(0)))
        {
            return;
        }

        c.EmitDelegate(() =>
        {
            SilkyUIManager?.DrawGlobalUI(Main.gameTimeCache);
        });
    }

    public override void Load()
    {
        On_Main.DoUpdate_HandleInput += On_Main_DoUpdate_HandleInput;
        IL_Main.DrawMenu += IL_Main_DrawMenu;

        Assemblies = ModLoader.Mods.Select(mod => mod.Code);
        ServiceProvider = BuildServiceProvider();

        Logger = ServiceProvider.GetRequiredService<ILog>();
        SilkyUIManager = ServiceProvider.GetRequiredService<SilkyUIManager>();
    }

    public override void Unload()
    {
        Assemblies = null;
        ServiceProvider?.Dispose();
        ServiceProvider = null;
    }

    #endregion

    #region ServiceProvider 服务提供商

    private ServiceProvider BuildServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(sp => SilkyUIFramework.Instance.Logger);
        serviceCollection.AddSingleton(sp => Instance);

        foreach (var types in Assemblies.Select(AssemblyManager.GetLoadableTypes))
        {
            RegisterServices(serviceCollection, types);

            // 扫描 UI
            foreach (var type in types.Where(type => type.IsSubclassOf(typeof(BasicBody))))
            {
                if (type.GetCustomAttribute<RegisterUIAttribute>() != null ||
                    type.GetCustomAttribute<RegisterGlobalUIAttribute>() != null)
                {
                    serviceCollection.AddTransient(type);
                }
            }
        }

        return serviceCollection.BuildServiceProvider();
    }

    private static void RegisterServices(IServiceCollection services, Type[] types)
    {
        foreach (var type in types)
        {
            if (type.GetCustomAttribute<ServiceAttribute>() is not { } serviceAttribute) continue;

            RegisterService(services, serviceAttribute.Lifetime, type);

            var interfaces = type.GetInterfaces();

            foreach (var serviceType in interfaces)
            {
                RegisterService(services, serviceAttribute.Lifetime, serviceType, type);
            }
        }
    }

    private static void RegisterService(
        IServiceCollection services, ServiceLifetime lifetime, Type serviceType)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                services.AddTransient(serviceType);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(serviceType);
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton(serviceType);
                break;
        }
    }

    private static void RegisterService(
        IServiceCollection services, ServiceLifetime lifetime, Type serviceType, Type implementationType)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                services.AddTransient(serviceType, implementationType);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(serviceType, implementationType);
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton(serviceType, implementationType);
                break;
        }
    }

    #endregion

    public override void PostSetupContent()
    {
        foreach (var types in Assemblies.Select(AssemblyManager.GetLoadableTypes))
        {
            ScanRegisterUI(types);
        }

        SilkyUIManager.InitializeGlobalUI();
    }

    private void ScanRegisterUI(Type[] types)
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