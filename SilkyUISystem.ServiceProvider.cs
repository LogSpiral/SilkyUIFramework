using log4net;
using MonoMod.Cil;
using Terraria.ModLoader.Core;

namespace SilkyUIFramework;

public partial class SilkyUISystem
{
    public override void Load()
    {
        // 更新全局 UI
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

            // 绘制全局 UI
            c.EmitDelegate(() =>
            {
                SilkyUIManager?.DrawGlobalUI(Main.gameTimeCache);
            });
        };

        Assemblies = ModLoader.Mods.Select(mod => mod.Code);
        ServiceProvider = BuildServiceProvider();

        Logger = ServiceProvider.GetRequiredService<ILog>();
        SilkyUIManager = ServiceProvider.GetRequiredService<SilkyUIManager>();
    }

    public override void Unload()
    {
        Assemblies = null;
        ServiceProvider = null;
    }

    private ServiceProvider BuildServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(sp => SilkyUIFramework.Instance.Logger);
        serviceCollection.AddSingleton(sp => Instance);

        foreach (var types in Assemblies.Select(AssemblyManager.GetLoadableTypes))
        {
            ScanRegisterServices(serviceCollection, types);

            // 扫描 UI
            foreach (var type in types.Where(type => type.IsSubclassOf(typeof(BasicBody))))
            {
                if (type.GetCustomAttribute<RegisterUIAttribute>() != null)
                {
                    RegisterService(serviceCollection, ServiceLifetime.Transient, type);
                }

                if (type.GetCustomAttribute<RegisterGlobalUIAttribute>() != null)
                {
                    RegisterService(serviceCollection, ServiceLifetime.Singleton, type);
                }
            }
        }

        return serviceCollection.BuildServiceProvider();
    }

    /// <summary> 注册服务 </summary>
    private static void ScanRegisterServices(IServiceCollection services, Type[] types)
    {
        foreach (var type in types)
        {
            if (type.GetCustomAttribute<ServiceAttribute>() is not { } serviceAttribute) continue;

            RegisterService(services, serviceAttribute.Lifetime, type);
        }
    }

    private static void RegisterService(
        IServiceCollection services, ServiceLifetime lifetime, Type implType)
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

}
