using Microsoft.CodeAnalysis;
using SilkyUIFramework.Core;
using SilkyUIFramework.MyElement;

namespace SilkyUIFramework;

public enum LayoutType
{
    /// <summary>
    /// 弹性盒子
    /// </summary>
    Flexbox,

    /// <summary>
    /// 自定义
    /// </summary>
    Custom,
}

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public partial class ViewGroup
{
    #region LayoutType LayoutDirection Gap

    public LayoutType LayoutType
    {
        get => _layoutType;
        set
        {
            if (value == _layoutType) return;
            _layoutType = value;
            MarkDirty();
        }
    }

    private LayoutType _layoutType;

    public LayoutDirection LayoutDirection
    {
        get => _layoutDirection;
        set
        {
            if (value == _layoutDirection) return;
            _layoutDirection = value;
            MarkDirty();
        }
    }

    private LayoutDirection _layoutDirection;

    public Size Gap
    {
        get => _gap;
        set => SetGap(value.Width, value.Height);
    }

    private Size _gap;

    public void SetGap(float? width, float? height)
    {
        if (width.Equals(Gap.Width) && height.Equals(Gap.Height)) return;
        _gap = _gap.With(width, height);
        MarkDirty();
    }

    #endregion

    public override Size Measure(Size container)
    {
        base.Measure(container);

        Classify();

        if (LayoutElements.Count <= 0) return OuterBounds.Size;

        var innerSize = InnerBounds.Size;
        foreach (var child in LayoutElements)
        {
            child.Measure(innerSize);
        }

        if (LayoutType != LayoutType.Flexbox) return OuterBounds.Size;
        WrapChildren();
        if (!AutomaticWidth && !AutomaticHeight) return OuterBounds.Size;

        var content = FlexboxMeasure();

        // 需要考虑约束
        if (AutomaticWidth) SetInnerBoundsWidth(MathHelper.Clamp(content.Width, MinInnerWidth, MaxInnerWidth));
        if (AutomaticHeight) SetInnerBoundsHeight(MathHelper.Clamp(content.Height, MinInnerHeight, MaxInnerHeight));

        return OuterBounds.Size;
    }

    public override Size Trim(Size container, float? assignWidth = null, float? assignHeight = null)
    {
        base.Trim(container, assignWidth, assignHeight);

        foreach (var child in FreeElements)
        {
            child.Trim(InnerBounds.Size);
        }

        return OuterBounds.Size;
    }

    /// <summary>对子元素进行布局</summary>
    private void Layout()
    {
        if (_layoutType == LayoutType.Flexbox)
            FlexboxLayout();

        foreach (var child in Children.OfType<ViewGroup>())
        {
            child.Layout();
        }
    }
}