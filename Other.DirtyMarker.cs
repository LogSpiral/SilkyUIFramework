using SilkyUIFramework.Core;

namespace SilkyUIFramework;

public partial class Other
{
    protected bool IsDirty { get; set; }

    /// <summary>
    /// 冒泡标记脏
    /// </summary>
    public void BubbleMarkerDirty()
    {
        IsDirty = true;

        if (Positioning is not Positioning.Absolute && Parent is { } parent)
        {
            parent.BubbleMarkerDirty();
        }
    }

    /// <summary>
    /// 处理脏标记
    /// </summary>
    public void HandleDirtyMarker()
    {
        if (IsDirty)
        {
            Measure(WidthValue, HeightValue);
        }
    }
}