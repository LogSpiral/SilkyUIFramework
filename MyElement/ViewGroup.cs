using SilkyUIFramework.MyElement;

namespace SilkyUIFramework;

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public partial class ViewGroup : UIView
{
    public bool OverflowHidden { get; set; }

    protected readonly List<UIView> Children = [];
    public IReadOnlyList<UIView> ReadOnlyChildren => Children;

    public override void CleanupDirtyMark()
    {
        base.CleanupDirtyMark();

        foreach (var child in Children)
        {
            child.CleanupDirtyMark();
        }
    }

    #region Append Remove RemoveChild

    public virtual void AppendChild(UIView child)
    {
        child.Remove();
        Children.Add(child);
        child.Parent = this;
        MarkDirty();
        PositionDirty = true;
    }

    public virtual void RemoveChild(UIView child)
    {
        Children.Remove(child);
        child.Parent = null;
        MarkDirty();
        PositionDirty = true;
    }

    public virtual void RemoveChildren()
    {
        foreach (var child in Children)
        {
            child.Parent = null;
        }

        Children.Clear();
        MarkDirty();
        PositionDirty = true;
    }

    #endregion

    #region Update UpdateStatus Draw

    public override void HandleUpdate(GameTime gameTime)
    {
        base.HandleUpdate(gameTime);
        UpdateChildren(gameTime);
    }

    protected virtual void UpdateChildren(GameTime gameTime)
    {
        foreach (var child in Children.Where(_ => !Invalid)) child.HandleUpdate(gameTime);
    }

    public override void HandleUpdateStatus(GameTime gameTime)
    {
        base.HandleUpdateStatus(gameTime);
        UpdateChildrenStatus(gameTime);
    }

    protected virtual void UpdateChildrenStatus(GameTime gameTime)
    {
        foreach (var child in Children.Where(_ => !Invalid))
        {
            child.HandleUpdateStatus(gameTime);
        }
    }

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.HandleDraw(gameTime, spriteBatch);
        DrawChildren(gameTime, spriteBatch);
    }

    public virtual void DrawChildren(GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach (var child in Children) child.HandleDraw(gameTime, spriteBatch);
    }

    #endregion

    protected readonly List<UIView> FreeElements = [];
    protected readonly List<UIView> LayoutElements = [];

    protected void Classify()
    {
        FreeElements.Clear();
        LayoutElements.Clear();

        foreach (var child in Children.Where(_ => !Invalid))
        {
            if (Positioning is Core.Positioning.Absolute or Core.Positioning.Fixed)
            {
                FreeElements.Add(child);
            }
            else
            {
                LayoutElements.Add(child);
            }
        }
    }

    public override UIView GetElementAt(Vector2 position)
    {
        if (Invalid) return null;

        if (OverflowHidden)
        {
            if (!ContainsPoint(position)) return null;
            foreach (var child in Children)
            {
                if (child.GetElementAt(position) is { } target) return target;
            }

            return this;
        }

        foreach (var child in Children)
        {
            if (child.GetElementAt(position) is { } target) return target;
        }

        return ContainsPoint(position) ? this : null;
    }

    public Vector2 ScrollOffset
    {
        get => _scrollOffset;
        protected set
        {
            if (value == _scrollOffset) return;
            _scrollOffset = value;
            PositionDirty = true;
        }
    }

    private Vector2 _scrollOffset;
}