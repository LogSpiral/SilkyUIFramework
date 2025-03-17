using SilkyUIFramework.Core;

namespace SilkyUIFramework.MyElement;

public partial class UIView
{
    #region BoxSizing Margin Padding Border

    public BoxSizing BoxSizing
    {
        get => _boxSizing;
        set
        {
            if (_boxSizing == value) return;
            _boxSizing = value;
            MarkDirty();
            PositionDirty = true;
        }
    }

    private BoxSizing _boxSizing;

    public Margin Margin
    {
        get => _margin;
        set
        {
            if (_margin == value) return;
            _margin = value;
            MarkDirty();
            PositionDirty = true;
        }
    }

    private Margin _margin;

    public Margin Padding
    {
        get => _padding;
        set
        {
            if (_padding == value) return;
            _padding = value;
            MarkDirty();
            PositionDirty = true;
        }
    }

    private Margin _padding;

    public Margin Border
    {
        get => _border;
        set
        {
            if (_border == value) return;
            _border = value;
            MarkDirty();
            PositionDirty = true;
        }
    }

    private Margin _border = new(2f);

    #endregion

    #region Width Height Automatic

    protected Size Container;

    private bool _automaticWidth;
    private bool _automaticHeight;

    public bool AutomaticWidth
    {
        get => _automaticWidth;
        set
        {
            if (_automaticWidth == value) return;
            _automaticWidth = value;
            MarkDirty();
        }
    }

    public bool AutomaticHeight
    {
        get => _automaticHeight;
        set
        {
            if (_automaticHeight == value) return;
            _automaticHeight = value;
            MarkDirty();
        }
    }

    private Dimension _minWidth;
    private Dimension _maxWidth = new(ushort.MaxValue * 10);
    private Dimension _width;

    private Dimension _minHeight;
    private Dimension _maxHeight = new(ushort.MaxValue * 10);
    private Dimension _height;

    #endregion

    #region SetDimension

    private void SetDimension(ref Dimension dimension, float? pixels = null, float? percent = null)
    {
        if (dimension.Pixels == pixels && dimension.Percent == percent) return;
        dimension = dimension.With(pixels, percent);
        MarkDirty();
    }

    public void SetWidth(float? pixels = null, float? percent = null) =>
        SetDimension(ref _width, pixels, percent);

    public void SetHeight(float? pixels = null, float? percent = null) =>
        SetDimension(ref _height, pixels, percent);

    public void SetMinWidth(float? pixels = null, float? percent = null) =>
        SetDimension(ref _minWidth, pixels, percent);

    public void SetMaxWidth(float? pixels = null, float? percent = null) =>
        SetDimension(ref _maxWidth, pixels, percent);

    public void SetMinHeight(float? pixels = null, float? percent = null) =>
        SetDimension(ref _minHeight, pixels, percent);

    public void SetMaxHeight(float? pixels = null, float? percent = null) =>
        SetDimension(ref _maxHeight, pixels, percent);

    #endregion

    public virtual void UpdateBounds()
    {
        if (IsDirty)
        {
            var container = (Parent is null || Positioning.IsFree())
                ? DeviceHelper.GetViewportSizeByUIScale()
                : Parent.InnerBounds.Size;
            Measure(container);
            Trim(container);
            CleanupDirtyMark();
        }
    }

    public virtual void Measure(Size container)
    {
        RecalculateWidthConstraint(container.Width);
        RecalculateHeightConstraint(container.Height);

        RecalculateBoundsWidth(_automaticWidth ? 0 : container.Width);
        RecalculateBoundsHeight(_automaticHeight ? 0 : container.Height);

        Container = container;
    }

    public virtual void Trim(Size container, float? outerWidth = null, float? outerHeight = null)
    {
        if (outerWidth.HasValue)
        {
            SetOuterBoundsWidth(outerWidth.Value);
        }
        else if (!_automaticWidth && Container.Width != container.Width)
        {
            RecalculateWidthConstraint(container.Width);
            RecalculateBoundsWidth(container.Width);
        }

        if (outerHeight.HasValue)
        {
            SetOuterBoundsHeight(outerHeight.Value);
        }
        else if (!_automaticHeight && Container.Height != container.Height)
        {
            RecalculateHeightConstraint(container.Height);
            RecalculateBoundsHeight(container.Height);
        }
    }

    #region Calculate Constraint: Original Inner Outer

    public float MinWidthValue { get; private set; }
    public float MaxWidthValue { get; private set; }
    public float WidthValue { get; private set; }

    public float MinHeightValue { get; private set; }
    public float MaxHeightValue { get; private set; }
    public float HeightValue { get; private set; }

    public float MinOuterWidth { get; private set; }
    public float MaxOuterWidth { get; private set; }
    public float MinOuterHeight { get; private set; }
    public float MaxOuterHeight { get; private set; }

    public float MinInnerWidth { get; private set; }
    public float MaxInnerWidth { get; private set; }
    public float MinInnerHeight { get; private set; }
    public float MaxInnerHeight { get; private set; }

    private void RecalculateWidthConstraint(float containerWidth)
    {
        MinWidthValue = _minWidth.CalculateSize(containerWidth);
        MaxWidthValue = _maxWidth.CalculateSize(containerWidth);

        switch (BoxSizing)
        {
            default:
            case BoxSizing.Border:
                MinInnerWidth = MinWidthValue - Border.Horizontal - Padding.Horizontal;
                MaxInnerWidth = MaxWidthValue - Border.Horizontal - Padding.Horizontal;
                MinOuterWidth = MinWidthValue + Margin.Horizontal;
                MaxOuterWidth = MaxWidthValue + Margin.Horizontal;
                break;
            case BoxSizing.Content:
                MinInnerWidth = MinWidthValue;
                MaxInnerWidth = MaxWidthValue;
                MinOuterWidth = MinWidthValue + Border.Horizontal + Padding.Horizontal + Margin.Horizontal;
                MaxOuterWidth = MaxWidthValue + Border.Horizontal + Padding.Horizontal + Margin.Horizontal;
                break;
        }
    }

    private void RecalculateHeightConstraint(float containerHeight)
    {
        MinHeightValue = _minHeight.CalculateSize(containerHeight);
        MaxHeightValue = _maxHeight.CalculateSize(containerHeight);

        switch (BoxSizing)
        {
            default:
            case BoxSizing.Border:
                MinInnerHeight = MinHeightValue - Border.Vertical - Padding.Vertical;
                MaxInnerHeight = MaxHeightValue - Border.Vertical - Padding.Vertical;
                MinOuterHeight = MinHeightValue + Margin.Vertical;
                MaxOuterHeight = MaxHeightValue + Margin.Vertical;
                break;
            case BoxSizing.Content:
                MinInnerHeight = MinHeightValue;
                MaxInnerHeight = MaxHeightValue;
                MinOuterHeight = MinHeightValue + Border.Vertical + Padding.Vertical + Margin.Vertical;
                MaxOuterHeight = MaxHeightValue + Border.Vertical + Padding.Vertical + Margin.Vertical;
                break;
        }
    }

    #endregion

    private void RecalculateBoundsWidth(float containerWidth)
    {
        WidthValue = _width.CalculateSize(containerWidth);
        switch (BoxSizing)
        {
            default:
            case BoxSizing.Border:
                WidthValue = MathHelper.Clamp(_width.CalculateSize(containerWidth), MinWidthValue, MaxWidthValue);
                SetBoundsWidth(WidthValue);
                break;
            case BoxSizing.Content:
                SetInnerBoundsWidth(WidthValue);
                break;
        }
    }

    private void RecalculateBoundsHeight(float containerHeight)
    {
        HeightValue = _height.CalculateSize(containerHeight);
        switch (BoxSizing)
        {
            default:
            case BoxSizing.Border:
                HeightValue = MathHelper.Clamp(_height.CalculateSize(containerHeight), MinHeightValue, MaxHeightValue);
                SetBoundsHeight(HeightValue);
                break;
            case BoxSizing.Content:
                SetInnerBoundsHeight(HeightValue);
                break;
        }
    }

    #region SetBoundsSize

    protected void SetOuterBoundsWidth(float width)
    {
        width = MathHelper.Clamp(width, MinOuterWidth, MaxOuterWidth);

        OuterBounds.Width = Math.Max(0, width);
        Bounds.Width = Math.Max(0, width -= Margin.Horizontal);
        InnerBounds.Width = Math.Max(0, width - Padding.Horizontal - Border.Horizontal);
    }

    protected void SetOuterBoundsHeight(float height)
    {
        height = MathHelper.Clamp(height, MinOuterHeight, MaxOuterHeight);

        OuterBounds.Height = Math.Max(0, height);
        Bounds.Height = Math.Max(0, height -= Margin.Vertical);
        InnerBounds.Height = Math.Max(0, height - Padding.Vertical - Border.Vertical);
    }

    protected void SetBoundsWidth(float width)
    {
        Bounds.Width = Math.Max(0, width);
        InnerBounds.Width = Math.Max(0, width - Padding.Horizontal - Border.Horizontal);
        OuterBounds.Width = Math.Max(0, width + Margin.Horizontal);
    }

    protected void SetBoundsHeight(float height)
    {
        Bounds.Height = Math.Max(0, height);
        InnerBounds.Height = Math.Max(0, height - Padding.Vertical - Border.Vertical);
        OuterBounds.Height = Math.Max(0, height + Margin.Vertical);
    }

    protected void SetInnerBoundsWidth(float width)
    {
        width = MathHelper.Clamp(width, MinInnerWidth, MaxInnerWidth);

        InnerBounds.Width = Math.Max(0, width);
        Bounds.Width = Math.Max(0, width += Padding.Horizontal + Border.Horizontal);
        OuterBounds.Width = Math.Max(0, width + Margin.Horizontal);
    }

    protected void SetInnerBoundsHeight(float height)
    {
        height = MathHelper.Clamp(height, MinInnerHeight, MaxInnerHeight);

        InnerBounds.Height = Math.Max(0, height);
        Bounds.Height = Math.Max(0, height += Padding.Vertical + Border.Vertical);
        OuterBounds.Height = Math.Max(0, height + Margin.Vertical);
    }

    #endregion
}