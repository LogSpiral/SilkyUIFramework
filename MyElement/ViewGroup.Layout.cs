using System.Security.Cryptography.X509Certificates;
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

    public void SetGap(float? width = null, float? height = null)
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

        if (LayoutType != LayoutType.Flexbox)
            return OuterBounds.Size;

        WrapChildren();

        if (!AutomaticWidth && !AutomaticHeight)
            return OuterBounds.Size;

        var contentSize = MeasureAxes();

        // 需要考虑约束
        if (AutomaticWidth) SetInnerBoundsWidth(MathHelper.Clamp(contentSize.Width, MinInnerWidth, MaxInnerWidth));
        if (AutomaticHeight) SetInnerBoundsHeight(MathHelper.Clamp(contentSize.Height, MinInnerHeight, MaxInnerHeight));

        return OuterBounds.Size;
    }

    public override Size Trim(Size container, float? assignWidth = null, float? assignHeight = null)
    {
        base.Trim(container, assignWidth, assignHeight);
        var size = InnerBounds.Size;

        if (LayoutType != LayoutType.Flexbox)
        {
            foreach (var child in Children.Where(el => !el.Invalid))
            {
                child.Trim(size);
            }

            return OuterBounds.Size;
        }

        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                foreach (var axis in _flexboxAxes)
                {
                    float? assign = null;
                    if (CrossAlignment == CrossAlignment.Stretch) assign = size.Height;

                    var remainingWidth = size.Width - axis.MainAxisSize;

                    if (remainingWidth > 0)
                    {
                        var grow = axis.CalculateGrow();
                        // goto
                        if (grow == 0) goto DirectTrim;

                        var each = remainingWidth / grow;

                        foreach (var element in axis.Elements)
                        {
                            element.Trim(size, element.OuterBounds.Width + each * element.FlexGrow, assign);
                        }
                    }
                    else
                    {
                        var shrink = axis.CalculateShrink();
                        // goto
                        if (shrink == 0) goto DirectTrim;

                        var each = remainingWidth / shrink;
                        foreach (var element in axis.Elements)
                        {
                            element.Trim(size, element.OuterBounds.Width + each * element.FlexShrink, assign);
                        }
                    }

                    // 多种情况下都不需要 assignWidth
                    DirectTrim:
                    foreach (var element in axis.Elements)
                    {
                        element.Trim(size, assignHeight: assign);
                    }
                }

                break;
            }
            case LayoutDirection.Column:
            {
                foreach (var axis in _flexboxAxes)
                {
                    float? assign = null;
                    if (CrossAlignment == CrossAlignment.Stretch) assign = size.Width;

                    var remainingHeight = size.Height - axis.MainAxisSize;

                    if (remainingHeight > 0)
                    {
                        var grow = axis.CalculateGrow();
                        if (grow == 0) goto DirectTrim;

                        var each = remainingHeight / grow;

                        foreach (var element in axis.Elements)
                        {
                            element.Trim(size, assign, element.OuterBounds.Height + each * element.FlexGrow);
                        }
                    }
                    else
                    {
                        var shrink = axis.CalculateShrink();
                        if (shrink == 0) goto DirectTrim;

                        var each = remainingHeight / shrink;
                        foreach (var element in axis.Elements)
                        {
                            element.Trim(size, assign, element.OuterBounds.Height + each * element.FlexShrink);
                        }
                    }

                    // 多种情况下都不需要 assignWidth
                    DirectTrim:
                    foreach (var element in axis.Elements)
                    {
                        element.Trim(size, assign);
                    }
                }

                break;
            }
        }

        foreach (var child in FreeElements)
        {
            child.Trim(size);
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