namespace SilkyUIFramework;

public partial class UIElementGroup
{
    #region FlexWrap MainAlignment CrossAlignment CrossContentAlignment

    public bool FlexWrap
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public MainAlignment MainAlignment
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public CrossAlignment CrossAlignment
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public CrossContentAlignment CrossContentAlignment
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    } = CrossContentAlignment.Stretch;

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