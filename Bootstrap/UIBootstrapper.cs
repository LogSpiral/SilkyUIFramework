namespace SilkyUIFramework.Bootstrap;

internal class UIBootstrapper
{
    public static void RegisterUI(SilkyUIManager manager, IEnumerable<Type[]> allTypes)
    {
        foreach (var types in allTypes)
        {
            var gameUI = CollectGameUI(types);
            var globalUI = CollectGlobalUI(types);

            foreach (var (type, layer) in gameUI)
                manager.RegisterUI(type, layer);

            manager.RegisterGlobalUI(globalUI);
        }
    }

    private static IEnumerable<(Type, string)> CollectGameUI(Type[] types)
        => types.Where(t => t.IsSubclassOf(typeof(BaseBody)))
                .Select(t => (t, t.GetCustomAttribute<RegisterUIAttribute>()?.LayerNode))
                .Where(p => p.LayerNode != null);

    private static IEnumerable<Type> CollectGlobalUI(Type[] types)
        => types.Where(t => t.IsSubclassOf(typeof(BaseBody)))
                .Where(t => t.GetCustomAttribute<RegisterGlobalUIAttribute>() != null);
}
