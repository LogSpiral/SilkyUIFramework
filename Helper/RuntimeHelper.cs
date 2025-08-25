using System.Diagnostics;

namespace SilkyUIFramework.Helper;

public static class RuntimeHelper
{
    public static void ErrorCapture(Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch (Exception ex)
        {
            SilkyUIFramework.Instance?.Logger?.Error("RuntimeHelper ErrorCapture", ex);
            if (Debugger.IsAttached) throw;
        }
    }
}
