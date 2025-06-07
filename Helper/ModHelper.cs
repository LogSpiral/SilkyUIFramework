using System.Text;

namespace SilkyUIFramework.Helpers;

public static class ModHelper
{
    public static string GetString(string path) =>
        Encoding.UTF8.GetString(ModContent.GetFileBytes(path));
}