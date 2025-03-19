using SilkyUIFramework.MyElement;

namespace SilkyUIFramework;

public partial class UIElementGroup
{
    #region FlexWrap MainAlignment CrossAlignment CrossContentAlignment

    public bool MainAxisFixed => LayoutDirection == LayoutDirection.Column ? !AutomaticHeight : !AutomaticWidth;

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

    #endregion

    protected readonly List<FlexLine> FlexLines = [];

    protected void WrapLayoutElements()
    {
        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                var maxMainAxisSize = (FlexWrap && MainAxisFixed) ? InnerBounds.Width : MaxInnerWidth;
                LayoutChildren.WrapRow(FlexLines, maxMainAxisSize, Gap.Width);

                return;
            }
            case LayoutDirection.Column:
            {
                var maxMainAxisSize = (FlexWrap && MainAxisFixed) ? InnerBounds.Height : MaxInnerHeight;
                LayoutChildren.WrapColumn(FlexLines, maxMainAxisSize, Gap.Height);

                return;
            }
        }
    }

    protected void ApplyFlexboxLayout()
    {
        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                FlexLines.CalculateCrossContentAlignment(CrossContentAlignment,
                    InnerBounds.Height, Gap.Height, out var top, out var crossGap);

                foreach (var flexLine in FlexLines)
                {
                    flexLine.CalculateMainAlignment(InnerBounds.Width, Gap.Width, MainAlignment, out var left, out var mainGap);

                    foreach (var element in flexLine.Elements)
                    {
                        float crossOffset = CrossAlignment.CalculateCrossOffset(flexLine.AvailableCrossSpace, element.OuterBounds.Height);
                        element.SetLayoutOffset(left, top + crossOffset);
                        left += element.OuterBounds.Width + mainGap;
                    }

                    top += flexLine.AvailableCrossSpace + crossGap;
                }
                break;
            }
            case LayoutDirection.Column:
            {
                FlexLines.CalculateCrossContentAlignment(CrossContentAlignment,
                    InnerBounds.Width, Gap.Width, out var left, out var crossGap);

                foreach (var flexLine in FlexLines)
                {
                    flexLine.CalculateMainAlignment(InnerBounds.Height, Gap.Height, MainAlignment, out var top, out var mainGap);

                    foreach (var element in flexLine.Elements)
                    {
                        float itemCrossOffset = CrossAlignment.CalculateCrossOffset(flexLine.AvailableCrossSpace, element.OuterBounds.Width);
                        element.SetLayoutOffset(left + itemCrossOffset, top);
                        top += element.OuterBounds.Height + mainGap;
                    }

                    left += flexLine.AvailableCrossSpace + crossGap;
                }
                break;
            }
        }
    }

}