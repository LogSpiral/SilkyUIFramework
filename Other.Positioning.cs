using Microsoft.CodeAnalysis;
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
            IsPositionDirty = true;
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
            IsPositionDirty = true;
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
            BubbleMarkerDirty();
        }
    }
    private Positioning _positioning;

    /// <summary>
    /// 由父元素控制, 不应直接修改
    /// </summary>
    protected Vector2 LayoutOffset
    {
        get => _layoutOffset;
        set
        {
            if (value == _layoutOffset) return;
            _layoutOffset = value;
            IsPositionDirty = true;
        }
    }
    private Vector2 _layoutOffset;

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
            IsPositionDirty = true;
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
            IsPositionDirty = true;
        }
    }
    private Vector2 _scrollOffset;

    public StickyType StickyType { get; set; }
    public Vector4 Sticky { get; set; }

    protected virtual void ApplyPosition(Bounds container, Vector2 scroll)
    {
        _outerBounds.X = container.X + scroll.X + LeftUnit.GetValue(container.Width) + LayoutOffset.X + DragOffset.X;
        _outerBounds.Y = container.Y + scroll.Y + TopUnit.GetValue(container.Height) + LayoutOffset.Y + DragOffset.Y;

        HandleStickyPositioning(container);

        _bounds.Position = _outerBounds.Position + Margin;
        _innerBounds.Position = _bounds.Position + Border + Padding;

        foreach (var child in _children)
        {
            child.ApplyPosition(_innerBounds, ScrollOffset);
        }

        IsPositionDirty = false;
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