using SilkyUIFramework.Core;

namespace SilkyUIFramework;

public partial class Other
{
    public Unit LeftUnit
    {
        get => _leftUnit;
        set
        {
            if (value == _leftUnit) return;
            _leftUnit = value;
            PositionDirty = true;
        }
    }
    private Unit _leftUnit;

    public Unit TopUnit
    {
        get => _topUnit;
        set
        {
            if (value == _topUnit) return;
            _topUnit = value;
            PositionDirty = true;
        }
    }
    private Unit _topUnit;

    public Positioning Positioning
    {
        get => _positioning;
        set
        {
            if (value == _positioning) return;
            _positioning = value;
            MarkLayoutDirty();
        }
    }
    private Positioning _positioning;

    /// <summary>
    /// 由父元素控制, 不应直接修改
    /// </summary>
    protected Vector2 LayoutPosition => _layoutPosition;
    private Vector2 _layoutPosition;

    protected void SetLayoutPosition(Vector2 position) => SetLayoutPosition(position.X, position.Y);
    protected void SetLayoutPosition(float? x = null, float? y = null)
    {
        if ((!x.HasValue || x.Value == _layoutPosition.X) && (!y.HasValue || y.Value == _layoutPosition.Y)) return;

        _layoutPosition.X = x.Value;
        _layoutPosition.Y = y.Value;
        PositionDirty = true;
    }

    /// <summary>
    /// 由拖动元素控制, 不应直接修改
    /// </summary>
    protected Vector2 DragOffset
    {
        get => _dragOffset;
        set
        {
            if (value == _dragOffset) return;
            _dragOffset = value;
            PositionDirty = true;
        }
    }
    private Vector2 _dragOffset;

    /// <summary>
    /// 由滚动元素控制子元素滚动位置, 不应直接修改
    /// </summary>
    protected Vector2 ScrollOffset
    {
        get => _scrollOffset;
        set
        {
            if (value == _scrollOffset) return;
            _scrollOffset = value;
            PositionDirty = true;
        }
    }
    private Vector2 _scrollOffset;

    public StickyType StickyType
    {
        get => _stickyType;
        set
        {
            if (value == _stickyType) return;
            _stickyType = value;
            if (Positioning == Positioning.Sticky)
                PositionDirty = true;
        }
    }
    private StickyType _stickyType;

    /// <summary>
    /// X: 左边界, Y: 上边界, Z: 右边界, W: 下边界
    /// </summary>
    public Vector4 Sticky
    {
        get => _sticky;
        set
        {
            if (value == _sticky) return;
            _sticky = value;
            if (Positioning == Positioning.Sticky)
                PositionDirty = true;
        }
    }
    private Vector4 _sticky;

    public void SetSticky(StickyType stickyType, float? left = null, float? top = null, float? right = null, float? bottom = null)
    {
    }

    public static Size GetViewportSize()
    {
        return Main.graphics.GraphicsDevice.Viewport.Bounds.Size() / Main.UIScale;
    }

    protected virtual void ApplyPosition()
    {
        switch (Positioning)
        {
            // 固定定位: 相对屏幕, 不受到布局影响
            case Positioning.Fixed:
            {
                Size container = GetViewportSize();
                _outerBounds.X = LeftUnit.GetValue(container.Width) + DragOffset.X;
                _outerBounds.Y = TopUnit.GetValue(container.Height) + DragOffset.Y;
                break;
            }
            // 绝对定位: 相对父元素, 不受到布局影响
            case Positioning.Absolute:
            {
                Bounds container = Parent is null ? new Bounds(Vector2.Zero, GetViewportSize()) : Parent._innerBounds;

                _outerBounds.X = container.X + LeftUnit.GetValue(container.Width) + DragOffset.X;
                _outerBounds.Y = container.Y + TopUnit.GetValue(container.Height) + DragOffset.Y;
                break;
            }
            // 粘性定位 and 相对定位
            default:
            case Positioning.Sticky:
            case Positioning.Relative:
            {
                Bounds container = Parent is null ? new Bounds(Vector2.Zero, GetViewportSize()) : Parent._innerBounds;

                var scroll = Parent?.ScrollOffset ?? Vector2.Zero;
                _outerBounds.X = container.X + scroll.X + LeftUnit.GetValue(container.Width) + LayoutPosition.X + DragOffset.X;
                _outerBounds.Y = container.Y + scroll.Y + TopUnit.GetValue(container.Height) + LayoutPosition.Y + DragOffset.Y;
                HandleStickyPositioning(container);
                break;
            }
        }

        _bounds.Position = _outerBounds.Position + Margin;
        _innerBounds.Position = _bounds.Position + Border + Padding;

        ApplyChildrenPosition();

        PositionDirty = false;
    }

    protected virtual void ApplyChildrenPosition()
    {
        foreach (var child in _children)
        {
            child.ApplyPosition();
        }
    }

    /// <summary>
    /// Sticky 定位应该根据父元素实际 Bounds 决定
    /// </summary>
    protected virtual void HandleStickyPositioning(Bounds container)
    {
        if (Positioning is not Positioning.Sticky) return;

        if (StickyType.HasFlag(StickyType.Left))
        {
            _outerBounds.X = Math.Max(_outerBounds.X, container.X + Sticky.X);
        }

        if (StickyType.HasFlag(StickyType.Top))
        {
            _outerBounds.Y = Math.Max(_outerBounds.Y, container.Y + Sticky.Y);
        }

        if (StickyType.HasFlag(StickyType.Right))
        {
            _outerBounds.X = Math.Min(_outerBounds.X,
                container.X + container.Width - _outerBounds.Width - Sticky.Z);
        }

        if (StickyType.HasFlag(StickyType.Bottom))
        {
            _outerBounds.Y = Math.Min(_outerBounds.Y,
                container.Y + container.Height - _outerBounds.Height - Sticky.W);
        }
    }
}