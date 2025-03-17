using SilkyUIFramework.MyElement;
using Terraria.UI;

namespace SilkyUIFramework;

public partial class ViewGroup
{
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

    /// <summary>
    /// 如果主轴方向上容器大小为自适应, 则不换行
    /// </summary>
    public bool MainAxisFixed => LayoutDirection == LayoutDirection.Column ? !AutomaticHeight : !AutomaticWidth;

    public FlexDirection FlexDirection
    {
        get => _flexDirection;
        set
        {
            if (value == _flexDirection) return;
            _flexDirection = value;
            MarkDirty();
        }
    }

    private FlexDirection _flexDirection;

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

    /// <summary>
    /// 交叉轴内容对其方式
    /// </summary>
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
                // 计算所有 flex 行在垂直方向（交叉轴）的整体起始偏移和间隔
                CalculateCrossContentAlignment(InnerBounds.Height, Gap.Height, out var startOffset, out var crossGap);

                foreach (var axis in AxisTracks)
                {
                    // 针对每一行：主轴为水平，容器宽度为可用宽度，按 MainAlignment 计算起始偏移和间隔
                    CalculateMainAlignment(axis, InnerBounds.Width, Gap.Width, MainAlignment, e => e.OuterBounds.Width, out var left, out var mainGap);

                    foreach (var element in axis.Elements)
                    {
                        // 针对每个元素：计算在当前行（交叉轴高度为 axis.CrossAxisSize）上的偏移
                        float itemCrossOffset = CalculateItemCrossOffset(axis.CrossAxisContainer, element.OuterBounds.Height, CrossAlignment);
                        element.SetLayoutOffset(left, startOffset + itemCrossOffset);
                        left += element.OuterBounds.Width + mainGap;
                    }

                    startOffset += axis.CrossAxisContainer + crossGap;
                }
                break;
            }
            case LayoutDirection.Column:
            {
                // 计算所有 flex 列在水平方向（交叉轴）的整体起始偏移和间隔
                CalculateCrossContentAlignment(InnerBounds.Width, Gap.Width, out var startOffset, out var crossGap);

                foreach (var axis in AxisTracks)
                {
                    // 针对每一列：主轴为垂直，容器高度为可用高度，按 MainAlignment 计算起始偏移和间隔
                    CalculateMainAlignment(axis, InnerBounds.Height, Gap.Height, MainAlignment, e => e.OuterBounds.Height, out var top, out var mainGap);

                    foreach (var element in axis.Elements)
                    {
                        // 针对每个元素：计算在当前列（交叉轴宽度为 axis.CrossAxisSize）上的偏移
                        float itemCrossOffset = CalculateItemCrossOffset(axis.CrossAxisContainer, element.OuterBounds.Width, CrossAlignment);
                        element.SetLayoutOffset(startOffset + itemCrossOffset, top);
                        top += element.OuterBounds.Height + mainGap;
                    }

                    startOffset += axis.CrossAxisContainer + crossGap;
                }
                break;
            }
        }
    }

    /// <summary>
    /// 计算交叉轴（多行或多列）整体对齐：得到起始偏移和行（或列）间的间隔
    /// </summary>
    /// <param name="containerSize">容器大小（Row 时为高度，Column 时为宽度）</param>
    /// <param name="gapSize">基础间隔（Row 时为 Gap.Height，Column 时为 Gap.Width）</param>
    /// <param name="startOffset">返回计算得到的起始偏移（Row 时为 top，Column 时为 left）</param>
    /// <param name="crossGap">返回计算得到的行（或列）间间隔</param>
    private void CalculateCrossContentAlignment(float containerSize, float gapSize, out float startOffset, out float crossGap)
    {
        switch (CrossContentAlignment)
        {
            default:
            case CrossContentAlignment.Start:
            case CrossContentAlignment.Stretch:
            {
                startOffset = 0f;
                crossGap = gapSize;
                break;
            }
            case CrossContentAlignment.Center:
            {
                crossGap = gapSize;
                var crossContentSize = AxisTracks.CrossAxisContentSum() + AxisTracks.CrossGapSum(gapSize);
                startOffset = (containerSize - crossContentSize) / 2f;
                break;
            }
            case CrossContentAlignment.End:
            {
                crossGap = gapSize;
                var crossContentSize = AxisTracks.CrossAxisContentSum() + AxisTracks.CrossGapSum(gapSize);
                startOffset = containerSize - crossContentSize;
                break;
            }
            case CrossContentAlignment.SpaceEvenly:
            {
                var crossContentSize = AxisTracks.CrossAxisContentSum();
                crossGap = (containerSize - crossContentSize) / (AxisTracks.Count + 1);
                startOffset = crossGap;
                break;
            }
            case CrossContentAlignment.SpaceBetween:
            {
                var crossContentSize = AxisTracks.CrossAxisContentSum();
                crossGap = (AxisTracks.Count > 1) ? (containerSize - crossContentSize) / (AxisTracks.Count - 1) : 0f;
                startOffset = 0f;
                break;
            }
        }
    }

    private void CalculateMainAlignment(
        AxisTrack axisTrack, float availableSize, float gapSize, MainAlignment alignment,
        Func<UIView, float> getSize, out float startOffset, out float calculatedGap)
    {
        int count = axisTrack.Elements.Count;
        if (count == 0)
        {
            startOffset = 0f;
            calculatedGap = gapSize;
            return;
        }

        float totalSize = 0f;
        foreach (var element in axisTrack.Elements)
        {
            totalSize += getSize(element);
        }

        float totalGap = gapSize * (count - 1);
        float contentSize = totalSize + totalGap;

        switch (alignment)
        {
            case MainAlignment.Start:
                startOffset = 0f;
                calculatedGap = gapSize;
                break;
            case MainAlignment.End:
                startOffset = availableSize - contentSize;
                calculatedGap = gapSize;
                break;
            case MainAlignment.Center:
                startOffset = (availableSize - contentSize) / 2f;
                calculatedGap = gapSize;
                break;
            case MainAlignment.SpaceEvenly:
                calculatedGap = (availableSize - totalSize) / (count + 1);
                startOffset = calculatedGap;
                break;
            case MainAlignment.SpaceBetween:
                if (count > 1)
                {
                    calculatedGap = (availableSize - totalSize) / (count - 1);
                    startOffset = 0f;
                }
                else
                {
                    calculatedGap = 0f;
                    startOffset = (availableSize - totalSize) / 2f;
                }
                break;
            default:
                startOffset = 0f;
                calculatedGap = gapSize;
                break;
        }
    }


    /// <summary>
    /// 计算元素在交叉轴上的偏移
    /// </summary>
    private static float CalculateItemCrossOffset(float availableSize, float itemSize, CrossAlignment alignment) => alignment switch
    {
        CrossAlignment.Center => (availableSize - itemSize) / 2f,
        CrossAlignment.End => availableSize - itemSize,
        CrossAlignment.Stretch or CrossAlignment.Start or _ => 0f,
    };

}