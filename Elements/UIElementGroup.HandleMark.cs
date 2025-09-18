namespace SilkyUIFramework.Elements;

public partial class UIElementGroup
{
    protected internal virtual void NotifyParentChildDirty()
    {
        LayoutIsDirty = true;
        PositionIsDirty = true;

        if (FitWidth || FitHeight)
        {
            Parent?.NotifyParentChildDirty();
        }
    }

    public bool ElementsOrderIsDirty { get; set; } = true;

    protected List<UIView> ElementsInOrder { get; } = [];

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
                UpdateLayoutFromFree();
            }
            else
            {
                UpdateLayoutFromFlow();
            }

            CleanupDirtyMark();
        }

        foreach (var child in ElementsCache)
        {
            child.UpdateLayout();
        }
    }

    protected void UpdateLayoutFromFree()
    {
        var container = GetParentInnerSpace();
        Prepare(container.Width, container.Height);
        RecalculateWidth();
        ResizeChildrenWidth();
        RecalculateHeight();
        ResizeChildrenHeight();
        UpdateChildrenLayoutOffset();

        MarkFreeElementsDirty();
    }

    protected void UpdateLayoutFromFlow()
    {
        PrepareChildren();
        RecalculateChildrenWidth();
        ResizeChildrenWidth();
        RecalculateChildrenHeight();
        ResizeChildrenHeight();
        UpdateChildrenLayoutOffset();

        MarkFreeElementsDirty();
    }

    protected void MarkFreeElementsDirty()
    {
        foreach (var item in from item in FreeElements
                             where item.Positioning == Positioning.Absolute
                             where !item.LayoutIsDirty || item.IsDependentParent()
                             select item)
        {
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