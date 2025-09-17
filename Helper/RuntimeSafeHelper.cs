using System.Diagnostics;
using log4net;

namespace SilkyUIFramework.Helper;

public static class RuntimeSafeHelper
{
    public static bool StrictMode { get; set; } = false;

    private static ILog Logger
    {
        get
        {
            if (field != null) return field;
            field = ModContent.GetInstance<SilkyUIFramework>()?.Logger;
            return field;
        }
    }

    public static void SafeInvoke<T>(T actions, Action<T> action) where T : Delegate
    {
        if (actions == null || action == null) return;

        var span = actions.GetInvocationList().OfType<T>().ToArray().AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            var @delegate = span[i];
            SafeInvoke(() => action(@delegate));
        }
    }

    public static void SafeInvoke(Action action)
    {
        if (action is null) return;

        try { action.Invoke(); }
        catch (Exception ex)
        {
            Logger?.Error("RuntimeHelper ErrorCapture", ex);
            if (Debugger.IsAttached && StrictMode) throw;
        }
    }
}