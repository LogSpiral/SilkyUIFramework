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

    public void UpdateElementsOrder()
    {
        if (ElementsOrderIsDirty)
        {
            ElementsInOrder.Clear();
            ElementsInOrder.AddRange(ElementsCache.OrderBy(el => el.ZIndex));
            ElementsOrderIsDirty = false;
        }

        foreach (var item in ElementsInOrder.OfType<UIElementGroup>())
        {
            item.UpdateElementsOrder();
        }
    }

    public override void UpdateLayout()
    {
        if (LayoutIsDirty)
        {
            if (Positioning.IsFree)
            {
                LayoutFromFree();
            }
            else
            {
                LayoutFromFlow();
            }

            CleanupDirtyMark();
        }

        foreach (var child in ElementsCache)
        {
            child.UpdateLayout();
        }
    }

    protected void LayoutFromFree()
    {
        var container = GetParentInnerSpace();
        Prepare(container.Width, container.Height);
        ResizeChildrenWidth();
        RecalculateHeight();
        ResizeChildrenHeight();
        ApplyLayout();

        foreach (var item in FreeChildren)
        {
            if (item.Positioning != Positioning.Absolute) continue;
            if (item.LayoutIsDirty) continue;
            if (item.Width.Percent == 0 && item.Height.Percent == 0 &&
                item.Left.Percent == 0 && item.Top.Percent == 0 &&
                item.Left.Alignment == 0 && item.Top.Alignment == 0) continue;

            item.MarkLayoutDirty();
        }
    }

    protected void LayoutFromFlow()
    {
        PrepareChildren();
        ResizeChildrenWidth();
        RecalculateChildrenHeight();
        ResizeChildrenHeight();
        ApplyLayout();

        foreach (var item in FreeChildren)
        {
            if (item.Positioning != Positioning.Absolute) continue;
            if (item.LayoutIsDirty) continue;
            if (item.Width.Percent == 0 && item.Height.Percent == 0 &&
                item.Left.Percent == 0 && item.Top.Percent == 0 &&
                item.Left.Alignment == 0 && item.Top.Alignment == 0) continue;

            item.MarkLayoutDirty();
        }
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

        foreach (var child in ElementsCache.Where(el => el.Positioning != Positioning.Fixed))
        {
            child.RecalculatePosition();
        }
    }
}