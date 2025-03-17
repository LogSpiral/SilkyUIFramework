using SilkyUIFramework.MyElement;

namespace SilkyUIFramework;

public enum LayoutType
{
    Flexbox,
    Custom,
}

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
                AxisTracks.AdjustAxisTracksCrossAxisSize(CrossContentAlignment, InnerBounds.Height, Gap.Height);
                AxisTracks.ProcessRowAxisTracks(CrossAlignment, innerSize, Gap.Width);
                break;
            }
            case LayoutDirection.Column:
            {
                AxisTracks.AdjustAxisTracksCrossAxisSize(CrossContentAlignment, InnerBounds.Width, Gap.Width);
                AxisTracks.ProcessColumnAxisTracks(CrossAlignment, innerSize, Gap.Height);
                break;
            }
        }
    }

    /// <summary>
    /// 处理每个轴轨道中子元素的 FlexGrow 和 FlexShrink 逻辑
    /// </summary>
    private void ProcessAxisTracks(bool isRow, Size innerSize)
    {
        var crossStretch = CrossAlignment == CrossAlignment.Stretch;

        foreach (var axisTrack in AxisTracks)
        {
            float? assignValue = crossStretch ? axisTrack.AvailableCrossSpace : null;
            var remaining = isRow ? innerSize.Width - axisTrack.MainSize : innerSize.Height - axisTrack.MainSize;

            if (remaining == 0)
            {
                if (isRow) axisTrack.Elements.ForEach(el => el.Trim(innerSize, outerHeight: assignValue));
                else axisTrack.Elements.ForEach(el => el.Trim(innerSize, outerWidth: assignValue));

                return;
            }

            if (remaining > 0)
            {
                var grow = axisTrack.CalculateTotalGrow();
                if (grow == 0)
                {
                    if (isRow) axisTrack.Elements.ForEach(el => el.Trim(innerSize, null, assignValue));
                    else axisTrack.Elements.ForEach(el => el.Trim(innerSize, assignValue, null));
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
            else
            {
                var shrink = axisTrack.CalculateTotalShrink();
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