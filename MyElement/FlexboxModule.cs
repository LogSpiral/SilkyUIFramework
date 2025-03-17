using SilkyUIFramework.MyElement;

namespace SilkyUIFramework;

#region enums

public enum FlexDirection
{
    Row,
    Column,
}

/// <summary> 主轴对其方式 </summary>
public enum MainAlignment
{
    /// <summary> 总体靠左 </summary>
    Start,

    /// <summary> 总体靠右 </summary>
    End,

    /// <summary> 总体居中 </summary>
    Center,

    /// <summary> 平分空间 </summary>
    SpaceEvenly,

    /// <summary> 两端对齐 </summary>
    SpaceBetween,
}

/// <summary> 交叉轴对齐方式 </summary>
public enum CrossAlignment
{
    /// <summary> 总体靠上 </summary>
    Start,

    /// <summary> 总体居中 </summary>
    Center,

    /// <summary> 总体靠下 </summary>
    End,

    /// <summary> 拉伸 </summary>
    Stretch,
}

public enum CrossContentAlignment
{
    Start,
    Center,
    End,
    SpaceEvenly,
    SpaceBetween,
    Stretch,
}

#endregion

public static class FlexboxModule
{
    public static void WrapElements(this List<AxisTrack> flexboxAxes, float maxMainAxisSize, float gap,
        List<UIView> elements, Func<UIView, float> mainAxis, Func<UIView, float> crossAxis)
    {
        flexboxAxes.Clear();
        var axis = new AxisTrack(elements[0], mainAxis(elements[0]), crossAxis(elements[0]));

        for (var i = 1; i < elements.Count; i++)
        {
            var element = elements[i];

            var mainAxisSize = mainAxis(element);
            var crossAxisSize = crossAxis(element);

            if (axis.MainSize + mainAxisSize + gap > maxMainAxisSize)
            {
                flexboxAxes.Add(axis);
                axis = new AxisTrack(element, mainAxisSize, crossAxisSize);
                continue;
            }

            axis.Elements.Add(element);
            axis.MainSize += mainAxisSize + gap;
            axis.CrossSize = MathF.Max(axis.CrossSize, crossAxisSize);
        }

        flexboxAxes.Add(axis);
    }

    public static void Measure(this List<AxisTrack> flexboxAxes, float gap, out float mainAxisSize,
        out float crossAxisSize)
    {
        mainAxisSize = 0f;
        crossAxisSize = 0f;

        foreach (var axis in flexboxAxes)
        {
            mainAxisSize = Math.Max(mainAxisSize, axis.MainSize);
            crossAxisSize += axis.CrossSize;
        }

        crossAxisSize += (flexboxAxes.Count - 1) * gap;
    }

    public static void ProcessRowAxisTracks(this List<AxisTrack> axisTracks, CrossAlignment crossAlignment, Size innerSize, float gap)
    {
        var crossStretch = crossAlignment == CrossAlignment.Stretch;

        foreach (var axisTrack in axisTracks)
        {
            float? assignValue = crossStretch ? axisTrack.AvailableCrossSpace : null;
            var remaining = innerSize.Width - axisTrack.MainSize;

            switch (remaining)
            {
                case 0:
                    axisTrack.Elements.ForEach(el => el.Trim(innerSize, outerHeight: assignValue));
                    break;
                case > 0:
                {
                    var grow = axisTrack.CalculateTotalGrow();
                    if (grow == 0)
                    {
                        axisTrack.Elements.ForEach(el => el.Trim(innerSize, outerHeight: assignValue));
                        break;
                    }

                    var each = remaining / grow;
                    foreach (var element in axisTrack.Elements)
                    {
                        element.Trim(innerSize, element.OuterBounds.Width + each * element.FlexGrow, assignValue);
                    }

                    axisTrack.MainSize = axisTrack.Elements.Sum(el => el.OuterBounds.Width) + axisTrack.CalculateTotalGap(gap);

                    break;
                }
                default:
                {
                    var shrink = axisTrack.CalculateTotalShrink();
                    if (shrink == 0)
                    {
                        axisTrack.Elements.ForEach(el => el.Trim(innerSize, outerHeight: assignValue));
                        break;
                    }

                    var each = remaining / shrink;
                    foreach (var element in axisTrack.Elements)
                    {
                        var newWidth = element.OuterBounds.Width + each * element.FlexShrink;
                        element.Trim(innerSize, newWidth, assignValue);
                    }

                    axisTrack.MainSize = axisTrack.Elements.Sum(el => el.OuterBounds.Width) + axisTrack.CalculateTotalGap(gap);

                    break;
                }
            }
        }
    }

    /// <summary>
    /// 处理每个轴轨道中子元素的 FlexGrow 和 FlexShrink 逻辑
    /// </summary>
    public static void ProcessColumnAxisTracks(this List<AxisTrack> axisTracks, CrossAlignment crossAlignment, Size innerSize, float gap)
    {
        var crossStretch = crossAlignment == CrossAlignment.Stretch;

        foreach (var axisTrack in axisTracks)
        {
            float? assignValue = crossStretch ? axisTrack.AvailableCrossSpace : null;
            var remaining = innerSize.Height - axisTrack.MainSize;

            switch (remaining)
            {
                case 0:
                    axisTrack.Elements.ForEach(el => el.Trim(innerSize, outerWidth: assignValue));
                    break;
                case > 0:
                {
                    var grow = axisTrack.CalculateTotalGrow();
                    if (grow == 0)
                    {
                        axisTrack.Elements.ForEach(el => el.Trim(innerSize, outerWidth: assignValue));
                        break;
                    }

                    var each = remaining / grow;
                    foreach (var element in axisTrack.Elements)
                    {
                        var newHeight = element.OuterBounds.Height + each * element.FlexGrow;
                        element.Trim(innerSize, assignValue, newHeight);
                    }

                    axisTrack.MainSize = axisTrack.Elements.Sum(el => el.OuterBounds.Height) + axisTrack.CalculateTotalGap(gap);

                    break;
                }
                default:
                {
                    var shrink = axisTrack.CalculateTotalShrink();
                    if (shrink == 0)
                    {
                        axisTrack.Elements.ForEach(el => el.Trim(innerSize, outerWidth: assignValue));
                        return;
                    }

                    var each = remaining / shrink;
                    foreach (var element in axisTrack.Elements)
                    {
                        var newHeight = element.OuterBounds.Height + each * element.FlexShrink;
                        element.Trim(innerSize, assignValue, newHeight);
                    }

                    axisTrack.MainSize = axisTrack.Elements.Sum(el => el.OuterBounds.Height) + axisTrack.CalculateTotalGap(gap);

                    break;
                }
            }
        }
    }

    public static void AdjustAxisTracksCrossAxisSize(this List<AxisTrack> axisTracks,
        CrossContentAlignment crossContentAlignment, float crossAxisSpace, float crossGap)
    {
        foreach (var axisTrack in axisTracks)
        {
            axisTrack.AvailableCrossSpace = axisTrack.CrossSize;
        }

        if (crossContentAlignment != CrossContentAlignment.Stretch) return;

        var remaining = crossAxisSpace - axisTracks.CalculateCrossSize(crossGap);
        if (remaining <= 0) return;

        var each = remaining / axisTracks.Count;
        foreach (var axisTrack in axisTracks)
        {
            axisTrack.AvailableCrossSpace += each;
        }
    }

    public static float CalculateCrossAxisContent(this List<AxisTrack> flexboxAxes) =>
        flexboxAxes.Sum(axis => axis.CrossSize);

    public static float CalculateCrossGap(this List<AxisTrack> flexboxAxes, float gap) => (flexboxAxes.Count - 1) * gap;

    public static float CalculateCrossSize(this List<AxisTrack> flexboxAxes, float gap) =>
        flexboxAxes.CalculateCrossAxisContent() + flexboxAxes.CalculateCrossGap(gap);

    public static void CalculateCrossContentAlignment(this List<AxisTrack> axisTracks,
        CrossContentAlignment crossContentAlignment, float containerSize, float gapSize, out float startOffset,
        out float crossGap)
    {
        switch (crossContentAlignment)
        {
            default:
            case CrossContentAlignment.Start:
            case CrossContentAlignment.Stretch:
            {
                startOffset = 0f;
                crossGap = gapSize;
                return;
            }
            case CrossContentAlignment.Center:
            {
                crossGap = gapSize;
                var crossContentSize = axisTracks.CalculateCrossAxisContent() + axisTracks.CalculateCrossGap(gapSize);
                startOffset = (containerSize - crossContentSize) / 2f;
                return;
            }
            case CrossContentAlignment.End:
            {
                crossGap = gapSize;
                var crossContentSize = axisTracks.CalculateCrossAxisContent() + axisTracks.CalculateCrossGap(gapSize);
                startOffset = containerSize - crossContentSize;
                return;
            }
            case CrossContentAlignment.SpaceEvenly:
            {
                var crossContentSize = axisTracks.CalculateCrossAxisContent();
                crossGap = (containerSize - crossContentSize) / (axisTracks.Count + 1);
                startOffset = crossGap;
                return;
            }
            case CrossContentAlignment.SpaceBetween:
            {
                var crossContentSize = axisTracks.CalculateCrossAxisContent();
                crossGap = (axisTracks.Count > 1) ? (containerSize - crossContentSize) / (axisTracks.Count - 1) : 0f;
                startOffset = 0f;
                return;
            }
        }
    }

    public static void CalculateMainAlignment(this AxisTrack axisTrack,
        float availableMainSize, float baseGap, MainAlignment mainAlignment, out float mainStartOffset,
        out float adjustedGap)
    {
        int elementCount = axisTrack.Elements.Count;
        if (elementCount == 0)
        {
            mainStartOffset = 0f;
            adjustedGap = baseGap;
            return;
        }

        float totalBaseGap = baseGap * (elementCount - 1);

        switch (mainAlignment)
        {
            default:
            case MainAlignment.Start:
                mainStartOffset = 0f;
                adjustedGap = baseGap;
                break;
            case MainAlignment.Center:
                mainStartOffset = (availableMainSize - axisTrack.MainSize) / 2f;
                adjustedGap = baseGap;
                break;
            case MainAlignment.End:
                mainStartOffset = availableMainSize - axisTrack.MainSize;
                adjustedGap = baseGap;
                break;
            case MainAlignment.SpaceEvenly:
            {
                var totalContentWithoutGaps = axisTrack.MainSize - totalBaseGap;
                adjustedGap = (availableMainSize - totalContentWithoutGaps) / (elementCount + 1);
                mainStartOffset = adjustedGap;
                break;
            }
            case MainAlignment.SpaceBetween:
            {
                var totalContentWithoutGaps = axisTrack.MainSize - totalBaseGap;
                if (elementCount > 1)
                {
                    adjustedGap = (availableMainSize - totalContentWithoutGaps) / (elementCount - 1);
                    mainStartOffset = 0f;
                }
                else
                {
                    adjustedGap = 0f;
                    mainStartOffset = (availableMainSize - totalContentWithoutGaps) / 2f;
                }

                break;
            }
        }
    }

    /// <summary>
    /// 计算元素在交叉轴方向的对齐偏移量
    /// </summary>
    public static float CalculateCrossOffset(this CrossAlignment crossAlignment, float crossAvailableSize,
        float itemCrossSize) => crossAlignment switch
        {
            CrossAlignment.Center => (crossAvailableSize - itemCrossSize) / 2f,
            CrossAlignment.End => crossAvailableSize - itemCrossSize,
            CrossAlignment.Stretch or CrossAlignment.Start or _ => 0f,
        };
}