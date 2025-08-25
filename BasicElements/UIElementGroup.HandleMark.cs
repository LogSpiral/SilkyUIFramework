namespace SilkyUIFramework;

public partial class UIElementGroup
{
    protected internal virtual void NotifyParentChildDirty()
    {
        if (LayoutIsDirty) return;

        if (FitWidth || FitHeight)
        {
            LayoutIsDirty = true;
            PositionIsDirty = true;
            Parent?.NotifyParentChildDirty();
            return;
        }

        LayoutIsDirty = true;
        PositionIsDirty = true;
    }

    public bool ElementsOrderIsDirty { get; set; } = true;

    public List<UIView> ElementsInOrder { get; } = [];

    public void RefreshElementsOrder()
    {
        if (ElementsOrderIsDirty)
        {
            ElementsInOrder.Clear();
            ElementsInOrder.AddRange(ElementsCache.OrderBy(el => el.ZIndex));
            ElementsOrderIsDirty = false;
        }

        foreach (var item in ElementsInOrder.OfType<UIElementGroup>())
        {
            item.RefreshElementsOrder();
        }
    }

    public override void RefreshLayout()
    {
        if (LayoutIsDirty)
        {
            if (Positioning.IsFree()) RefreshLayoutFromFree();
            else RefreshLayoutFromFlow();

            CleanupDirtyMark();
        }

        foreach (var child in ElementsCache)
        {
            child.RefreshLayout();
        }
    }

    protected void RefreshLayoutFromFree()
    {
        var container = GetParentInnerSpace();
        Prepare(container.Width, container.Height);
        ResizeChildrenWidth();
        RecalculateHeight();
        ResizeChildrenHeight();
        ApplyLayout();
    }

    protected void RefreshLayoutFromFlow()
    {
        PrepareChildren();
        ResizeChildrenWidth();
        RecalculateChildrenHeight();
        ResizeChildrenHeight();
        ApplyLayout();
    }

    public override void UpdatePosition()
    {
        base.UpdatePosition();

        foreach (var child in ElementsCache)
        {
            child.UpdatePosition();
        }
    }

    public override void RecalculatePosition()
    {
        base.RecalculatePosition();

        foreach (var child in ElementsCache.Where(el => el.Positioning is not Positioning.Fixed))
        {
            child.RecalculatePosition();
        }
    }
}