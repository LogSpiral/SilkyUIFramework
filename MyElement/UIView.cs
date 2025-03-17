using Iced.Intel;
using SilkyUIFramework.Core;

namespace SilkyUIFramework.MyElement;

public static class PositioningExtensions
{
    public static bool IsFree(this Positioning positioning) => positioning is Positioning.Fixed or Positioning.Absolute;
}

public partial class UIView
{
    #region Invalid DirtyMark

    public bool Invalid
    {
        get => _invalid;
        set
        {
            if (_invalid == value) return;
            _invalid = value;
            MarkDirty();
        }
    }

    private bool _invalid;

    protected bool IsDirty { get; set; } = true;

    protected void MarkDirty()
    {
        IsDirty = true;
        if (Positioning.IsFree()) return;
        Parent?.OnChildDirty();
    }

    public virtual void CleanupDirtyMark() => IsDirty = false;

    #endregion

    #region Parent Remove() ContainsPoint() GetElementAt()

    public ViewGroup Parent { get; protected internal set; }

    public virtual void Remove() => Parent?.RemoveChild(this);

    public virtual bool ContainsPoint(Vector2 point) => Bounds.Contains(point);

    public virtual UIView GetElementAt(Vector2 position)
    {
        return !Invalid && ContainsPoint(position) ? this : null;
    }

    #endregion

    /// <summary> 按条件获取祖先元素 </summary>
    public UIView GetAncestor(Func<UIView, bool> condition = null)
    {
        if (condition is null) return Parent;

        var parent = Parent;

        do
        {
            if (condition(parent)) return this;
            parent = parent.Parent;
        } while (parent != null);

        return null;
    }

    protected bool PositionDirty { get; set; } = true;

    public Positioning Positioning
    {
        get => _positioning;
        set
        {
            if (_positioning == value) return;
            var freeChanged = _positioning.IsFree() != value.IsFree();
            _positioning = value;
            PositionDirty = true;
            if (freeChanged)
            {
                IsDirty = true;
                Parent?.OnChildDirty();
            }
        }
    }

    private Positioning _positioning;

    public StickyType StickyType
    {
        get => _stickyType;
        set
        {
            if (value == _stickyType) return;
            _stickyType = value;
            if (Positioning == Positioning.Sticky) PositionDirty = true;
        }
    }

    private StickyType _stickyType;

    public Vector4 Sticky
    {
        get => _sticky;
        set
        {
            if (_sticky == value) return;
            _sticky = value;
            if (Positioning is Positioning.Sticky) PositionDirty = true;
        }
    }

    private Vector4 _sticky;

    #region Anchor

    private Anchor _left;
    private Anchor _top;

    public Anchor Left
    {
        get => _left;
        set => SetLeft(value.Pixels, value.Percent, value.Alignment);
    }

    public Anchor Top
    {
        get => _top;
        set => SetTop(value.Pixels, value.Percent, value.Alignment);
    }

    public void SetLeft(float? pixels = null, float? percent = null, float? alignment = null) =>
        SetAnchor(ref _left, pixels, percent, alignment);

    public void SetTop(float? pixels = null, float? percent = null, float? alignment = null) =>
        SetAnchor(ref _top, pixels, percent, alignment);

    private void SetAnchor(ref Anchor anchor, float? pixels = null, float? percent = null, float? alignment = null)
    {
        if (pixels == anchor.Pixels && percent == anchor.Percent && alignment == anchor.Alignment) return;
        anchor = anchor.With(pixels, percent, alignment);

        if (Positioning != Positioning.Static)
            PositionDirty = true;
    }

    #endregion

    #region _layoutOffset _dragOffset

    private Vector2 _layoutOffset;

    public Vector2 LayoutOffset
    {
        get => _layoutOffset;
        set => SetLayoutOffset(value.X, value.Y);
    }

    protected internal void SetLayoutOffset(float? x = null, float? y = null)
    {
        if (x == _layoutOffset.X && y == _layoutOffset.Y) return;

        if (x.HasValue) _layoutOffset.X = x.Value;
        if (y.HasValue) _layoutOffset.Y = y.Value;

        PositionDirty = true;
    }

    private Vector2 _dragOffset;

    protected Vector2 DragOffset
    {
        get => _dragOffset;
        set => SetDragOffset(value.X, value.Y);
    }

    public void SetDragOffset(float? x = null, float? y = null)
    {
        if ((!x.HasValue || x.Value == _layoutOffset.X) && (!y.HasValue || y.Value == _layoutOffset.Y)) return;

        if (x.HasValue) _dragOffset.X = x.Value;
        if (y.HasValue) _dragOffset.Y = y.Value;

        if (Positioning != Positioning.Static) return;
        PositionDirty = true;
    }

    #endregion

    public virtual void UpdatePosition()
    {
        if (PositionDirty)
        {
            RecalculatePosition();
        }
    }

    public virtual void RecalculatePosition()
    {
        switch (Positioning)
        {
            // 固定定位: 相对屏幕, 不受到布局影响
            case Positioning.Fixed:
            {
                var container = DeviceHelper.GetViewportSizeByUIScale();
                OuterBounds.X = Left.CalculatePosition(container.Width, OuterBounds.Width) + DragOffset.X;
                OuterBounds.Y = Top.CalculatePosition(container.Height, OuterBounds.Height) + DragOffset.Y;
                break;
            }
            // 绝对定位: 相对父元素, 不受到布局影响
            case Positioning.Absolute:
            {
                var container = Parent?.InnerBounds ??
                                new Bounds(Vector2.Zero, DeviceHelper.GetViewportSizeByUIScale());

                OuterBounds.X = container.X + Left.CalculatePosition(container.Width, OuterBounds.Width) +
                                DragOffset.X;
                OuterBounds.Y = container.Y + Top.CalculatePosition(container.Height, OuterBounds.Height) +
                                DragOffset.Y;
                break;
            }
            // 粘性定位 and 相对定位
            default:
            case Positioning.Sticky:
            case Positioning.Relative:
            {
                var container = Parent?.InnerBounds ??
                                new Bounds(Vector2.Zero, DeviceHelper.GetViewportSizeByUIScale());

                var scroll = Parent?.ScrollOffset ?? Vector2.Zero;
                OuterBounds.X = container.X + scroll.X + Left.CalculatePosition(container.Width, OuterBounds.Width) +
                                LayoutOffset.X + DragOffset.X;
                OuterBounds.Y = container.Y + scroll.Y + Top.CalculatePosition(container.Height, OuterBounds.Height) +
                                LayoutOffset.Y + DragOffset.Y;
                HandleStickyPositioning(container);
                break;
            }
            // 完全由父元素控制
            case Positioning.Static:
            {
                var container = Parent?.InnerBounds ??
                                new Bounds(Vector2.Zero, DeviceHelper.GetViewportSizeByUIScale());

                var scroll = Parent?.ScrollOffset ?? Vector2.Zero;
                OuterBounds.X = container.X + scroll.X + LayoutOffset.X;
                OuterBounds.Y = container.Y + scroll.Y + LayoutOffset.Y;
                break;
            }
        }

        Bounds.Position = OuterBounds.Position + Margin;
        InnerBounds.Position = Bounds.Position + Border + Padding;

        PositionDirty = false;
    }

    protected virtual void HandleStickyPositioning(Bounds container)
    {
        if (Positioning != Positioning.Sticky) return;

        if (StickyType.HasFlag(StickyType.Left))
            OuterBounds.X = Math.Max(OuterBounds.X, container.X + Sticky.X);

        if (StickyType.HasFlag(StickyType.Top))
            OuterBounds.Y = Math.Max(OuterBounds.Y, container.Y + Sticky.Y);

        if (StickyType.HasFlag(StickyType.Right))
            OuterBounds.X = Math.Min(OuterBounds.X, container.X + container.Width - OuterBounds.Width - Sticky.Z);

        if (StickyType.HasFlag(StickyType.Bottom))
            OuterBounds.Y = Math.Min(OuterBounds.Y, container.Y + container.Height - OuterBounds.Height - Sticky.W);
    }

    protected internal Bounds Bounds;
    public Bounds GetBounds() => Bounds;

    protected internal Bounds InnerBounds;
    public Bounds GetInnerBounds() => InnerBounds;

    protected internal Bounds OuterBounds;
    public Bounds GetOuterBounds() => OuterBounds;


    public event Action<GameTime> OnUpdateStatus;
    protected virtual void UpdateStatus(GameTime gameTime) { }

    public virtual void HandleUpdateStatus(GameTime gameTime)
    {
        OnUpdateStatus?.Invoke(gameTime);
        UpdateStatus(gameTime);
    }

    public event Action<GameTime> OnUpdate;
    protected virtual void Update(GameTime gameTime) { }

    public virtual void HandleUpdate(GameTime gameTime)
    {
        OnUpdate?.Invoke(gameTime);
        Update(gameTime);
    }

    public event Action<GameTime, SpriteBatch> DrawAction;

    protected virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var viewport = Main.graphics.GraphicsDevice.Viewport;
        var matrix = Main.UIScaleMatrix;

        var background = Color.White * 0.2f;
        var borderColor = Color.White * 0.5f;
        SDFRectangle.DrawWithBorder(Bounds.Position, Bounds.Size, new Vector4(0f), background, 2, borderColor, matrix);
    }

    public virtual void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawAction?.Invoke(gameTime, spriteBatch);
        Draw(gameTime, spriteBatch);
    }
}
