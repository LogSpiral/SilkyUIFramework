namespace SilkyUIFramework;

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

    /// <summary>
    /// 预处理
    /// </summary>
    public override void Prepare(float? width, float? height)
    {
        base.Prepare(width, height);

        PrepareChildren();

        if (LayoutElements.Count <= 0) return;
        if (LayoutType == LayoutType.Flexbox) FlexboxModel.OnPrepare();
    }

    public virtual void PrepareChildren()
    {
        ClassifyChildren();
        if (LayoutElements.Count <= 0) return;

        float? spaceWidth = FitWidth ? null : InnerBounds.Width;
        float? spaceHeight = FitHeight ? null : InnerBounds.Height;

        foreach (var el in LayoutElements)
        {
            el.Prepare(spaceWidth, spaceHeight);
        }

        if (LayoutType == LayoutType.Flexbox) FlexboxModel.OnPrepareChildren();
    }

    /// <summary> 重新设置子元素宽度 </summary>
    public virtual void ResizeChildrenWidth()
    {
        if (LayoutElements.Count <= 0) return;
        var innerSize = InnerBounds.Size;

        // 如果不适应宽度, 子元素也不适应宽度, 则重新设置子元素的宽度
        if (!FitWidth)
        {
            foreach (var el in LayoutElements)
            {
                el.RefreshWidth(innerSize.Width);
            }
        }

        // Flexbox 处理
        if (LayoutType is LayoutType.Flexbox) FlexboxModel.OnResizeChildrenWidth();

        foreach (var item in LayoutElements.OfType<UIElementGroup>())
        {
            item.ResizeChildrenWidth();
        }
    }

    public override void RecalculateHeight()
    {
        if (LayoutElements.Count <= 0) return;
        base.RecalculateHeight();

        RecalculateChildrenHeight();

        if (LayoutType == LayoutType.Flexbox) FlexboxModel.OnRecalculateHeight();
    }

    protected virtual void RecalculateChildrenHeight()
    {
        if (LayoutElements.Count <= 0) return;
        foreach (var el in LayoutElements)
        {
            el.RecalculateHeight();
        }

        if (LayoutType == LayoutType.Flexbox) FlexboxModel.OnRecalculateChildrenHeight();
    }

    protected virtual void ResizeChildrenHeight()
    {
        if (LayoutElements.Count <= 0) return;
        var innerSize = InnerBounds.Size;

        if (!FitHeight)
        {
            foreach (var el in LayoutElements)
            {
                el.RefreshHeight(innerSize.Height);
            }
        }

        if (LayoutType == LayoutType.Flexbox) FlexboxModel.OnResizeChildrenHeight();

        foreach (var item in LayoutElements.OfType<UIElementGroup>())
        {
            item.ResizeChildrenHeight();
        }
    }


    protected virtual void UpdateChildrenLayoutOffset()
    {
        if (LayoutElements.Count <= 0) return;

        switch (LayoutType)
        {
            case LayoutType.Flexbox:
            {
                FlexboxModel.OnUpdateChildrenLayoutOffset();
                break;
            }
            case LayoutType.Custom:
            default: goto case LayoutType.Flexbox;
        }

        foreach (var child in LayoutElements.OfType<UIElementGroup>())
        {
            child.UpdateChildrenLayoutOffset();
        }
    }
}