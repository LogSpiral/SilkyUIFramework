using SilkyUIFramework.Animation;

namespace SilkyUIFramework.BasicElements;

public partial class View
{
    #region RoundedRectangle

    public RoundedRectangle RoundedRectangle { get; } = new();

    public Vector4 CornerRadius
    {
        get => RoundedRectangle.CornerRadius;
        set => RoundedRectangle.CornerRadius = value;
    }

    public float Border
    {
        get => RoundedRectangle.Border;
        set => RoundedRectangle.Border = value;
    }

    public Color BgColor
    {
        get => RoundedRectangle.BgColor;
        set => RoundedRectangle.BgColor = value;
    }

    public Color BorderColor
    {
        get => RoundedRectangle.BorderColor;
        set => RoundedRectangle.BorderColor = value;
    }

    #endregion

    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    /// <summary>
    /// 使元素无效, 就像不存在一样
    /// </summary>
    public bool Invalidate { get; set; } = false;

    public float ZIndex { get; set; }

    public AnimationTimer HoverTimer { get; } = new();

    /// <summary>
    /// 最后绘制边框
    /// </summary>
    public bool FinallyDrawBorder { get; set; }

    /// <summary>
    /// 隐藏完全溢出元素
    /// </summary>
    public bool HideFullyOverflowedElements { get; set; }

    /// <summary>
    /// 拖动忽略, true: 不影响父辈元素拖动, false: 阻止父辈元素拖动
    /// </summary>
    public bool DragIgnore { get; set; } = true;

    public bool LeftMousePressed { get; set; }
    public bool RightMousePressed { get; set; }
    public bool MiddleMousePressed { get; set; }

    /// <summary> true: 固定宽, 否则由布局决定 </summary>
    public bool SpecifyWidth { get; set; }

    /// <summary> true: 固定高, 否则由布局决定 </summary>
    public bool SpecifyHeight { get; set; }
}