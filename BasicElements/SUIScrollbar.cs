using SilkyUIFramework.Animation;

namespace SilkyUIFramework.BasicElements;

public class SUIScrollbar : View
{
    #region ScrollPosition

    public event Action<Vector2> OnCurrentScrollPositionChanged;

    public virtual Vector2 CurrentScrollPosition
    {
        get => _currentScrollPosition;
        set
        {
            _currentScrollPosition = Vector2.Clamp(value, Vector2.Zero, GetScrollRange());
            OnCurrentScrollPositionChanged?.Invoke(_currentScrollPosition);
        }
    }

    private Vector2 _currentScrollPosition;

    private Vector2 _startScrollPosition;

    protected Vector2 StartScrollPosition
    {
        get => _startScrollPosition;
        set => _startScrollPosition = Vector2.Clamp(value, Vector2.Zero, GetScrollRange());
    }

    /// <summary>
    /// 滚动条目标位置
    /// </summary>
    public Vector2 TargetScrollPosition
    {
        get => Vector2.Clamp(_targetScrollPosition, Vector2.Zero, GetScrollRange());
        set
        {
            StartScrollPosition = CurrentScrollPosition;
            ScrollTimer.StartForwardUpdateAndReset();
            _targetScrollPosition = Vector2.Clamp(value, new Vector2(-500f),
                GetScrollRange() + new Vector2(500f));
        }
    }

    private Vector2 _targetScrollPosition;

    public void ScrollByTop() => TargetScrollPosition = Vector2.Zero;

    /// <summary>
    /// 横向滚动指定距离
    /// </summary>
    public void ScrollByEnd() => TargetScrollPosition = GetScrollRange();

    /// <summary>
    /// 横向滚动指定距离
    /// </summary>
    public void HScrollBy(float value) => TargetScrollPosition += new Vector2(value, 0);

    /// <summary>
    /// 纵向滚动指定距离
    /// </summary>
    public void VScrollBy(float value) => TargetScrollPosition += new Vector2(0, value);

    #endregion

    #region ContainerSize & ContentSize

    public Vector2 ContainerSize
    {
        get => _containerSize;
        set => _containerSize = Vector2.Max(Vector2.One, value);
    }

    private Vector2 _containerSize = Vector2.One;

    public Vector2 ContentSize
    {
        get => _contentSize;
        set => _contentSize = Vector2.Max(Vector2.One, value);
    }

    private Vector2 _contentSize = Vector2.One;

    #endregion

    #region Constructor 构造器

    public readonly Direction ScrollDirection;

    public readonly View TargetView;

    public readonly RoundedRectangle ControlBar = new();

    public readonly AnimationTimer ScrollTimer = new();

    public SUIScrollbar(Direction scrollDirection, View targetView)
    {
        DragIgnore = false;

        ScrollDirection = scrollDirection;
        TargetView = targetView;

        OnUpdateAnimationTimer += gameTime =>
            ScrollTimer.Update((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
    }

    #endregion

    #region Scroll Range

    public Vector2 GetScrollRange() => Vector2.Max(Vector2.Zero, ContentSize - ContainerSize);

    public void SetScrollRange(Vector2 maskSize, Vector2 targetSize)
    {
        ContainerSize = maskSize;
        ContentSize = targetSize;
    }

    public void SetHScrollRange(float containerWidth, float childrenWidth) =>
        SetScrollRange(new Vector2(containerWidth, 1f), new Vector2(childrenWidth, 1f));

    public void SetVScrollRange(float containerHeight, float childrenHeight) =>
        SetScrollRange(new Vector2(1f, containerHeight), new Vector2(1f, childrenHeight));

    #endregion

    public Vector2 GetBarPosition(Vector2 barSize)
    {
        var progress = CurrentScrollPosition / GetScrollRange();
        if (float.IsNaN(progress.X)) progress.X = 0;
        if (float.IsNaN(progress.Y)) progress.Y = 0;
        return Vector2.Max(Vector2.Zero, (_innerDimensions.Size() - barSize) * progress);
    }

    public Vector2 GetBarSize()
    {
        var ratio = ContainerSize / ContentSize;
        var size = _innerDimensions.Size() * ratio;

        Vector2 minSize;
        switch (ScrollDirection)
        {
            case BasicElements.Direction.Horizontal:
                minSize = new Vector2(_innerDimensions.Height * 2, _innerDimensions.Height);
                break;
            default:
            case BasicElements.Direction.Vertical:
                minSize = new Vector2(_innerDimensions.Width, _innerDimensions.Width * 2);
                break;
        }

        return Vector2.Clamp(size, minSize, _innerDimensions.Size());
    }

    public Vector2 GetBarRange() => _innerDimensions.Size() - GetBarSize();

    public Vector2 BarPositionOnScreen => _innerDimensions.Position() + GetBarPosition(GetBarSize());

    /// <summary> 直接设置滚动位置 </summary>
    public void SetScrollPosition(Vector2 position)
    {
        CurrentScrollPosition = position;
        StartScrollPosition = position;
        TargetScrollPosition = position;
    }

    public bool BarContainsPoint()
    {
        var focus = Main.MouseScreen;
        var barPos = BarPositionOnScreen;
        var barSize = GetBarSize();

        return focus.X > barPos.X && focus.Y > barPos.Y && focus.X < barPos.X + barSize.X &&
               focus.Y < barPos.Y + barSize.Y;
    }

    #region MouseDown & MouseUp

    protected bool IsScrollbarDragging;
    protected Vector2 BarDragOffset;

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        IsScrollbarDragging = true;
        BarDragOffset = Main.MouseScreen - GetBarPosition(GetBarSize());
        base.LeftMouseDown(evt);
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        IsScrollbarDragging = false;
        base.LeftMouseUp(evt);
    }

    #endregion

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        CurrentScrollPosition = ScrollTimer.Lerp(_startScrollPosition, _targetScrollPosition);
        if (IsScrollbarDragging)
        {
            var barPosition = Main.MouseScreen - BarDragOffset;
            var scrollPosition = barPosition * GetScrollRange() / GetBarRange();
            if (float.IsNaN(scrollPosition.X)) scrollPosition.X = 0;
            if (float.IsNaN(scrollPosition.Y)) scrollPosition.Y = 0;
            SetScrollPosition(scrollPosition);
        }

        base.DrawSelf(spriteBatch);
        DrawScrollbar();
    }

    public (Color Default, Color Hover) BarColor = (Color.Black * 0.2f, Color.Black * 0.3f);

    protected virtual void DrawScrollbar()
    {
        if (GetBarSize() is not { X: > 0, Y: > 0 } barSize) return;

        var barIsHover = IsScrollbarDragging || BarContainsPoint();
        ControlBar.CornerRadius = new Vector4(Math.Min(barSize.X, barSize.Y) / 2f);
        ControlBar.BgColor = barIsHover ? BarColor.Hover : BarColor.Default;
        ControlBar.Draw(BarPositionOnScreen, barSize, false, FinalMatrix);
    }
}