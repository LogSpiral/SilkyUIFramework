using System.Text;

namespace SilkyUIFramework.Helper;

public static class ModHelper
{
    public static string GetString(string path) =>
        Encoding.UTF8.GetString(ModContent.GetFileBytes(path));
}