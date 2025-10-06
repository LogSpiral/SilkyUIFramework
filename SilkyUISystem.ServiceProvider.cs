using log4net;
using MonoMod.Cil;
using Terraria.ModLoader.Core;

namespace SilkyUIFramework;

public partial class SilkyUISystem
{
    public override void Load()
    {
        On_Main.DoUpdate_HandleInput += (orig, self) =>
        {
            orig(self);

            PlayerInput.SetZoom_UI();
            SilkyUIManager?.UpdateGlobalUI(Main.gameTimeCache);
            PlayerInput.SetZoom_World();
        };

        IL_Main.DrawMenu += il =>
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.Before, i => i.MatchCall<Main>(nameof(Main.DrawThickCursor)))) return;
            if (!c.TryGotoPrev(MoveType.Before, i => i.MatchLdcI4(0))) return;

            // 2025/9/16 需要找个新地方 游戏里面开启菜单的时候就不绘制了
            // Draw Global UI
            c.EmitDelegate(() => SilkyUIManager?.DrawGlobalUI(Main.gameTimeCache));
        };

        Assemblies = ModLoader.Mods.Select(mod => mod.Code);
        ServiceProvider = BuildServiceProvider();

        Logger = ServiceProvider.GetRequiredService<ILog>();
        SilkyUIManager = ServiceProvider.GetRequiredService<SilkyUIManager>();
    }

    private ServiceProvider BuildServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(_ => SilkyUIFramework.Instance.Logger);
        serviceCollection.AddSingleton(_ => Instance);

        foreach (var types in Assemblies.Select(AssemblyManager.GetLoadableTypes))
        {
            RegisterServices(serviceCollection, ScanServices(types));

            foreach (var type in types.Where(type => type.IsSubclassOf(typeof(BaseBody))))
            {
                if (type.GetCustomAttribute<RegisterUIAttribute>() != null)
                {
                    RegisterServiceImplementation(serviceCollection, ServiceLifetime.Transient, type);
                }

                if (type.GetCustomAttribute<RegisterGlobalUIAttribute>() != null)
                {
                    RegisterServiceImplementation(serviceCollection, ServiceLifetime.Singleton, type);
                }
            }
        }

        return serviceCollection.BuildServiceProvider();
    }

    private static IEnumerable<(Type, ServiceAttribute)> ScanServices(Type[] types)
    {
        return types
            .Select(type => (Type: type, Service: type.GetCustomAttribute<ServiceAttribute>()))
            .Where((values, index) => values.Service != null);
    }

    private static void RegisterServices(IServiceCollection services, IEnumerable<(Type, ServiceAttribute)> values)
    {
        foreach (var (Type, Service) in values)
        {
            RegisterServiceImplementation(services, Service.Lifetime, Type);
        }
    }

    private static void RegisterServiceImplementation(IServiceCollection services, ServiceLifetime lifetime, Type implType)
    {
        var interfaces = implType.GetInterfaces();

        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                services.AddTransient(implType);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(implType);
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton(implType);
                break;
        }

        foreach (var serviceType in interfaces)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                    services.AddTransient(serviceType, sp => sp.GetRequiredService(implType));
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(serviceType, sp => sp.GetRequiredService(implType));
                    break;
                case ServiceLifetime.Singleton:
                    services.AddSingleton(serviceType, sp => sp.GetRequiredService(implType));
                    break;
            }
        }
    }

    public override void Unload()
    {
        Assemblies = null;
        ServiceProvider = null;
    }
}
