namespace SilkyUIFramework;

public enum LayoutType
{
    Flexbox,
    Custom,
}

public partial class UIElementGroup
{
    #region Properties & Fields LayoutType LayoutDirection Gap

    public LayoutType LayoutType
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public FlexDirection FlexDirection
    {
        get => _flexDirection;
        set
        {
            if (value == _flexDirection) return;
            _flexDirection = value;
            MarkLayoutDirty();
        }
    }

    private FlexDirection _flexDirection;

    public Size Gap
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public void SetGap(float gap) => Gap = gap;

    public void SetGap(float width, float height)
    {
        Gap = Gap.With(width, height);
    }

    #endregion

    public virtual void PrepareChildren()
    {
        ClassifyChildren();

        if (LayoutChildren.Count <= 0)
        {
            FlexLines.Clear();
            return;
        }

        float? spaceWidth = FitWidth ? null : InnerBounds.Width;
        float? spaceHeight = FitHeight ? null : InnerBounds.Height;

        foreach (var el in LayoutChildren)
        {
            el.Prepare(spaceWidth, spaceHeight);
        }

        if (LayoutType != LayoutType.Flexbox) return;

        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                if (FitWidth)
                {
                    FlexLines.Clear();
                    FlexLines.Add(FlexLine.SingleRow(LayoutChildren, Gap.Width));
                }
                else
                {
                    var maxMainAxisSize = (FlexWrap && !FitWidth) ? InnerBounds.Width : MaxInnerWidth;
                    LayoutChildren.FlexWrapRow(FlexLines, maxMainAxisSize, Gap.Width);
                }

                break;
            }
            case FlexDirection.Column:
            {
                if (FitHeight)
                {
                    FlexLines.Clear();
                    FlexLines.Add(FlexLine.SingleColumn(LayoutChildren, Gap.Height));
                }
                else
                {
                    var maxMainAxisSize = (FlexWrap && !FitHeight) ? InnerBounds.Height : MaxInnerHeight;
                    LayoutChildren.FlexWrapColumn(FlexLines, maxMainAxisSize, Gap.Height);
                }

                break;
            }
        }
    }

    /// <summary>
    /// 预处理
    /// </summary>
    public override void Prepare(float? width, float? height)
    {
        base.Prepare(width, height);

        PrepareChildren();

        if (LayoutType != LayoutType.Flexbox) return;

        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                FlexLines.MeasureSize(Gap.Width, out var contentWidth, out var contentHeight);
                if (FitWidth)
                    SetInnerBoundsWidth(MathHelper.Clamp(contentWidth, MinInnerWidth, MaxInnerWidth));
                if (FitHeight)
                    SetInnerBoundsHeight(MathHelper.Clamp(contentHeight, MinInnerHeight, MaxInnerHeight));

                break;
            }
            case FlexDirection.Column:
            {
                FlexLines.MeasureSize(Gap.Height, out var contentHeight, out var contentWidth);
                if (FitWidth)
                    SetInnerBoundsWidth(MathHelper.Clamp(contentWidth, MinInnerWidth, MaxInnerWidth));
                if (FitHeight)
                    SetInnerBoundsHeight(MathHelper.Clamp(contentHeight, MinInnerHeight, MaxInnerHeight));

                break;
            }
        }
    }

    public virtual void RecalculateChildrenHeight()
    {
        foreach (var el in LayoutChildren)
        {
            el.RecalculateHeight();
        }

        if (LayoutType != LayoutType.Flexbox) return;

        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                if (FitHeight)
                {
                    var content = FlexLines.CalculateCrossAxisContent() + FlexLines.FenceGap(Gap.Height);
                    SetInnerBoundsHeight(MathHelper.Clamp(content, MinInnerHeight, MaxInnerHeight));
                }

                foreach (var flexLine in FlexLines)
                {
                    flexLine.CrossSize = flexLine.Elements.Max(el => el.OuterBounds.Height);
                }
                break;
            }
            case FlexDirection.Column:
            {
                foreach (var flexLine in FlexLines)
                {
                    flexLine.MainSize = flexLine.Elements.Sum(el => el.OuterBounds.Height) + flexLine.GetFenceGap(Gap.Height);
                }
                break;
            }
        }
    }

    /// <summary>
    /// 重新设置子元素宽度
    /// </summary>
    public virtual void ResizeChildrenWidth()
    {
        if (LayoutChildren.Count <= 0) return;

        var innerSize = InnerBounds.Size;

        // 如果不适应宽度, 子元素也不适应宽度, 则重新设置子元素的宽度
        if (!FitWidth)
        {
            foreach (var el in LayoutChildren)
            {
                el.RefreshWidth(innerSize.Width);
            }
        }

        // flexbox 处理
        if (LayoutType is LayoutType.Flexbox)
        {
            switch (FlexDirection)
            {
                default:
                case FlexDirection.Row:
                {
                    // 宽度可能被父元素拉伸, 再次计算元素换行
                    if (FlexWrap)
                    {
                        var maxMainAxisSize = innerSize.Width;
                        LayoutChildren.FlexWrapRow(FlexLines, maxMainAxisSize, Gap.Width);
                    }
                    else
                    {
                        FlexLines.RefreshMainSizeByRowMode(Gap.Width);
                    }

                    // 拉伸或者压缩宽度
                    FlexLines.GrowOrShrinkByRowMode(innerSize, Gap.Width);
                    break;
                }
                case FlexDirection.Column:
                {
                    FlexLines.ResizeCrossAxis(CrossContentAlignment, InnerBounds.Width, Gap.Width);
                    if (CrossAlignment == CrossAlignment.Stretch)
                    {
                        foreach (FlexLine flexLine in FlexLines)
                        {
                            // 子元素使用适应宽度或者子元素的宽度小于交叉轴的空间
                            foreach (var el in flexLine.Elements.Where(
                                el => el.FitWidth || el.OuterBounds.Width < flexLine.CrossSpace))
                            {
                                el.SetExactWidth(flexLine.CrossSpace);
                            }
                        }
                    }
                    break;
                }
            }
        }

        foreach (var item in LayoutChildren.OfType<UIElementGroup>())
        {
            item.ResizeChildrenWidth();
        }
    }

    public override void RecalculateHeight()
    {
        base.RecalculateHeight();

        if (LayoutChildren.Count <= 0) return;

        RecalculateChildrenHeight();

        if (LayoutType != LayoutType.Flexbox) return;

        switch (FlexDirection)
        {
            default:
            case FlexDirection.Row:
            {
                if (FitHeight)
                {
                    var content = FlexLines.CalculateCrossAxisContent() + FlexLines.FenceGap(Gap.Height);
                    SetInnerBoundsHeight(MathHelper.Clamp(content, MinInnerHeight, MaxInnerHeight));
                }
                break;
            }
            case FlexDirection.Column:
            {
                if (FitHeight)
                {
                    var content = FlexLines.Max(line => line.MainSize);
                    SetInnerBoundsHeight(MathHelper.Clamp(content, MinInnerHeight, MaxInnerHeight));
                }
                break;
            }
        }
    }

    protected virtual void ResizeChildrenHeight()
    {
        if (LayoutChildren.Count <= 0) return;

        var innerSize = InnerBounds.Size;

        if (!FitHeight)
        {
            foreach (var el in LayoutChildren)
            {
                el.RefreshHeight(innerSize.Height);
            }
        }

        if (LayoutType is LayoutType.Flexbox)
        {
            switch (FlexDirection)
            {
                default:
                case FlexDirection.Row:
                {
                    FlexLines.ResizeCrossAxis(CrossContentAlignment, InnerBounds.Height, Gap.Height);
                    if (CrossAlignment == CrossAlignment.Stretch)
                    {
                        foreach (FlexLine flexLine in FlexLines)
                        {
                            foreach (var el in flexLine.Elements.Where(
                                el => el.FitHeight || el.OuterBounds.Height < flexLine.CrossSpace))
                            {
                                el.SetExactHeight(flexLine.CrossSpace);
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
                        LayoutChildren.FlexWrapColumn(FlexLines, maxMainAxisSize, Gap.Height);
                    }
                    else
                    {
                        FlexLines.RefreshMainSizeByColumnMode(Gap.Width);
                    }

                    FlexLines.GrowOrShrinkByColumnMode(innerSize, Gap.Height);
                    break;
                }
            }
        }

        foreach (var item in LayoutChildren.OfType<UIElementGroup>())
        {
            item.ResizeChildrenHeight();
        }
    }


    protected virtual void ApplyLayout()
    {
        if (LayoutType == LayoutType.Flexbox)
        {
            ApplyFlexboxLayout();
        }

        foreach (var child in LayoutChildren.OfType<UIElementGroup>())
        {
            child.ApplyLayout();
        }
    }
}