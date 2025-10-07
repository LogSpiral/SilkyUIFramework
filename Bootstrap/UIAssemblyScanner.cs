using Terraria.ModLoader.Core;

namespace SilkyUIFramework.Bootstrap;

internal class UIAssemblyScanner()
{
    private readonly IReadOnlyList<Assembly> _assemblies = [.. ModLoader.Mods.Select(m => m.Code)];
    public IEnumerable<Type[]> GetAllTypes() => _assemblies.Select(AssemblyManager.GetLoadableTypes);
}
