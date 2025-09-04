namespace SilkyUIFramework;

public partial class UIElementGroup
{
    #region Properties & Fields LayoutType LayoutDirection Gap

    /// <summary>
    /// 目前仅有 Flexbox 可以使用，请不要自定义布局。
    /// </summary>
    public LayoutType LayoutType
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            MarkLayoutDirty();
        }
    } = LayoutType.Flexbox;

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
    public void SetGap(float width, float height) => Gap = Gap.With(width, height);

    private readonly FlexboxModule FlexboxModule;
    public readonly GridModule GridModule;

    public LayoutModule LayoutModule
    {
        get
        {
            return LayoutType switch
            {
                LayoutType.Grid => GridModule,
                LayoutType.Custom => field,
                { } => FlexboxModule,
            };
        }
        set
        {
            if (value.Parent != this) return;
            if (field.GetType() == value.GetType()) return;
            field = value;
            if (LayoutType == LayoutType.Custom) MarkLayoutDirty();
        }
    }

    #endregion

    public override void Prepare(float? width, float? height)
    {
        base.Prepare(width, height);

        PrepareChildren();

        if (LayoutElements.Count <= 0) return;
        LayoutModule?.PostPrepare();
    }

    public virtual void PrepareChildren()
    {
        ClassifyChildren();
        if (LayoutElements.Count <= 0) return;

        LayoutModule?.UpdateCacheStatus();

        float? availableWidth = FitWidth ? null : InnerBounds.Width;
        float? availableHeight = FitHeight ? null : InnerBounds.Height;
        for (int i = 0; i < LayoutElements.Count; i++)
        {
            LayoutModule.ModifyAvailableSize(LayoutElements[i], i, ref availableWidth, ref availableHeight);
            LayoutElements[i].Prepare(availableWidth, availableHeight);
        }

        LayoutModule?.PostPrepareChildren();
    }

    /// <summary> 重新设置子元素宽度 </summary>
    public virtual void ResizeChildrenWidth()
    {
        if (LayoutElements.Count <= 0) return;

        if (!FitWidth)
        {
            for (int i = 0; i < LayoutElements.Count; i++)
            {
                LayoutElements[i].RefreshWidth(InnerBounds.Width);
            }
        }

        LayoutModule?.PostResizeChildrenWidth();

        foreach (var item in LayoutElements.OfType<UIElementGroup>())
        {
            item.ResizeChildrenWidth();
        }
    }

    public override void RecalculateWidth()
    {
        base.RecalculateWidth();
        RecalculateChildrenWidth();

        LayoutModule?.PostRecalculateWidth();
    }

    protected virtual void RecalculateChildrenWidth()
    {
        if (LayoutElements.Count <= 0) return;
        for (int i = 0; i < LayoutElements.Count; i++)
        {
            LayoutElements[i].RecalculateWidth();
        }

        LayoutModule?.PostRecalculateChildrenWidth();
    }

    public override void RecalculateHeight()
    {
        base.RecalculateHeight();
        RecalculateChildrenHeight();

        LayoutModule?.PostRecalculateHeight();
    }

    protected virtual void RecalculateChildrenHeight()
    {
        if (LayoutElements.Count <= 0) return;
        for (int i = 0; i < LayoutElements.Count; i++)
        {
            LayoutElements[i].RecalculateHeight();
        }

        LayoutModule?.PostRecalculateChildrenHeight();
    }

    protected virtual void ResizeChildrenHeight()
    {
        if (LayoutElements.Count <= 0) return;
        var innerSize = InnerBounds.Size;

        if (!FitHeight)
        {
            for (int i = 0; i < LayoutElements.Count; i++)
            {
                LayoutElements[i].RefreshHeight(innerSize.Height);
            }
        }

        LayoutModule?.PostResizeChildrenHeight();

        foreach (var item in LayoutElements.OfType<UIElementGroup>())
        {
            item.ResizeChildrenHeight();
        }
    }


    protected virtual void UpdateChildrenLayoutOffset()
    {
        if (LayoutElements.Count <= 0) return;

        LayoutModule.ModifyLayoutOffset();

        foreach (var child in LayoutElements.OfType<UIElementGroup>())
        {
            child.UpdateChildrenLayoutOffset();
        }
    }
}