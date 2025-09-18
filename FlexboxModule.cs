﻿using static SilkyUIFramework.CrossAlignment;

namespace SilkyUIFramework;

public partial class FlexboxModule
{
    private readonly List<FlexLine> _flexLines = [];

    #region Cache Status

    private bool _flexWrap;
    private FlexDirection _flexDirection;
    private MainAlignment _mainAlignment;
    private CrossAlignment _crossAlignment;
    private CrossContentAlignment _crossContentAlignment;

    public sealed override void UpdateCacheStatus()
    {
        base.UpdateCacheStatus();
        _flexDirection = Parent.FlexDirection;
        _flexWrap = Parent.FlexWrap;
        _mainAlignment = Parent.MainAlignment;
        _crossAlignment = Parent.CrossAlignment;
        _crossContentAlignment = Parent.CrossContentAlignment;
    }

    #endregion

    private float MaxMainSize()
    {
        var mainSize = 0f;

        for (int i = 0; i < _flexLines.Count; i++)
        {
            mainSize = Math.Max(_flexLines[i].MainSize, mainSize);
        }

        return mainSize;
    }

    private void SingleRow()
    {
        _flexLines.Clear();
        _flexLines.Add(FlexLine.CreateSingleRow(Parent.LayoutChildren, Gap.Width));
    }

    private void SingleColumn()
    {
        _flexLines.Clear();
        _flexLines.Add(FlexLine.CreateSingleColumn(Parent.LayoutChildren, Gap.Height));
    }

    private void WrapRow()
    {
        var container = Parent.InnerBounds.Width;
        var gap = Gap.Width;
        var elements = Parent.LayoutChildren;
        _flexLines.Clear();

        var line = FlexLine.CreateRow(elements[0]);
        _flexLines.Add(line);

        for (var i = 1; i < elements.Count; i++)
        {
            var element = elements[i];

            var mainAxisSize = element.OuterBounds.Width;
            var crossAxisSize = element.OuterBounds.Height;

            if (line.MainSize + mainAxisSize + gap <= container)
            {
                line.Elements.Add(element);
                line.MainSize += mainAxisSize + gap;
                line.CrossSize = Math.Max(line.CrossSize, crossAxisSize);
                continue;
            }

            line = FlexLine.CreateRow(element);
            _flexLines.Add(line);
        }
    }

    private void WrapColumn()
    {
        var maxMainAxisSize = Parent.InnerBounds.Height;
        var gap = Gap.Height;
        var elements = Parent.LayoutChildren;
        _flexLines.Clear();

        var line = FlexLine.CreateColumn(elements[0]);
        _flexLines.Add(line);

        for (var i = 1; i < elements.Count; i++)
        {
            var element = elements[i];

            var mainAxisSize = element.OuterBounds.Height;
            var crossAxisSize = element.OuterBounds.Width;

            if (line.MainSize + mainAxisSize + gap <= maxMainAxisSize)
            {
                line.Elements.Add(element);
                line.MainSize += mainAxisSize + gap;
                line.CrossSize = Math.Max(line.CrossSize, crossAxisSize);
                continue;
            }

            line = FlexLine.CreateColumn(element);
            _flexLines.Add(line);
        }
    }

    private void MeasureSize(float gap, out float mainSize, out float crossSize)
    {
        mainSize = 0f;
        crossSize = (_flexLines.Count - 1) * gap;

        for (int i = 0; i < _flexLines.Count; i++)
        {
            var line = _flexLines[i];
            mainSize = Math.Max(mainSize, line.MainSize);
            crossSize += line.CrossSize;
        }
    }

    private void RowGrowOrShrink()
    {
        var width = Parent.InnerBounds.Width;
        var gap = Gap.Width;

        for (int i = 0; i < _flexLines.Count; i++)
        {
            var line = _flexLines[i];
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

                    for (int j = 0; j < growElements.Length; j++)
                    {
                        var (Element, AvailableGrowth) = growElements[j];
                        var share = remaining / totalGrow;
                        var alloc = Math.Min(AvailableGrowth, share * Element.FlexGrow);

                        SetOuterWidth(Element, Element.OuterBounds.Width + alloc);

                        remaining -= alloc;
                        totalGrow -= Element.FlexGrow;
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

                    for (int j = 0; j < shrinkElements.Length; j++)
                    {
                        var (Element, AvailableShrink) = shrinkElements[j];
                        var share = remaining / totalShrink;
                        var alloc = Math.Max(AvailableShrink, share * Element.FlexShrink);

                        SetOuterWidth(Element, Element.OuterBounds.Width + alloc);

                        remaining -= alloc;
                        totalShrink -= Element.FlexShrink;
                    }

                    break;
                }
            }

            line.UpdateMainSizeByRow(gap);
        }
    }

    private void ColumnGrowOrShrink()
    {
        var height = Parent.InnerBounds.Height;
        for (int i = 0; i < _flexLines.Count; i++)
        {
            var line = _flexLines[i];
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

                    for (var j = 0; j < sortedElements.Length; j++)
                    {
                        var (Element, AvailableGrowth) = sortedElements[j];
                        if (totalGrow <= 0) break;

                        var share = remaining / totalGrow;
                        var alloc = Math.Min(AvailableGrowth, share * Element.FlexGrow);

                        SetOuterHeight(Element, Element.OuterBounds.Height + alloc);

                        remaining -= alloc;
                        totalGrow -= Element.FlexGrow;
                    }

                    line.UpdateMainSizeByColumn(Gap.Height);
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

                    for (var j = 0; j < sortedElements.Length; j++)
                    {
                        var (Element, AvailableShrink) = sortedElements[j];
                        if (totalShrink <= 0 || remaining >= 0) break;

                        var share = remaining / totalShrink;
                        var alloc = Math.Max(AvailableShrink, share * Element.FlexShrink);

                        SetOuterHeight(Element, Element.OuterBounds.Height + alloc);

                        remaining -= alloc;
                        totalShrink -= Element.FlexShrink;
                    }

                    line.UpdateMainSizeByColumn(Gap.Height);
                    break;
                }
            }
        }
    }

    private float _crossSize, _crossContent;
    private float UpdateCrossSize(float gap)
    {
        _crossContent = _flexLines.Sum(line => line.CrossSize);
        return _crossSize = _crossContent + (_flexLines.Count - 1) * gap;
    }

    private void UpdateCrossContentAlignment(float container, float gap)
    {
        UpdateCrossSize(gap);
        switch (_crossContentAlignment)
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
                _crossGapCache = (container - _crossContent) / (_flexLines.Count + 1);
                _crossOffsetCache = _crossGapCache;
                return;
            }
            case CrossContentAlignment.SpaceBetween:
            {
                _crossGapCache = _flexLines.Count > 1 ? (container - _crossContent) / (_flexLines.Count - 1) : 0f;
                _crossOffsetCache = 0f;
                return;
            }
        }
    }

    private float CalculateCrossOffset(float availableSize, float itemCrossSize) => _crossAlignment switch
    {
        Center => (availableSize - itemCrossSize) / 2f,
        End => availableSize - itemCrossSize,
        Stretch or Start or { } => 0f,
    };
}