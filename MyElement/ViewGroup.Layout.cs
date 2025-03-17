using System.Net.Mime;
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

    public override void Measure(Size container)
    {
        base.Measure(container);

        ClassifyElements();

        if (LayoutChildren.Count <= 0) return;

        var innerSize = InnerBounds.Size;
        foreach (var child in LayoutChildren)
        {
            child.Measure(innerSize);
        }

        if (LayoutType != LayoutType.Flexbox) return;

        WrapLayoutElements();

        if (!AutomaticWidth && !AutomaticHeight) return;

        float contentWidth, contentHeight;

        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                AxisTracks.Measure(Gap.Width, out contentWidth, out contentHeight);
                break;
            }
            case LayoutDirection.Column:
            {
                AxisTracks.Measure(Gap.Height, out contentHeight, out contentWidth);
                break;
            }
        }

        // 需要考虑约束
        if (AutomaticWidth) SetInnerBoundsWidth(contentWidth);
        if (AutomaticHeight) SetInnerBoundsHeight(contentHeight);
    }

    public override void Trim(Size container, float? outerWidth = null, float? outerHeight = null)
    {
        base.Trim(container, outerWidth, outerHeight);

        var innerSize = InnerBounds.Size;
        if (LayoutType is not LayoutType.Flexbox)
        {
            foreach (var child in LayoutChildren)
            {
                child.Trim(innerSize);
            }
            return;
        }

        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                AdjustAxisTracksCrossAxisSize(InnerBounds.Height, Gap.Height);
                // 处理每个轴轨道中子元素的 FlexGrow/FlexShrink 逻辑（Row 布局中处理宽度）
                ProcessAxisTracks(isRow: true, innerSize);
                break;
            }
            case LayoutDirection.Column:
            {
                AdjustAxisTracksCrossAxisSize(InnerBounds.Width, Gap.Width);
                // 处理每个轴轨道中子元素的 FlexGrow/FlexShrink 逻辑（Column 布局中处理高度）
                ProcessAxisTracks(isRow: false, innerSize);
                break;
            }
        }
    }

    /// <summary>
    /// 当交叉对齐方式为 Stretch 时，根据容器的交叉轴尺寸调整各个轴轨道的交叉轴尺寸
    /// </summary>
    /// <param name="isRow">是否为行布局（Row），true 表示 Row 布局，false 表示 Column 布局</param>
    private void AdjustAxisTracksCrossAxisSize(float crossAxisContainer, float crossGap)
    {
        foreach (var axisTrack in AxisTracks)
        {
            axisTrack.CrossAxisContainer = axisTrack.CrossAxisContent;
        }

        if (CrossContentAlignment is CrossContentAlignment.Stretch)
        {
            var crossContentSize = AxisTracks.CrossAxisContentSum() + AxisTracks.CrossGapSum(crossGap);
            if (crossContentSize < crossAxisContainer)
            {
                var each = (crossAxisContainer - crossContentSize) / AxisTracks.Count;
                foreach (var axisTrack in AxisTracks)
                {
                    axisTrack.CrossAxisContainer += each;
                }
            }
        }
    }

    /// <summary>
    /// 处理每个轴轨道中子元素的 FlexGrow 和 FlexShrink 逻辑
    /// </summary>
    /// <param name="isRow">是否为行布局（Row），true 表示 Row 布局，false 表示 Column 布局</param>
    /// <param name="innerSize">内部容器尺寸</param>
    private void ProcessAxisTracks(bool isRow, Size innerSize)
    {
        var crossStretch = CrossAlignment == CrossAlignment.Stretch;
        foreach (var axisTrack in AxisTracks)
        {
            // 对于 Row 布局，assignValue 用于 outerHeight 参数；对于 Column 布局，用于 outerWidth 参数
            float? assignValue = crossStretch ? axisTrack.CrossAxisContainer : null;
            // 计算剩余空间：Row 布局中使用容器宽度，Column 布局中使用容器高度
            float remaining = isRow ? innerSize.Width - axisTrack.MainAxisContent : innerSize.Height - axisTrack.MainAxisContent;

            if (remaining == 0)
            {
                axisTrack.Elements.ForEach(el =>
                {
                    if (isRow)
                        el.Trim(innerSize, null, assignValue);
                    else
                        el.Trim(innerSize, assignValue, null);
                });
                return;
            }
            else if (remaining > 0)
            {
                var grow = axisTrack.GrowSum();
                if (grow == 0)
                {
                    axisTrack.Elements.ForEach(el =>
                    {
                        if (isRow)
                            el.Trim(innerSize, null, assignValue);
                        else
                            el.Trim(innerSize, assignValue, null);
                    });
                    return;
                }

                var each = remaining / grow;
                foreach (var element in axisTrack.Elements)
                {
                    if (isRow)
                    {
                        var newWidth = element.OuterBounds.Width + each * element.FlexGrow;
                        element.Trim(innerSize, newWidth, assignValue);
                    }
                    else
                    {
                        var newHeight = element.OuterBounds.Height + each * element.FlexGrow;
                        element.Trim(innerSize, assignValue, newHeight);
                    }
                }
            }
            else // remaining < 0，表示需要缩小尺寸
            {
                var shrink = axisTrack.ShrinkSum();
                if (shrink == 0)
                {
                    axisTrack.Elements.ForEach(el =>
                    {
                        if (isRow)
                            el.Trim(innerSize, null, assignValue);
                        else
                            el.Trim(innerSize, assignValue, null);
                    });
                    return;
                }

                var each = remaining / shrink;
                foreach (var element in axisTrack.Elements)
                {
                    if (isRow)
                    {
                        var newWidth = element.OuterBounds.Width + each * element.FlexShrink;
                        element.Trim(innerSize, newWidth, assignValue);
                    }
                    else
                    {
                        var newHeight = element.OuterBounds.Height + each * element.FlexShrink;
                        element.Trim(innerSize, assignValue, newHeight);
                    }
                }
            }
        }
    }


    /// <summary>对子元素进行布局</summary>
    private void ApplyLayout()
    {
        if (_layoutType == LayoutType.Flexbox)
            ApplyFlexboxLayout();

        foreach (var child in LayoutChildren.OfType<ViewGroup>())
        {
            child.ApplyLayout();
        }
    }
}