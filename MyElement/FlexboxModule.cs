using SilkyUIFramework.MyElement;
using Terraria;

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
    public static void WrapRow(this List<UIView> elements, List<FlexLine> flexLines, float maxMainAxisSize, float gap)
    {
        flexLines.Clear();
        var element = elements[0];
        var flexLine = new FlexLine(element, element.OuterBounds.Width, element.OuterBounds.Height);

        for (var i = 1; i < elements.Count; i++)
        {
            element = elements[i];

            var mainAxisSize = element.OuterBounds.Width;
            var crossAxisSize = element.OuterBounds.Height;

            if (flexLine.MainSize + mainAxisSize + gap > maxMainAxisSize)
            {
                flexLines.Add(flexLine);
                flexLine = new FlexLine(element, mainAxisSize, crossAxisSize);
                continue;
            }

            flexLine.Elements.Add(element);
            flexLine.MainSize += mainAxisSize + gap;
            flexLine.CrossSize = MathF.Max(flexLine.CrossSize, crossAxisSize);
        }

        flexLines.Add(flexLine);
    }

    public static void WrapColumn(this List<UIView> elements, List<FlexLine> flexLines, float maxMainAxisSize, float gap)
    {
        flexLines.Clear();
        var element = elements[0];
        var flexLine = new FlexLine(element, element.OuterBounds.Height, element.OuterBounds.Width);

        for (var i = 1; i < elements.Count; i++)
        {
            element = elements[i];

            var mainAxisSize = element.OuterBounds.Height;
            var crossAxisSize = element.OuterBounds.Width;

            if (flexLine.MainSize + mainAxisSize + gap > maxMainAxisSize)
            {
                flexLines.Add(flexLine);
                flexLine = new FlexLine(element, mainAxisSize, crossAxisSize);
                continue;
            }

            flexLine.Elements.Add(element);
            flexLine.MainSize += mainAxisSize + gap;
            flexLine.CrossSize = MathF.Max(flexLine.CrossSize, crossAxisSize);
        }

        flexLines.Add(flexLine);
    }

    public static void Measure(this List<FlexLine> flexLines, float gap, out float mainAxisSize,
        out float crossAxisSize)
    {
        mainAxisSize = 0f;
        crossAxisSize = 0f;

        foreach (var flexLine in flexLines)
        {
            mainAxisSize = Math.Max(mainAxisSize, flexLine.MainSize);
            crossAxisSize += flexLine.CrossSize;
        }

        crossAxisSize += (flexLines.Count - 1) * gap;
    }

    public static void ProcessFlexLinesByRow(this List<FlexLine> flexLines, CrossAlignment crossAlignment, Size space, Size innerSize, float gap)
    {
        // 交叉轴拉伸
        if (crossAlignment == CrossAlignment.Stretch)
        {
            foreach (FlexLine flexLine in flexLines)
            {
                foreach (var el in flexLine.Elements.Where(
                    el => el.AutomaticHeight && flexLine.AvailableCrossSpace > el.OuterBounds.Height))
                {
                    el.SpecifyHeight(Math.Min(el.MaxOuterHeight, flexLine.AvailableCrossSpace));
                }
            }
        }

        // 主轴 grow 或 shrink
        foreach (var flexLine in flexLines)
        {
            var remaining = innerSize.Width - flexLine.MainSize;
            switch (remaining)
            {
                case 0: break;
                // remaining > 0
                case > 0:
                {
                    var sortedElements = flexLine.Elements
                        .Where(el => el.FlexGrow > 0)
                        .Select(el => new { Element = el, AvailableGrowth = el.MaxOuterWidth - el.OuterBounds.Width })
                        .OrderBy(item => item.AvailableGrowth)
                        .ToList();
                    var totalGrow = sortedElements.Sum(el => el.Element.FlexGrow);

                    foreach (var item in sortedElements)
                    {
                        if (totalGrow <= 0)
                            break;

                        var each = remaining / totalGrow;
                        var alloc = Math.Min(item.AvailableGrowth, each * item.Element.FlexGrow);

                        item.Element.SpecifyWidth(item.Element.OuterBounds.Width + alloc);

                        remaining -= alloc;
                        totalGrow -= item.Element.FlexGrow;
                    }

                    flexLine.MainSize = flexLine.Elements.Sum(el => el.OuterBounds.Width) + flexLine.CalculateTotalGap(gap);

                    break;
                }
                // remaining < 0
                default:
                {
                    var sortedElements = flexLine.Elements
                        .Where(el => el.FlexShrink > 0)
                        .Select(el => new { Element = el, AvailableShrink = el.MinOuterWidth - el.OuterBounds.Width })
                        .OrderByDescending(item => item.AvailableShrink)
                        .ToList();
                    var totalShrink = sortedElements.Sum(el => el.Element.FlexShrink);

                    foreach (var item in sortedElements)
                    {
                        if (totalShrink <= 0 || remaining >= 0) break;

                        var each = remaining / totalShrink;
                        var alloc = Math.Max(item.AvailableShrink, each * item.Element.FlexShrink);

                        item.Element.SpecifyWidth(item.Element.OuterBounds.Width + alloc);

                        remaining -= alloc;
                        totalShrink -= item.Element.FlexShrink;
                    }

                    flexLine.MainSize = flexLine.Elements.Sum(el => el.OuterBounds.Width) + flexLine.CalculateTotalGap(gap);
                    break;
                }
            }
        }
    }

    public static void ProcessFlexLinesByColumn(this List<FlexLine> flexLines, CrossAlignment crossAlignment, Size space, Size innerSize, float gap)
    {
        if (crossAlignment == CrossAlignment.Stretch)
        {
            foreach (FlexLine flexLine in flexLines)
            {
                foreach (var el in flexLine.Elements.Where(
                    el => el.AutomaticWidth && flexLine.AvailableCrossSpace > el.OuterBounds.Width))
                {
                    el.SpecifyWidth(Math.Min(el.MaxOuterWidth, flexLine.AvailableCrossSpace));
                }
            }
        }

        // 主轴 grow 或 shrink
        foreach (var flexLine in flexLines)
        {
            var remaining = innerSize.Height - flexLine.MainSize;
            switch (remaining)
            {
                case 0: break;
                // remaining > 0
                case > 0:
                {
                    var sortedElements = flexLine.Elements
                        .Where(el => el.FlexGrow > 0)
                        .Select(el => new { Element = el, AvailableGrowth = el.MaxOuterHeight - el.OuterBounds.Height })
                        .OrderBy(item => item.AvailableGrowth)
                        .ToList();
                    var totalGrow = sortedElements.Sum(el => el.Element.FlexGrow);

                    foreach (var item in sortedElements)
                    {
                        if (totalGrow <= 0)
                            break;

                        var each = remaining / totalGrow;
                        var alloc = Math.Min(item.AvailableGrowth, each * item.Element.FlexGrow);

                        item.Element.SpecifyHeight(item.Element.OuterBounds.Height + alloc);

                        remaining -= alloc;
                        totalGrow -= item.Element.FlexGrow;
                    }

                    flexLine.MainSize = flexLine.Elements.Sum(el => el.OuterBounds.Height) + flexLine.CalculateTotalGap(gap);

                    break;
                }
                // remaining < 0
                default:
                {
                    var sortedElements = flexLine.Elements
                        .Where(el => el.FlexShrink > 0)
                        .Select(el => new { Element = el, AvailableShrink = el.MinOuterHeight - el.OuterBounds.Height })
                        .OrderByDescending(item => item.AvailableShrink)
                        .ToList();
                    var totalShrink = sortedElements.Sum(el => el.Element.FlexShrink);

                    foreach (var item in sortedElements)
                    {
                        if (totalShrink <= 0 || remaining >= 0) break;

                        var each = remaining / totalShrink;
                        var alloc = Math.Max(item.AvailableShrink, each * item.Element.FlexShrink);

                        item.Element.SpecifyHeight(item.Element.OuterBounds.Height + alloc);

                        remaining -= alloc;
                        totalShrink -= item.Element.FlexShrink;
                    }

                    flexLine.MainSize = flexLine.Elements.Sum(el => el.OuterBounds.Height) + flexLine.CalculateTotalGap(gap);
                    break;
                }
            }
        }
    }

    public static void AdjustFlexLinesCrossAxisSize(this List<FlexLine> flexLines,
        CrossContentAlignment crossContentAlignment, float crossAxisSpace, float crossGap)
    {
        foreach (var flexLine in flexLines)
        {
            flexLine.AvailableCrossSpace = flexLine.CrossSize;
        }

        if (crossContentAlignment != CrossContentAlignment.Stretch) return;

        var remaining = crossAxisSpace - flexLines.CalculateCrossSize(crossGap);
        if (remaining <= 0) return;

        var each = remaining / flexLines.Count;
        foreach (var flexLine in flexLines)
        {
            flexLine.AvailableCrossSpace += each;
        }
    }

    public static float CalculateCrossAxisContent(this List<FlexLine> flexLines) => flexLines.Sum(flexLine => flexLine.CrossSize);

    public static float CalculateCrossGap(this List<FlexLine> flexLines, float gap) => (flexLines.Count - 1) * gap;

    public static float CalculateCrossSize(this List<FlexLine> flexLines, float gap) =>
        flexLines.CalculateCrossAxisContent() + flexLines.CalculateCrossGap(gap);

    public static void CalculateCrossContentAlignment(this List<FlexLine> flexLines,
        CrossContentAlignment crossContentAlignment, float containerSize, float gapSize, out float startOffset, out float crossGap)
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
                var crossContentSize = flexLines.CalculateCrossAxisContent() + flexLines.CalculateCrossGap(gapSize);
                startOffset = (containerSize - crossContentSize) / 2f;
                return;
            }
            case CrossContentAlignment.End:
            {
                crossGap = gapSize;
                var crossContentSize = flexLines.CalculateCrossAxisContent() + flexLines.CalculateCrossGap(gapSize);
                startOffset = containerSize - crossContentSize;
                return;
            }
            case CrossContentAlignment.SpaceEvenly:
            {
                var crossContentSize = flexLines.CalculateCrossAxisContent();
                crossGap = (containerSize - crossContentSize) / (flexLines.Count + 1);
                startOffset = crossGap;
                return;
            }
            case CrossContentAlignment.SpaceBetween:
            {
                var crossContentSize = flexLines.CalculateCrossAxisContent();
                crossGap = (flexLines.Count > 1) ? (containerSize - crossContentSize) / (flexLines.Count - 1) : 0f;
                startOffset = 0f;
                return;
            }
        }
    }

    public static void CalculateMainAlignment(this FlexLine flexLine,
        float availableMainSize, float baseGap, MainAlignment mainAlignment, out float mainStartOffset,
        out float adjustedGap)
    {
        int elementCount = flexLine.Elements.Count;
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
                mainStartOffset = (availableMainSize - flexLine.MainSize) / 2f;
                adjustedGap = baseGap;
                break;
            case MainAlignment.End:
                mainStartOffset = availableMainSize - flexLine.MainSize;
                adjustedGap = baseGap;
                break;
            case MainAlignment.SpaceEvenly:
            {
                var totalContentWithoutGaps = flexLine.MainSize - totalBaseGap;
                adjustedGap = (availableMainSize - totalContentWithoutGaps) / (elementCount + 1);
                mainStartOffset = adjustedGap;
                break;
            }
            case MainAlignment.SpaceBetween:
            {
                var totalContentWithoutGaps = flexLine.MainSize - totalBaseGap;
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