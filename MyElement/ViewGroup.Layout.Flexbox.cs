using SilkyUIFramework.MyElement;

namespace SilkyUIFramework;

public partial class ViewGroup
{
    #region FlexWrap MainAlignment CrossAlignment CrossContentAlignment

    public bool MainAxisFixed => LayoutDirection == LayoutDirection.Column ? !AutomaticHeight : !AutomaticWidth;

    public bool FlexWrap
    {
        get => _flexWrap;
        set
        {
            if (value == _flexWrap) return;
            _flexWrap = value;
            MarkDirty();
        }
    }

    private bool _flexWrap;

    public MainAlignment MainAlignment
    {
        get => _mainAlignment;
        set
        {
            if (value == _mainAlignment) return;
            _mainAlignment = value;
            MarkDirty();
        }
    }

    private MainAlignment _mainAlignment;

    public CrossAlignment CrossAlignment
    {
        get => _crossAlignment;
        set
        {
            if (value == _crossAlignment) return;
            _crossAlignment = value;
            MarkDirty();
        }
    }

    private CrossAlignment _crossAlignment;

    public CrossContentAlignment CrossContentAlignment
    {
        get => _crossContentAlignment;
        set
        {
            if (value == _crossContentAlignment) return;
            _crossContentAlignment = value;
            MarkDirty();
        }
    }

    private CrossContentAlignment _crossContentAlignment;

    #endregion

    protected readonly List<AxisTrack> AxisTracks = [];

    /// <summary>
    /// 对子元素进行换行
    /// </summary>
    private void WrapLayoutElements()
    {
        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                var maxMainAxisSize = (FlexWrap && MainAxisFixed) ? InnerBounds.Width : MaxInnerWidth;
                AxisTracks.WrapElements(maxMainAxisSize, Gap.Width,
                    LayoutChildren, el => el.OuterBounds.Width, el => el.OuterBounds.Height);

                return;
            }
            case LayoutDirection.Column:
            {
                var maxMainAxisSize = (FlexWrap && MainAxisFixed) ? InnerBounds.Height : MaxInnerHeight;
                AxisTracks.WrapElements(maxMainAxisSize, Gap.Height,
                    LayoutChildren, el => el.OuterBounds.Height, el => el.OuterBounds.Width);

                return;
            }
        }
    }

    public void ApplyFlexboxLayout()
    {
        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                AxisTracks.CalculateCrossContentAlignment(CrossContentAlignment,
                    InnerBounds.Height, Gap.Height, out var top, out var crossGap);

                foreach (var axis in AxisTracks)
                {
                    // 针对每一行：主轴为水平，容器宽度为可用宽度，按 MainAlignment 计算起始偏移和间隔
                    axis.CalculateMainAlignment(InnerBounds.Width, Gap.Width, MainAlignment, out var left, out var mainGap);

                    foreach (var element in axis.Elements)
                    {
                        // 针对每个元素：计算在当前行（交叉轴高度为 axis.CrossAxisSize）上的偏移
                        float crossOffset = CrossAlignment.CalculateCrossOffset(axis.AvailableCrossSpace, element.OuterBounds.Height);
                        element.SetLayoutOffset(left, top + crossOffset);
                        left += element.OuterBounds.Width + mainGap;
                    }

                    top += axis.AvailableCrossSpace + crossGap;
                }
                break;
            }
            case LayoutDirection.Column:
            {
                AxisTracks.CalculateCrossContentAlignment(CrossContentAlignment,
                    InnerBounds.Width, Gap.Width, out var left, out var crossGap);

                foreach (var axis in AxisTracks)
                {
                    // 针对每一列：主轴为垂直，容器高度为可用高度，按 MainAlignment 计算起始偏移和间隔
                    axis.CalculateMainAlignment(InnerBounds.Height, Gap.Height, MainAlignment, out var top, out var mainGap);

                    foreach (var element in axis.Elements)
                    {
                        // 针对每个元素：计算在当前列（交叉轴宽度为 axis.CrossAxisSize）上的偏移
                        float itemCrossOffset = CrossAlignment.CalculateCrossOffset(axis.AvailableCrossSpace, element.OuterBounds.Width);
                        element.SetLayoutOffset(left + itemCrossOffset, top);
                        top += element.OuterBounds.Height + mainGap;
                    }

                    left += axis.AvailableCrossSpace + crossGap;
                }
                break;
            }
        }
    }

}