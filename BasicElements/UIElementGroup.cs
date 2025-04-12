namespace SilkyUIFramework;

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public partial class UIElementGroup : UIView
{
    public bool OverflowHidden { get; set; }

    protected List<UIView> Elements { get; } = [];
    public IEnumerable<UIView> GetValidChildren() => Elements.Where(el => !el.Invalid);
    public IReadOnlyList<UIView> Children => Elements;

    public sealed override void HandleMounted(SilkyUI silkyUI)
    {
        base.HandleMounted(silkyUI);

        foreach (var el in Children)
        {
            el.HandleMounted(silkyUI);
        }
    }

    public sealed override void HandleUnmounted()
    {
        base.HandleUnmounted();

        foreach (var el in Children)
        {
            el.HandleUnmounted();
        }
    }

    /// <summary>
    /// 尺寸与布局完成后, 清理脏标记 (只会清理 <see cref="LayoutChildren"/>)
    /// </summary>
    public override void CleanupDirtyMark()
    {
        base.CleanupDirtyMark();

        // 仅针对
        foreach (var child in LayoutChildren)
        {
            child.CleanupDirtyMark();
        }
    }

    #region Append Remove RemoveChild

    public virtual bool HasChild(UIView child) => Elements.Contains(child);

    public virtual void AppendChild(UIView child)
    {
        child.Remove();
        Elements.Add(child);
        child.Parent = this;
        MarkLayoutDirty();
        MarkPositionDirty();
        ChildrenOrderIsDirty = true;

        if (SilkyUI != null) child.HandleMounted(SilkyUI);
    }

    public virtual void RemoveChild(UIView child)
    {
        if (!Elements.Remove(child)) return;

        child.Parent = null;
        MarkLayoutDirty();
        MarkPositionDirty();
        ChildrenOrderIsDirty = true;
        child.HandleUnmounted();
    }

    public virtual void RemoveAllChildren()
    {
        foreach (var child in Elements.ToArray())
        {
            RemoveChild(child);
        }
    }

    #endregion

    public UIView SelectedElement { get; protected set; }

    public virtual void SelectChild(UIView selectTarget)
    {
        if (!Elements.Contains(selectTarget)) return;

        SelectedElement?.HandleDeselected();
        SelectedElement = selectTarget;
        SelectedElement?.HandleSelected();
    }

    public override void OnLeftMouseDown(UIMouseEvent evt)
    {
        if (evt.Source != this && evt.Previous is { Selectable: true })
        {
            SelectChild(evt.Previous);
        }

        base.OnLeftMouseDown(evt);
    }

    #region Update UpdateStatus Draw

    public override void HandleUpdate(GameTime gameTime)
    {
        base.HandleUpdate(gameTime);
        UpdateChildren(gameTime);
    }

    protected virtual void UpdateChildren(GameTime gameTime)
    {
        foreach (var child in GetValidChildren()) child.HandleUpdate(gameTime);
    }

    public override void HandleUpdateStatus(GameTime gameTime)
    {
        base.HandleUpdateStatus(gameTime);
        UpdateChildrenStatus(gameTime);
    }

    protected virtual void UpdateChildrenStatus(GameTime gameTime)
    {
        foreach (var child in GetValidChildren())
        {
            child.HandleUpdateStatus(gameTime);
        }
    }

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.HandleDraw(gameTime, spriteBatch);

        DrawChildren(gameTime, spriteBatch);
    }

    public virtual Rectangle GetClippingRectangle(SpriteBatch spriteBatch)
    {
        var bounds = HiddenBox switch
        {
            HiddenBox.Outer => OuterBounds,
            HiddenBox.Inner => InnerBounds,
            _ => Bounds,
        };

        var topLeft = Vector2.Transform(bounds.Position, SilkyUI.TransformMatrix);
        var rightBottom = Vector2.Transform(bounds.RightBottom, SilkyUI.TransformMatrix);
        var rectangle = new Rectangle(
                (int)Math.Floor(topLeft.X), (int)Math.Floor(topLeft.Y),
                (int)Math.Ceiling(rightBottom.X - topLeft.X),
                (int)Math.Ceiling(rightBottom.Y - topLeft.Y));

        var scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
        return Rectangle.Intersect(rectangle, scissorRectangle);
    }

    public virtual void DrawChildren(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (OverflowHidden)
        {
            var innerBounds = InnerBounds;
            spriteBatch.End();
            var originalScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
            var scissor = Rectangle.Intersect(GetClippingRectangle(spriteBatch), originalScissor);
            spriteBatch.GraphicsDevice.ScissorRectangle = scissor;
            var renderStatus = RenderStates.BackupStates(Main.graphics.GraphicsDevice, spriteBatch);
            spriteBatch.Begin(SpriteSortMode.Deferred,
                null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, SilkyUI.TransformMatrix);

            foreach (var child in ElementsSortedByZIndex.Where(el => el.OuterBounds.Intersects(innerBounds)))
            {
                child.HandleDraw(gameTime, spriteBatch);
            }

            spriteBatch.End();
            spriteBatch.GraphicsDevice.ScissorRectangle = originalScissor;
            renderStatus.Begin(spriteBatch, SpriteSortMode.Deferred);

            return;
        }

        foreach (var child in ElementsSortedByZIndex)
        {
            child.HandleDraw(gameTime, spriteBatch);
        }
    }

    #endregion

    protected readonly List<UIView> FreeChildren = [];
    protected readonly List<UIView> LayoutChildren = [];

    protected void ClassifyChildren()
    {
        FreeChildren.Clear();
        LayoutChildren.Clear();

        foreach (var child in GetValidChildren())
        {
            if (child.Positioning.IsFree())
            {
                FreeChildren.Add(child);
            }
            else
            {
                LayoutChildren.Add(child);
            }
        }
    }

    public override UIView GetElementAt(Vector2 mousePosition)
    {
        if (Invalid) return null;

        // 开启溢出隐藏后, 需要先检查自身是否包含点
        if (OverflowHidden)
        {
            if (!ContainsPoint(mousePosition)) return null;

            foreach (var child in ElementsSortedByZIndex.Reverse<UIView>())
            {
                var target = child.GetElementAt(mousePosition);
                if (target != null) return target;
            }

            // 所有子元素都不符合条件, 如果自身不忽略鼠标交互, 则返回自己
            return IgnoreMouseInteraction ? null : this;
        }

        // 没有开启溢出隐藏, 直接检查所有有效子元素
        foreach (var child in ElementsSortedByZIndex.Reverse<UIView>())
        {
            var target = child.GetElementAt(mousePosition);
            if (target != null) return target;
        }

        // 忽略鼠标交互
        if (IgnoreMouseInteraction) return null;

        // 元素包含点
        return ContainsPoint(mousePosition) ? this : null;
    }

    public Vector2 ScrollOffset
    {
        get;
        protected set
        {
            if (field == value) return;
            field = value;
            MarkPositionDirty();
        }
    }
}