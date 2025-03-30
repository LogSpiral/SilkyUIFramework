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

    protected List<UIView> ElementsSortedByZIndex { get; } = [];
    public bool ChildrenOrderIsDirty { get; set; } = true;

    protected void ReorderChildren()
    {
        ElementsSortedByZIndex.Clear();
        ElementsSortedByZIndex.AddRange(GetValidChildren().OrderBy(el => el.ZIndex));
    }

    public override void RefreshLayout()
    {
        if (ChildrenOrderIsDirty)
        {
            ReorderChildren();
            ChildrenOrderIsDirty = false;
        }

        if (LayoutIsDirty)
        {
            if (Positioning.IsFree())
            {
                var container = GetParentAvailableSpace();
                Prepare(container.Width, container.Height);
                ResizeChildrenWidth();
                RecalculateHeight();
                ResizeChildrenHeight();
                ApplyLayout();
            }
            else
            {
                PrepareChildren();
                ResizeChildrenWidth();
                RecalculateChildrenHeight();
                ResizeChildrenHeight();
                ApplyLayout();
            }

            CleanupDirtyMark();
        }

        foreach (var child in GetValidChildren())
        {
            child.RefreshLayout();
        }
    }

    public override void UpdatePosition()
    {
        base.UpdatePosition();

        foreach (var child in GetValidChildren())
        {
            child.UpdatePosition();
        }
    }

    public override void RecalculatePosition()
    {
        base.RecalculatePosition();

        foreach (var child in GetValidChildren().Where(el => el.Positioning is not Positioning.Fixed))
        {
            child.RecalculatePosition();
        }
    }
}