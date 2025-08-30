using System.Collections;
using static SilkyUIFramework.CrossAlignment;

namespace SilkyUIFramework;

public class FlexboxModule(UIElementGroup parent) : IEnumerable<FlexLine>, ILayoutModule
{
    private UIElementGroup Parent { get; } = parent;

    private FlexDirection FlexDirection => Parent.FlexDirection;
    private bool FlexWrap => Parent.FlexWrap;
    private MainAlignment MainAlignment => Parent.MainAlignment;
    private CrossAlignment CrossAlignment => Parent.CrossAlignment;
    private CrossContentAlignment CrossContentAlignment => Parent.CrossContentAlignment;
    private List<FlexLine> FlexLines { get; } = [];

    public IEnumerator<FlexLine> GetEnumerator() => FlexLines.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear() => FlexLines.Clear();

    public float MaxMainSize() => FlexLines.Max(line => line.MainSize);

    private void UpdateMainSizeByRow(float gap)
    {
        foreach (var line in FlexLines) line.UpdateMainSizeByRow(gap);
    }

    private void UpdateMainSizeByColumn(float gap)
    {
        foreach (var line in FlexLines) line.UpdateMainSizeByColumn(gap);
    }

    private void SingleRow(IReadOnlyList<UIView> elements, float gap)
    {
        FlexLines.Clear();
        FlexLines.Add(FlexLine.CreateSingleRow(elements, gap));
    }

    private void SingleColumn(IReadOnlyList<UIView> elements, float gap)
    {
        FlexLines.Clear();
        FlexLines.Add(FlexLine.CreateSingleColumn(elements, gap));
    }

    public void OnUpdateChildrenLayoutOffset()
    {
        var bounds = Parent.InnerBounds;
        var gap = Parent.Gap;

        switch (FlexDirection)
        {
            case FlexDirection.Row:
            {
                CalculateCrossContentAlignment(bounds.Height, gap.Height, out var top, out var crossGap);

                foreach (var flexLine in FlexLines)
                {
                    CalculateMainAlignment(flexLine, bounds.Width, gap.Width, out var left, out var mainGap);

                    foreach (var el in flexLine)
                    {
                        var crossOffset = CalculateCrossOffset(flexLine.CrossSpace, el.OuterBounds.Height);
                        el.SetLayoutOffset(left, top + crossOffset);
                        left += el.OuterBounds.Width + mainGap;
                    }

                    top += flexLine.CrossSpace + crossGap;
                }

                break;
            }
            case FlexDirection.Column:
            {
                CalculateCrossContentAlignment(bounds.Width, gap.Width, out var left, out var crossGap);

                foreach (var flexLine in FlexLines)
                {
                    CalculateMainAlignment(flexLine, bounds.Height, gap.Height, out var top, out var mainGap);

                    foreach (var el in flexLine)
                    {
                        var itemCrossOffset = CalculateCrossOffset(flexLine.CrossSpace, el.OuterBounds.Width);
                        el.SetLayoutOffset(left + itemCrossOffset, top);
                        top += el.OuterBounds.Height + mainGap;
                    }

                    left += flexLine.CrossSpace + crossGap;
                }

                break;
            }
            default: goto case FlexDirection.Row;
        }
    }

    public void OnPrepare()
    {
        switch (FlexDirection)
        {
            case FlexDirection.Row:
            {
                MeasureSize(Parent.Gap.Width, out var main, out var cross);
                if (Parent.FitWidth) Parent.SetExactInnerWidth(main);
                if (Parent.FitHeight) Parent.SetExactInnerHeight(cross);
                break;
            }
            case FlexDirection.Column:
            {
                MeasureSize(Parent.Gap.Height, out var main, out var cross);
                if (Parent.FitWidth) Parent.SetExactInnerWidth(cross);
                if (Parent.FitHeight) Parent.SetExactInnerHeight(main);
                break;
            }
            default: goto case FlexDirection.Row;
        }
    }

    public void OnPrepareChildren()
    {
        switch (FlexDirection)
        {
            case FlexDirection.Row:
            {
                if (!FlexWrap || Parent.FitWidth) SingleRow(Parent.LayoutChildren, Parent.Gap.Width);
                else WrapRow(Parent.LayoutChildren, Parent.InnerBounds.Width, Parent.Gap.Width);
                break;
            }
            case FlexDirection.Column:
            {
                if (!FlexWrap || Parent.FitHeight) SingleColumn(Parent.LayoutChildren, Parent.Gap.Height);
                else WrapColumn(Parent.LayoutChildren, Parent.InnerBounds.Height, Parent.Gap.Height);
                break;
            }
            default: goto case FlexDirection.Row;
        }
    }

    public void OnResizeChildrenWidth()
    {
        var innerSize = Parent.InnerBounds.Size;
        var gap = Parent.Gap;

        switch (FlexDirection)
        {
            case FlexDirection.Row:
            {
                // 宽度可能被父元素拉伸, 再次计算元素换行
                if (FlexWrap) WrapRow(Parent.LayoutChildren, innerSize.Width, gap.Width);
                else UpdateMainSizeByRow(gap.Width);

                // 拉伸或者压缩宽度
                RowModeGrowOrShrink(innerSize, gap.Width);
                break;
            }
            case FlexDirection.Column:
            {
                ResizeCrossAxis(innerSize.Width, gap.Width);
                if (CrossAlignment == Stretch)
                {
                    foreach (var flexLine in FlexLines)
                    {
                        foreach (var el in flexLine.Where(el =>
                                     el.FitWidth || el.OuterBounds.Width < flexLine.CrossSpace))
                        {
                            el.SetExactOuterWidth(flexLine.CrossSpace);
                        }
                    }
                }

                break;
            }
            default: goto case FlexDirection.Row;
        }
    }

    public void OnRecalculateHeight()
    {
        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                if (Parent.FitHeight)
                {
                    var content = CalculateCrossSize(Parent.Gap.Height);
                    Parent.SetExactInnerHeight(MathHelper.Clamp(content, Parent.MinInnerHeight, Parent.MaxInnerHeight));
                }

                break;
            }
            case FlexDirection.Column:
            {
                if (Parent.FitHeight)
                {
                    var content = MaxMainSize();
                    Parent.SetExactInnerHeight(MathHelper.Clamp(content, Parent.MinInnerHeight, Parent.MaxInnerHeight));
                }

                break;
            }
        }
    }

    public void OnResizeChildrenHeight()
    {
        var innerSize = Parent.InnerBounds.Size;
        var gap = Parent.Gap;

        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                ResizeCrossAxis(innerSize.Height, gap.Height);
                if (CrossAlignment == Stretch)
                {
                    foreach (var flexLine in FlexLines)
                    {
                        foreach (var el in flexLine.Where(el =>
                                     el.FitHeight || el.OuterBounds.Height < flexLine.CrossSpace))
                        {
                            el.SetExactOuterHeight(flexLine.CrossSpace);
                        }
                    }
                }

                break;
            }
            case FlexDirection.Column:
            {
                if (FlexWrap)
                {
                    var maxMainAxisSize = innerSize.Height;
                    WrapColumn(Parent.LayoutChildren, maxMainAxisSize, gap.Height);
                }
                else
                {
                    UpdateMainSizeByColumn(gap.Height);
                }

                ColumnModeGrowOrShrink(innerSize, gap.Height);
                break;
            }
        }
    }

    private void WrapRow(IReadOnlyList<UIView> elements, float maxMainAxisSize, float gap)
    {
        FlexLines.Clear();

        var flexLine = FlexLine.CreateRow(elements[0]);
        FlexLines.Add(flexLine);

        for (var i = 1; i < elements.Count; i++)
        {
            var element = elements[i];

            var mainAxisSize = element.OuterBounds.Width;
            var crossAxisSize = element.OuterBounds.Height;

            if (flexLine.MainSize + mainAxisSize + gap <= maxMainAxisSize)
            {
                flexLine.Elements.Add(element);
                flexLine.MainSize += mainAxisSize + gap;
                flexLine.CrossSize = MathF.Max(flexLine.CrossSize, crossAxisSize);
                continue;
            }

            flexLine = FlexLine.CreateRow(element);
            FlexLines.Add(flexLine);
        }
    }

    private void WrapColumn(IReadOnlyList<UIView> elements, float maxMainAxisSize, float gap)
    {
        FlexLines.Clear();

        var flexLine = FlexLine.CreateColumn(elements[0]);
        FlexLines.Add(flexLine);

        for (var i = 1; i < elements.Count; i++)
        {
            var element = elements[i];

            var mainAxisSize = element.OuterBounds.Height;
            var crossAxisSize = element.OuterBounds.Width;

            if (flexLine.MainSize + mainAxisSize + gap <= maxMainAxisSize)
            {
                flexLine.Elements.Add(element);
                flexLine.MainSize += mainAxisSize + gap;
                flexLine.CrossSize = MathF.Max(flexLine.CrossSize, crossAxisSize);
                continue;
            }

            flexLine = FlexLine.CreateColumn(element);
            FlexLines.Add(flexLine);
        }
    }

    private void MeasureSize(float gap, out float mainAxisSize, out float crossAxisSize)
    {
        mainAxisSize = 0f;
        crossAxisSize = 0f;

        foreach (var flexLine in FlexLines)
        {
            mainAxisSize = Math.Max(mainAxisSize, flexLine.MainSize);
            crossAxisSize += flexLine.CrossSize;
        }

        crossAxisSize += (FlexLines.Count - 1) * gap;
    }

    private void RowModeGrowOrShrink(Size innerSize, float gap)
    {
        foreach (var flexLine in FlexLines)
        {
            var remaining = innerSize.Width - flexLine.MainSize;
            switch (remaining)
            {
                case > 0:
                {
                    var growElements = flexLine.Elements
                        .Where(el => el.FlexGrow > 0)
                        .Select(el => new { Element = el, AvailableGrowth = el.MaxOuterWidth - el.OuterBounds.Width })
                        .Where(el => el.AvailableGrowth > 0)
                        .OrderBy(item => item.AvailableGrowth)
                        .ToList();
                    var totalGrow = growElements.Sum(el => el.Element.FlexGrow);

                    foreach (var item in growElements)
                    {
                        var each = remaining / totalGrow;
                        var alloc = Math.Min(item.AvailableGrowth, each * item.Element.FlexGrow);

                        item.Element.SetExactOuterWidth(item.Element.OuterBounds.Width + alloc);

                        remaining -= alloc;
                        totalGrow -= item.Element.FlexGrow;
                    }

                    break;
                }
                case < 0:
                {
                    var shrinkElements = flexLine.Elements
                        .Where(el => el.FlexShrink > 0)
                        .Select(el => new { Element = el, AvailableShrink = el.MinOuterWidth - el.OuterBounds.Width })
                        .Where(el => el.AvailableShrink < 0)
                        .OrderByDescending(item => item.AvailableShrink)
                        .ToList();
                    var totalShrink = shrinkElements.Sum(el => el.Element.FlexShrink);

                    foreach (var item in shrinkElements)
                    {
                        var each = remaining / totalShrink;
                        var alloc = Math.Max(item.AvailableShrink, each * item.Element.FlexShrink);

                        item.Element.SetExactOuterWidth(item.Element.OuterBounds.Width + alloc);

                        remaining -= alloc;
                        totalShrink -= item.Element.FlexShrink;
                    }

                    break;
                }
            }

            flexLine.MainSize = flexLine.Elements.Sum(el => el.OuterBounds.Width) + flexLine.GetFenceGap(gap);
        }
    }

    private void ColumnModeGrowOrShrink(Size innerSize, float gap)
    {
        // 主轴 grow 或 shrink
        foreach (var flexLine in FlexLines)
        {
            var remaining = innerSize.Height - flexLine.MainSize;

            switch (remaining)
            {
                case > 0:
                {
                    var sortedElements = flexLine.Elements
                        .Where(el => el.FlexGrow > 0)
                        .Select(el => new { Element = el, AvailableGrowth = el.MaxOuterHeight - el.OuterBounds.Height })
                        .Where(item => item.AvailableGrowth > 0)
                        .OrderBy(item => item.AvailableGrowth)
                        .ToList();
                    var totalGrow = sortedElements.Sum(item => item.Element.FlexGrow);

                    foreach (var item in sortedElements)
                    {
                        if (totalGrow <= 0) break;

                        var each = remaining / totalGrow;
                        var alloc = Math.Min(item.AvailableGrowth, each * item.Element.FlexGrow);

                        item.Element.SetExactOuterHeight(item.Element.OuterBounds.Height + alloc);

                        remaining -= alloc;
                        totalGrow -= item.Element.FlexGrow;
                    }

                    flexLine.MainSize = flexLine.Elements.Sum(el => el.OuterBounds.Height) + flexLine.GetFenceGap(gap);

                    break;
                }
                case < 0:
                {
                    var sortedElements = flexLine.Elements
                        .Where(el => el.FlexShrink > 0)
                        .Select(el => new { Element = el, AvailableShrink = el.MinOuterHeight - el.OuterBounds.Height })
                        .Where(el => el.AvailableShrink < 0)
                        .OrderByDescending(item => item.AvailableShrink)
                        .ToList();
                    var totalShrink = sortedElements.Sum(el => el.Element.FlexShrink);

                    foreach (var item in sortedElements)
                    {
                        if (totalShrink <= 0 || remaining >= 0) break;

                        var each = remaining / totalShrink;
                        var alloc = Math.Max(item.AvailableShrink, each * item.Element.FlexShrink);

                        item.Element.SetExactOuterHeight(item.Element.OuterBounds.Height + alloc);

                        //Main.NewText($"Height: {item.Element.OuterBounds.Height} alloc: {alloc} each: {each} totalShrink:{totalShrink} remaining:{remaining}");

                        remaining -= alloc;
                        totalShrink -= item.Element.FlexShrink;
                    }

                    flexLine.MainSize = flexLine.Elements.Sum(el => el.OuterBounds.Height) + flexLine.GetFenceGap(gap);
                    break;
                }
                default: break;
            }
        }
    }

    private void ResizeCrossAxis(float crossAxisSpace, float crossGap)
    {
        foreach (var flexLine in FlexLines)
        {
            flexLine.CrossSpace = flexLine.CrossSize;
        }

        if (CrossContentAlignment != CrossContentAlignment.Stretch) return;

        var remaining = crossAxisSpace - CalculateCrossSize(crossGap);
        if (remaining <= 0) return;

        var each = remaining / FlexLines.Count;
        foreach (var flexLine in FlexLines)
        {
            flexLine.CrossSpace += each;
        }
    }

    private float CalculateCrossSize(float gap) => CalculateCrossAxisContent() + (FlexLines.Count - 1) * gap;

    private float CalculateCrossAxisContent() => FlexLines.Sum(flexLine => flexLine.CrossSize);

    private void CalculateCrossContentAlignment(float containerSize, float gapSize, out float startOffset,
        out float crossGap)
    {
        switch (CrossContentAlignment)
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
                var crossContentSize = CalculateCrossSize(gapSize);
                startOffset = (containerSize - crossContentSize) / 2f;
                return;
            }
            case CrossContentAlignment.End:
            {
                crossGap = gapSize;
                var crossContentSize = CalculateCrossSize(gapSize);
                startOffset = containerSize - crossContentSize;
                return;
            }
            case CrossContentAlignment.SpaceEvenly:
            {
                var crossContentSize = CalculateCrossAxisContent();
                crossGap = (containerSize - crossContentSize) / (FlexLines.Count + 1);
                startOffset = crossGap;
                return;
            }
            case CrossContentAlignment.SpaceBetween:
            {
                var crossContentSize = CalculateCrossAxisContent();
                crossGap = FlexLines.Count > 1 ? (containerSize - crossContentSize) / (FlexLines.Count - 1) : 0f;
                startOffset = 0f;
                return;
            }
        }
    }

    private void CalculateMainAlignment(FlexLine flexLine,
        float availableMainSize, float baseGap, out float mainStartOffset, out float adjustedGap)
    {
        var elementCount = flexLine.Elements.Count;
        if (elementCount == 0)
        {
            mainStartOffset = 0f;
            adjustedGap = baseGap;
            return;
        }

        var totalBaseGap = baseGap * (elementCount - 1);

        switch (MainAlignment)
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

    private float CalculateCrossOffset(float crossAvailableSize,
        float itemCrossSize) => CrossAlignment switch
    {
        Center => (crossAvailableSize - itemCrossSize) / 2f,
        End => crossAvailableSize - itemCrossSize,
        Stretch or Start or _ => 0f,
    };
}