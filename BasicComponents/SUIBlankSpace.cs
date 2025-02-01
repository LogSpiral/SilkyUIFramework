namespace SilkyUIFramework.BasicComponents;

/// <summary>
/// 有大用! 用于填充空白, 使得滚动条在最底部
/// </summary>
public class SUIBlankSpace : View
{
    public SUIBlankSpace()
    {
        MinWidth.Set(0f, 1f);
        MinWidth.Set(0f, 1f);
        SetSize(0f, 0f, 1f, 1f);
    }
}