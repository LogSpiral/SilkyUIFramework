using static SilkyUIFramework.CrossAlignment;

namespace SilkyUIFramework;

public partial class FlexboxModule(UIElementGroup parent) : LayoutModule(parent)
{
    private float _crossOffsetCache, _crossGapCache;

    public override sealed void PostPrepare()
    {
        switch (_flexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                MeasureSize(Gap.Width, out var mainSize, out var crossSize);
                if (FitWidth) SetInnerWidth(Parent, mainSize);
                if (FitHeight) SetInnerHeight(Parent, crossSize);
                break;
            }
            case FlexDirection.Column:
            {
                MeasureSize(Gap.Height, out var mainSize, out var crossSize);
                if (FitWidth) SetInnerWidth(Parent, crossSize);
                if (FitHeight) SetInnerHeight(Parent, mainSize);
                break;
            }
        }
    }

    public override sealed void PostPrepareChildren()
    {
        switch (_flexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                if (_flexWrap && !FitWidth) WrapRow();
                else SingleRow();
                break;
            }
            case FlexDirection.Column:
            {
                if (_flexWrap && !FitHeight) WrapColumn();
                else SingleColumn();
                break;
            }
        }
    }

    public override sealed void PostResizeChildrenWidth()
    {
        switch (_flexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                // 宽度可能被父元素拉伸, 再次计算元素换行
                if (_flexWrap) WrapRow();
                else
                {
                    for (var i = 0; i < _flexLines.Count; i++)
                    {
                        _flexLines[i].UpdateMainSizeByRow(Gap.Width);
                    }
                }
                RowGrowOrShrink();
                break;
            }
            case FlexDirection.Column:
            {
                if (_crossContentAlignment == CrossContentAlignment.Stretch)
                {
                    var remaining = Parent.InnerBounds.Width - UpdateCrossSize(Gap.Width);
                    if (remaining > 0)
                    {
                        var share = remaining / _flexLines.Count;
                        for (int i = 0; i < _flexLines.Count; i++) _flexLines[i].CrossSize += share;
                    }
                }

                if (_crossAlignment != Stretch) break;
                for (int i = 0; i < _flexLines.Count; i++)
                {
                    var line = _flexLines[i];
                    for (int j = 0; j < line.Elements.Count; j++)
                    {
                        var el = line.Elements[j];
                        if (!el.FitWidth && el.OuterBounds.Width >= line.CrossSize) continue;
                        SetOuterWidth(el, line.CrossSize);
                    }
                }

                break;
            }
        }
    }

    public override sealed void PostRecalculateHeight()
    {
        if (!FitHeight) return;
        switch (_flexDirection)
        {
            default:
            case FlexDirection.Row:
                SetInnerHeight(Parent, UpdateCrossSize(Gap.Height));
                break;
            case FlexDirection.Column:
                SetInnerHeight(Parent, MaxMainSize());
                break;
        }
    }

    public override sealed void PostRecalculateChildrenHeight()
    {
        switch (_flexDirection)
        {
            default:
            case FlexDirection.Row:
                for (int i = 0; i < _flexLines.Count; i++)
                {
                    var line = _flexLines[i];
                    line.CrossSize = line.MaxOuterHeight();
                }

                break;
            case FlexDirection.Column:
                for (int i = 0; i < _flexLines.Count; i++)
                {
                    _flexLines[i].UpdateMainSizeByColumn(Gap.Height);
                }

                break;
        }
    }

    public override sealed void PostResizeChildrenHeight()
    {
        switch (_flexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                if (_crossContentAlignment == CrossContentAlignment.Stretch)
                {
                    var remaining = Parent.InnerBounds.Height - UpdateCrossSize(Gap.Height);
                    if (remaining > 0)
                    {
                        var share = remaining / _flexLines.Count;
                        for (int i = 0; i < _flexLines.Count; i++)
                        {
                            _flexLines[i].CrossSize += share;
                        }
                    }
                }

                if (_crossAlignment == Stretch)
                {
                    for (int i = 0; i < _flexLines.Count; i++)
                    {
                        var line = _flexLines[i];
                        for (int j = 0; j < line.Elements.Count; j++)
                        {
                            var el = line.Elements[j];
                            if (!el.FitHeight && el.OuterBounds.Height >= line.CrossSize) continue;
                            SetOuterHeight(el, line.CrossSize);
                        }
                    }
                }

                var innerBounds = Parent.InnerBounds;
                var gap = Gap;

                for (int i = 0; i < _flexLines.Count; i++)
                    _flexLines[i].UpdateMainAlignment(_mainAlignment, innerBounds.Width, gap.Width);

                UpdateCrossContentAlignment(innerBounds.Height, gap.Height);
                break;
            }
            case FlexDirection.Column:
            {
                if (_flexWrap) WrapColumn();
                else
                {
                    for (int i = 0; i < _flexLines.Count; i++)
                    {
                        _flexLines[i].UpdateMainSizeByColumn(Gap.Height);
                    }
                }
                ColumnGrowOrShrink();

                var innerBounds = Parent.InnerBounds;
                var gap = Gap;

                for (int i = 0; i < _flexLines.Count; i++)
                    _flexLines[i].UpdateMainAlignment(_mainAlignment, innerBounds.Height, gap.Height);

                UpdateCrossContentAlignment(innerBounds.Width, gap.Width);
                break;
            }
        }
    }

    public override sealed void ModifyLayoutOffset()
    {
        var crossStart = _crossOffsetCache;
        var crossGap = _crossGapCache;

        switch (_flexDirection)
        {
            case FlexDirection.Row:
            {
                for (int i = 0; i < _flexLines.Count; i++)
                {
                    var line = _flexLines[i];
                    var left = line.MainOffset;

                    for (int j = 0; j < line.Elements.Count; j++)
                    {
                        var el = line.Elements[j];
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
                for (int i = 0; i < _flexLines.Count; i++)
                {
                    var line = _flexLines[i];
                    var top = line.MainOffset;

                    for (int j = 0; j < line.Elements.Count; j++)
                    {
                        var el = line.Elements[j];
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
}