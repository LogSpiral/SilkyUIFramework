using static SilkyUIFramework.CrossAlignment;

namespace SilkyUIFramework;

public partial class FlexboxModel(UIElementGroup parent) : LayoutModel(parent)
{
    private float _crossOffsetCache, _crossGapCache;

    public override sealed void OnUpdateChildrenLayoutOffset()
    {
        var crossStart = _crossOffsetCache;
        var crossGap = _crossGapCache;

        switch (FlexDirection)
        {
            case FlexDirection.Row:
            {
                foreach (var line in FlexLines)
                {
                    var left = line.MainOffset;

                    foreach (var el in line)
                    {
                        var crossOffset = CalculateCrossOffset(line.CrossSize, el.OuterBounds.Height);
                        el.SetLayoutOffset(left, crossStart + crossOffset);
                        left += el.OuterBounds.Width + line.MainGap;
                    }

                    crossStart += line.CrossSize + crossGap;
                }

                break;
            }
            case FlexDirection.Column:
            {
                foreach (var line in FlexLines)
                {
                    var top = line.MainOffset;

                    foreach (var el in line)
                    {
                        var itemCrossOffset = CalculateCrossOffset(line.CrossSize, el.OuterBounds.Width);
                        el.SetLayoutOffset(crossStart + itemCrossOffset, top);
                        top += el.OuterBounds.Height + line.MainGap;
                    }

                    crossStart += line.CrossSize + crossGap;
                }

                break;
            }
            default: goto case FlexDirection.Row;
        }
    }

    public override sealed void OnPrepare()
    {
        UpdateCacheStatus();
        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                MeasureSize(Parent.Gap.Width, out var mainSize, out var crossSize);
                if (FitWidth) Parent.SetExactInnerWidth(mainSize);
                if (FitHeight) Parent.SetExactInnerHeight(crossSize);
                break;
            }
            case FlexDirection.Column:
            {
                MeasureSize(Parent.Gap.Height, out var mainSize, out var crossSize);
                if (FitWidth) Parent.SetExactInnerWidth(crossSize);
                if (FitHeight) Parent.SetExactInnerHeight(mainSize);
                break;
            }
        }
    }

    public override sealed void OnPrepareChildren()
    {
        switch (FlexDirection)
        {
            case FlexDirection.Row:
            {
                if (FlexWrap && !FitWidth) WrapRow();
                else SingleRow();
                break;
            }
            case FlexDirection.Column:
            {
                if (FlexWrap && !FitHeight) WrapColumn();
                else SingleColumn();
                break;
            }
            default: goto case FlexDirection.Row;
        }
    }

    public override sealed void OnResizeChildrenWidth()
    {
        switch (FlexDirection)
        {
            case FlexDirection.Row:
            {
                // 宽度可能被父元素拉伸, 再次计算元素换行
                if (FlexWrap) WrapRow();
                else UpdateMainSizeByRow();

                // 拉伸或者压缩宽度
                RowModeGrowOrShrink();
                break;
            }
            case FlexDirection.Column:
            {
                if (CrossContentAlignment == CrossContentAlignment.Stretch)
                {
                    var remaining = Parent.InnerBounds.Width - UpdateCrossSize(Parent.Gap.Width);
                    if (remaining > 0)
                    {
                        var each = remaining / FlexLines.Count;
                        FlexLines.ForEach(line => line.CrossSize += each);
                    }
                }

                if (CrossAlignment != Stretch) break;
                foreach (var line in FlexLines)
                {
                    foreach (var el in line.Where(el => el.FitWidth || el.OuterBounds.Width < line.CrossSize))
                    {
                        el.SetExactOuterWidth(line.CrossSize);
                    }
                }

                break;
            }
            default: goto case FlexDirection.Row;
        }
    }

    public override sealed void OnRecalculateHeight()
    {
        if (!FitHeight) return;
        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
                Parent.SetExactInnerHeight(UpdateCrossSize(Parent.Gap.Height));
                break;
            case FlexDirection.Column:
                Parent.SetExactInnerHeight(MaxMainSize());
                break;
        }
    }

    public override sealed void OnRecalculateChildrenHeight()
    {
        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
                foreach (var line in FlexLines) line.CrossSize = line.Max(el => el.OuterBounds.Height);
                break;
            case FlexDirection.Column:
                foreach (var line in FlexLines) line.UpdateMainSizeByColumn(Parent.Gap.Height);
                break;
        }
    }

    public override sealed void OnResizeChildrenHeight()
    {
        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                if (CrossContentAlignment == CrossContentAlignment.Stretch)
                {
                    var remaining = Parent.InnerBounds.Height - UpdateCrossSize(Parent.Gap.Height);
                    if (remaining > 0)
                    {
                        var each = remaining / FlexLines.Count;
                        FlexLines.ForEach(line => line.CrossSize += each);
                    }
                }

                if (CrossAlignment == Stretch)
                {
                    foreach (var line in FlexLines)
                    {
                        foreach (var el in line.Where(el => el.FitHeight || el.OuterBounds.Height < line.CrossSize))
                        {
                            el.SetExactOuterHeight(line.CrossSize);
                        }
                    }
                }

                var innerBounds = Parent.InnerBounds;
                var gap = Parent.Gap;

                FlexLines.ForEach(line => line.UpdateMainAlignment(MainAlignment, innerBounds.Width, gap.Width));
                UpdateCrossContentAlignment(innerBounds.Height, gap.Height);
                break;
            }
            case FlexDirection.Column:
            {
                if (FlexWrap) WrapColumn();
                else UpdateMainSizeByColumn();
                ColumnModeGrowOrShrink();

                var innerBounds = Parent.InnerBounds;
                var gap = Parent.Gap;

                FlexLines.ForEach(line => line.UpdateMainAlignment(MainAlignment, innerBounds.Height, gap.Height));
                UpdateCrossContentAlignment(innerBounds.Width, gap.Width);
                break;
            }
        }
    }
}