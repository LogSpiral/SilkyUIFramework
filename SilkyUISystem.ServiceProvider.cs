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
            PlayerInput.SetZoom_Unscaled();
        };

        IL_Main.DrawMenu += il =>
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

        ServiceProvider?.Dispose();
        ServiceProvider = null;
    }

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
                if (type.GetCustomAttribute<RegisterUIAttribute>() != null)
                {
                    serviceCollection.AddTransient(type);
                }

                if (type.GetCustomAttribute<RegisterGlobalUIAttribute>() != null)
                {
                    serviceCollection.AddSingleton(type);
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

}
