namespace SilkyUIFramework;

public enum LayoutType { Flexbox, Custom, }

public partial class UIElementGroup
{
    #region Properties & Fields LayoutType LayoutDirection Gap

    public LayoutType LayoutType
    {
        get => _layoutType;
        set
        {
            if (value == _layoutType) return;
            _layoutType = value;
            MarkDirty();
        }
    }

    private LayoutType _layoutType;

    public LayoutDirection LayoutDirection
    {
        get => _layoutDirection;
        set
        {
            if (value == _layoutDirection) return;
            _layoutDirection = value;
            MarkDirty();
        }
    }

    private LayoutDirection _layoutDirection;

    public Size Gap
    {
        get => _gap;
        set => SetGap(value.Width, value.Height);
    }

    private Size _gap;

    public void SetGap(float? width = null, float? height = null)
    {
        if (width.Equals(Gap.Width) && height.Equals(Gap.Height)) return;
        _gap = _gap.With(width, height);
        MarkDirty();
    }

    #endregion

    public override void Measure(Size container)
    {
        base.Measure(container);

        ClassifyElements();

        if (LayoutChildren.Count <= 0) return;

        var availableSpace = GetAvailableSpace();

        foreach (var child in LayoutChildren)
        {
            child.Measure(availableSpace);
        }

        if (LayoutType == LayoutType.Flexbox) HandleFlexbox();
    }

    protected virtual void HandleFlexbox()
    {
        WrapLayoutElements();

        if (!AutomaticWidth && !AutomaticHeight) return;

        float contentWidth, contentHeight;

        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                FlexLines.Measure(Gap.Width, out contentWidth, out contentHeight);
                break;
            }
            case LayoutDirection.Column:
            {
                FlexLines.Measure(Gap.Height, out contentHeight, out contentWidth);
                break;
            }
        }

        // 需要考虑约束
        if (AutomaticWidth) SetInnerBoundsWidth(contentWidth);
        if (AutomaticHeight) SetInnerBoundsHeight(contentHeight);
    }

    public virtual void Trim()
    {
        var space = GetAvailableSpace();
        var innerSize = InnerBounds.Size;

        if (LayoutType is LayoutType.Flexbox)
        {
            switch (LayoutDirection)
            {
                default:
                case LayoutDirection.Row:
                {
                    FlexLines.AdjustFlexLinesCrossAxisSize(CrossContentAlignment, InnerBounds.Height, Gap.Height);
                    FlexLines.ProcessFlexLinesByRow(CrossAlignment, space, innerSize, Gap.Width);
                    break;
                }
                case LayoutDirection.Column:
                {
                    FlexLines.AdjustFlexLinesCrossAxisSize(CrossContentAlignment, InnerBounds.Width, Gap.Width);
                    FlexLines.ProcessFlexLinesByColumn(CrossAlignment, space, innerSize, Gap.Height);
                    break;
                }
            }
        }
        else
        {
            foreach (var child in LayoutChildren)
            {
                child.SpecifyWidth(space.Width);
                child.SpecifyHeight(space.Height);
            }
        }

        foreach (var item in LayoutChildren.OfType<UIElementGroup>())
        {
            item.Trim();
        }
    }


    private void ApplyLayout()
    {
        if (_layoutType == LayoutType.Flexbox)
        {
            ApplyFlexboxLayout();
        }

        foreach (var child in LayoutChildren.OfType<UIElementGroup>())
        {
            child.ApplyLayout();
        }
    }
}