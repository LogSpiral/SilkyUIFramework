namespace SilkyUIFramework.BasicElements;

public abstract partial class BasicBody : UIElementGroup
{
    public virtual bool Enabled { get; set; } = true;
    public virtual bool IsInteractable => true;

    protected bool AvailableItem { get; set; } = false;
    protected bool AvailableScroll { get; set; } = false;

    protected BasicBody()
    {
        SetSize(16f * 30f, 9f * 30f);
        SetGap(10f);

        Positioning = Positioning.Fixed;
        Border = 2f;
        BorderColor = Color.Black;
        BackgroundColor = Color.White * 0.25f;

        LayoutType = LayoutType.Flexbox;
        FlexDirection = FlexDirection.Column;
        MainAlignment = MainAlignment.Start;
        FlexWrap = false;
        FinallyDrawBorder = true;
    }

    //private bool _adjustingWidth;
    //private bool _adjustingHeight;

    //private float _startWidth;
    //private float _startHeight;

    //private Vector2 _startPosition;

    //public override void OnLeftMouseDown(UIMouseEvent evt)
    //{
    //    base.OnLeftMouseDown(evt);

    //    if (evt.Source == this)
    //    {
    //        if (evt.MousePosition.X > Bounds.Right - Border - 2)
    //        {
    //            _startWidth = Bounds.Width;
    //            _adjustingWidth = true;
    //            FitWidth = false;
    //        }

    //        //_startHeight = Bounds.Height;
    //        //_adjustingHeight = true;

    //        _startPosition = evt.MousePosition;
    //    }
    //}

    //public override void OnLeftMouseUp(UIMouseEvent evt)
    //{
    //    base.OnLeftMouseUp(evt);

    //    _adjustingWidth = false;
    //    _adjustingHeight = false;
    //}

    protected override void UpdateStatus(GameTime gameTime)
    {
        WatchScreenSize();

        //if (_adjustingWidth)
        //{
        //    var offset = Main.MouseScreen.X - _startPosition.X;
        //    SetWidth(_startWidth + offset * 2f);
        //}

        //if (_adjustingHeight)
        //{
        //    var offset = Main.MouseScreen.Y - _startPosition.Y;
        //    SetHeight(_startHeight + offset * 2f);
        //}

        if (IsMouseHovering)
        {
            if (!AvailableScroll)
            {
                PlayerInput.LockVanillaMouseScroll("SilkyUIFramework");
            }

            if (!AvailableItem)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        base.UpdateStatus(gameTime);
    }

    protected Size oldScreenSize = GraphicsDeviceHelper.GetBackBufferSize();

    protected void WatchScreenSize()
    {
        var newScreenSize = GraphicsDeviceHelper.GetBackBufferSizeByUIScale();
        if (oldScreenSize == newScreenSize) return;

        OnScreenSizeChanged(newScreenSize, oldScreenSize);
        oldScreenSize = newScreenSize;
    }

    protected virtual void OnScreenSizeChanged(Size newScreenSize, Size oldScreenSize)
    {
        MarkLayoutDirty();
    }

    public override UIView GetElementAt(Vector2 mousePosition)
    {
        if (Invalid) return null;

        if (!ContainsPoint(mousePosition)) return null;

        foreach (var child in ElementsSortedByZIndex.Reverse<UIView>())
        {
            var target = child.GetElementAt(mousePosition);
            if (target != null) return target;
        }

        // 所有子元素都不符合条件, 如果自身不忽略鼠标交互, 则返回自己
        return IgnoreMouseInteraction ? null : this;
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
            var container = GetParentInnerSpace();
            Prepare(container.Width, container.Height);
            ResizeChildrenWidth();
            RecalculateHeight();
            ResizeChildrenHeight();
            ApplyLayout();

            CleanupDirtyMark();
        }

        foreach (var child in GetValidChildren())
        {
            child.RefreshLayout();
        }
    }
}