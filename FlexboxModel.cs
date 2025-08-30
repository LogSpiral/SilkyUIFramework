using System.Collections;
using static SilkyUIFramework.CrossAlignment;

namespace SilkyUIFramework;

public partial class FlexboxModel : IEnumerable<FlexLine>
{
    private List<FlexLine> FlexLines { get; } = [];

    #region Cache Status

    private bool FlexWrap { get; set; }
    private FlexDirection FlexDirection { get; set; }
    private MainAlignment MainAlignment { get; set; }
    private CrossAlignment CrossAlignment { get; set; }
    private CrossContentAlignment CrossContentAlignment { get; set; }

    protected sealed override void UpdateCacheStatus()
    {
        base.UpdateCacheStatus();
        FlexDirection = Parent.FlexDirection;
        FlexWrap = Parent.FlexWrap;
        MainAlignment = Parent.MainAlignment;
        CrossAlignment = Parent.CrossAlignment;
        CrossContentAlignment = Parent.CrossContentAlignment;
    }

    #endregion

    #region Ienumerable

    public IEnumerator<FlexLine> GetEnumerator() => FlexLines.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    private float MaxMainSize() => FlexLines.Max(line => line.MainSize);

    private void UpdateMainSizeByRow()
    {
        foreach (var line in FlexLines) line.UpdateMainSizeByRow(Parent.Gap.Width);
    }

    private void UpdateMainSizeByColumn()
    {
        foreach (var line in FlexLines) line.UpdateMainSizeByColumn(Parent.Gap.Height);
    }

    private void SingleRow()
    {
        FlexLines.Clear();
        FlexLines.Add(FlexLine.CreateSingleRow(Parent.LayoutChildren, Parent.Gap.Width));
    }

    private void SingleColumn()
    {
        FlexLines.Clear();
        FlexLines.Add(FlexLine.CreateSingleColumn(Parent.LayoutChildren, Parent.Gap.Height));
    }

    private void WrapRow()
    {
        var container = Parent.InnerBounds.Width;
        var gap = Parent.Gap.Width;
        var elements = Parent.LayoutChildren;
        FlexLines.Clear();

        var line = FlexLine.CreateRow(elements[0]);
        FlexLines.Add(line);

        for (var i = 1; i < elements.Count; i++)
        {
            var element = elements[i];

            var mainAxisSize = element.OuterBounds.Width;
            var crossAxisSize = element.OuterBounds.Height;

            if (line.MainSize + mainAxisSize + gap <= container)
            {
                line.Elements.Add(element);
                line.MainSize += mainAxisSize + gap;
                line.CrossSize = MathF.Max(line.CrossSize, crossAxisSize);
                continue;
            }

            line = FlexLine.CreateRow(element);
            FlexLines.Add(line);
        }
    }

    private void WrapColumn()
    {
        var maxMainAxisSize = Parent.InnerBounds.Height;
        var gap = Parent.Gap.Height;
        var elements = Parent.LayoutChildren;
        FlexLines.Clear();

        var line = FlexLine.CreateColumn(elements[0]);
        FlexLines.Add(line);

        for (var i = 1; i < elements.Count; i++)
        {
            var element = elements[i];

            var mainAxisSize = element.OuterBounds.Height;
            var crossAxisSize = element.OuterBounds.Width;

            if (line.MainSize + mainAxisSize + gap <= maxMainAxisSize)
            {
                line.Elements.Add(element);
                line.MainSize += mainAxisSize + gap;
                line.CrossSize = MathF.Max(line.CrossSize, crossAxisSize);
                continue;
            }

            line = FlexLine.CreateColumn(element);
            FlexLines.Add(line);
        }
    }

    private void MeasureSize(float gap, out float mainSize, out float crossSize)
    {
        mainSize = 0f;
        crossSize = (FlexLines.Count - 1) * gap;

        foreach (var line in FlexLines)
        {
            mainSize = Math.Max(mainSize, line.MainSize);
            crossSize += line.CrossSize;
        }
    }

    private void RowModeGrowOrShrink()
    {
        var width = Parent.InnerBounds.Width;
        var gap = Parent.Gap.Width;

        foreach (var line in FlexLines)
        {
            var remaining = width - line.MainSize;
            switch (remaining)
            {
                case > 0:
                {
                    var growElements = line.Elements
                        .Where(el => el.FlexGrow > 0)
                        .Select(el => (Element: el, AvailableGrowth: el.MaxOuterWidth - el.OuterBounds.Width))
                        .Where(item => item.AvailableGrowth > 0)
                        .OrderBy(item => item.AvailableGrowth).ToArray();
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
                    var shrinkElements = line.Elements
                        .Where(el => el.FlexShrink > 0)
                        .Select(el => (Element: el, AvailableShrink: el.MinOuterWidth - el.OuterBounds.Width))
                        .Where(item => item.AvailableShrink < 0)
                        .OrderByDescending(item => item.AvailableShrink).ToArray();
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

            line.MainSize = line.Elements.Sum(el => el.OuterBounds.Width) + line.GetFenceGap(gap);
        }
    }

    private void ColumnModeGrowOrShrink()
    {
        var height = Parent.InnerBounds.Height;
        foreach (var line in FlexLines)
        {
            var remaining = height - line.MainSize;

            switch (remaining)
            {
                case > 0:
                {
                    var sortedElements = line.Elements
                        .Where(el => el.FlexGrow > 0)
                        .Select(el => (Element: el, AvailableGrowth: el.MaxOuterHeight - el.OuterBounds.Height))
                        .Where(item => item.AvailableGrowth > 0)
                        .OrderBy(item => item.AvailableGrowth).ToArray();
                    var totalGrow = sortedElements.Sum(item => item.Element.FlexGrow);

                    foreach (var (Element, AvailableGrowth) in sortedElements)
                    {
                        if (totalGrow <= 0) break;

                        var each = remaining / totalGrow;
                        var alloc = Math.Min(AvailableGrowth, each * Element.FlexGrow);

                        Element.SetExactOuterHeight(Element.OuterBounds.Height + alloc);

                        remaining -= alloc;
                        totalGrow -= Element.FlexGrow;
                    }

                    line.MainSize = line.Elements.Sum(el => el.OuterBounds.Height) + line.GetFenceGap(Parent.Gap.Height);

                    break;
                }
                case < 0:
                {
                    var sortedElements = line.Elements
                        .Where(el => el.FlexShrink > 0)
                        .Select(el => (Element: el, AvailableShrink: el.MinOuterHeight - el.OuterBounds.Height))
                        .Where(item => item.AvailableShrink < 0)
                        .OrderByDescending(item => item.AvailableShrink).ToArray();
                    var totalShrink = sortedElements.Sum(el => el.Element.FlexShrink);

                    foreach (var (Element, AvailableShrink) in sortedElements)
                    {
                        if (totalShrink <= 0 || remaining >= 0) break;

                        var each = remaining / totalShrink;
                        var alloc = Math.Max(AvailableShrink, each * Element.FlexShrink);

                        Element.SetExactOuterHeight(Element.OuterBounds.Height + alloc);

                        remaining -= alloc;
                        totalShrink -= Element.FlexShrink;
                    }

                    line.MainSize = line.Elements.Sum(el => el.OuterBounds.Height) + line.GetFenceGap(Parent.Gap.Height);
                    break;
                }
            }
        }
    }

    private float _crossSize, _crossContent;
    private float UpdateCrossSize(float gap) => _crossSize = UpdateCrossContent() + (FlexLines.Count - 1) * gap;
    private float UpdateCrossContent() => _crossContent = FlexLines.Sum(line => line.CrossSize);

    private void UpdateCrossContentAlignment(float container, float gap)
    {
        UpdateCrossSize(gap);
        switch (CrossContentAlignment)
        {
            default:
            case CrossContentAlignment.Start:
            case CrossContentAlignment.Stretch:
            {
                _crossOffsetCache = 0f;
                _crossGapCache = gap;
                return;
            }
            case CrossContentAlignment.Center:
            {
                _crossGapCache = gap;
                _crossOffsetCache = (container - _crossSize) / 2f;
                return;
            }
            case CrossContentAlignment.End:
            {
                _crossGapCache = gap;
                _crossOffsetCache = container - _crossSize;
                return;
            }
            case CrossContentAlignment.SpaceEvenly:
            {
                _crossGapCache = (container - _crossContent) / (FlexLines.Count + 1);
                _crossOffsetCache = _crossGapCache;
                return;
            }
            case CrossContentAlignment.SpaceBetween:
            {
                _crossGapCache = FlexLines.Count > 1 ? (container - _crossContent) / (FlexLines.Count - 1) : 0f;
                _crossOffsetCache = 0f;
                return;
            }
        }
    }

    private float CalculateCrossOffset(float space, float itemCrossSize) => CrossAlignment switch
    {
        Center => (space - itemCrossSize) / 2f,
        End => space - itemCrossSize,
        Stretch or Start or { } => 0f,
    };
}