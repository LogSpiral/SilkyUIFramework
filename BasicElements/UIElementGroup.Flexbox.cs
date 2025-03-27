namespace SilkyUIFramework;

public partial class UIElementGroup
{
    #region FlexWrap MainAlignment CrossAlignment CrossContentAlignment

    public bool FlexWrap
    {
        get => _flexWrap;
        set
        {
            if (_flexWrap == value) return;
            _flexWrap = value;
            MarkLayoutDirty();
        }
    }

    private bool _flexWrap;

    public MainAlignment MainAlignment
    {
        get => _mainAlignment;
        set
        {
            if (_mainAlignment == value) return;
            _mainAlignment = value;
            MarkLayoutDirty();
        }
    }

    private MainAlignment _mainAlignment;

    public CrossAlignment CrossAlignment
    {
        get => _crossAlignment;
        set
        {
            if (_crossAlignment == value) return;
            _crossAlignment = value;
            MarkLayoutDirty();
        }
    }

    private CrossAlignment _crossAlignment;

    public CrossContentAlignment CrossContentAlignment
    {
        get => _crossContentAlignment;
        set
        {
            if (_crossContentAlignment == value) return;
            _crossContentAlignment = value;
            MarkLayoutDirty();
        }
    }

    private CrossContentAlignment _crossContentAlignment;

    #endregion

    protected readonly List<FlexLine> FlexLines = [];

    protected void ApplyFlexboxLayout()
    {
        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                FlexLines.CalculateCrossContentAlignment(CrossContentAlignment,
                    InnerBounds.Height, Gap.Height, out var top, out var crossGap);

                foreach (var flexLine in FlexLines)
                {
                    flexLine.CalculateMainAlignment(InnerBounds.Width, Gap.Width, MainAlignment, out var left, out var mainGap);

                    foreach (var el in flexLine.Elements)
                    {
                        float crossOffset = CrossAlignment.CalculateCrossOffset(flexLine.CrossSpace, el.OuterBounds.Height);
                        el.SetLayoutOffset(left, top + crossOffset);
                        left += el.OuterBounds.Width + mainGap;
                    }

                    top += flexLine.CrossSpace + crossGap;
                }
                break;
            }
            case FlexDirection.Column:
            {
                FlexLines.CalculateCrossContentAlignment(CrossContentAlignment,
                    InnerBounds.Width, Gap.Width, out var left, out var crossGap);

                foreach (var flexLine in FlexLines)
                {
                    flexLine.CalculateMainAlignment(InnerBounds.Height, Gap.Height, MainAlignment, out var top, out var mainGap);

                    foreach (var el in flexLine.Elements)
                    {
                        float itemCrossOffset = CrossAlignment.CalculateCrossOffset(flexLine.CrossSpace, el.OuterBounds.Width);
                        el.SetLayoutOffset(left + itemCrossOffset, top);
                        top += el.OuterBounds.Height + mainGap;
                    }

                    left += flexLine.CrossSpace + crossGap;
                }
                break;
            }
        }
    }

}