namespace SilkyUIFramework.BasicElements;

public partial class UIView
{
    #region BoxSizing Margin Padding Border

    public BoxSizing BoxSizing
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
            MarkPositionDirty();
        }
    }

    public Margin Margin
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
            MarkPositionDirty();
        }
    }

    public void SetMargin(float margin) => Padding = new Margin(margin);

    public void SetMargin(float leftAndRight, float topAndBottom) =>
        Margin = new Margin(leftAndRight, topAndBottom, leftAndRight, topAndBottom);

    public Margin Padding
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
            MarkPositionDirty();
        }
    }

    public void SetPadding(float padding) => Padding = padding;

    public void SetPadding(float leftAndRight, float topAndBottom) =>
        Padding = new Margin(leftAndRight, topAndBottom, leftAndRight, topAndBottom);

    public float Border
    {
        get => RectangleRender.Border;
        set
        {
            if (RectangleRender.Border == value) return;
            RectangleRender.Border = value;
            MarkLayoutDirty();
            MarkPositionDirty();
        }
    }

    #endregion

    #region Fit Width Height

    public bool FitWidth
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public bool FitHeight
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    private Dimension _minWidth;
    private Dimension _maxWidth = new(ushort.MaxValue * 100);
    private Dimension _width;

    private Dimension _minHeight;
    private Dimension _maxHeight = new(ushort.MaxValue * 100);
    private Dimension _height;

    public Dimension MinWidth => _minWidth;
    public Dimension MaxWidth => _maxWidth;
    public Dimension Width => _width;
    public Dimension MinHeight => _minHeight;
    public Dimension MaxHeight => _maxHeight;
    public Dimension Height => _height;

    #endregion

    #region SetDimension

    private void SetDimension(ref Dimension dimension, float? pixels = null, float? percent = null)
    {
        if ((pixels.HasValue && dimension.Pixels != pixels.Value) ||
            (percent.HasValue && dimension.Percent != percent.Value))
        {
            dimension = dimension.With(pixels, percent);
            MarkLayoutDirty();
        }
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

    public void SetSize(float? widthPixels = null, float? heightPixels = null, float? widthPercent = null,
        float? heightPercent = null)
    {
        SetWidth(widthPixels, widthPercent);
        SetHeight(heightPixels, heightPercent);
    }

    #endregion

    /// <summary>
    /// 获取可用空间
    /// </summary>
    protected Size GetAvailableSpace() =>
        new(FitWidth ? 0 : InnerBounds.Width, FitHeight ? 0 : InnerBounds.Height);

    /// <summary>
    /// 给自己用的
    /// </summary>
    protected Size GetParentAvailableSpace() =>
        Parent?.GetAvailableSpace() ?? GraphicsDeviceHelper.GetBackBufferSizeByUIScale();

    public virtual void RefreshLayout()
    {
        if (!LayoutIsDirty) return;

        var container = GetParentAvailableSpace();
        Prepare(container.Width, container.Height);
        RecalculateHeight();
        CleanupDirtyMark();
    }

    public virtual void Prepare(float? width, float? height)
    {
        ComputeWidthConstraint(width ?? 0);
        ComputeHeightConstraint(height ?? 0);

        if (FitWidth)
            DefineInnerBoundsWidth(MathHelper.Clamp(0f, MinInnerWidth, MaxInnerWidth));
        else
            RecalculateBoundsWidth(width ?? 0);

        if (FitHeight)
            DefineInnerBoundsHeight(MathHelper.Clamp(0f, MinInnerHeight, MaxInnerHeight));
        else
            RecalculateBoundsHeight(height ?? 0);
    }

    public virtual void RecalculateHeight() { }

    public virtual void RefreshWidth(float containerWidth)
    {
        ComputeWidthConstraint(containerWidth);
        if (!FitWidth)
        {
            RecalculateBoundsWidth(containerWidth);
        }
    }

    public virtual void RefreshHeight(float containerHeight)
    {
        ComputeHeightConstraint(containerHeight);
        if (!FitWidth)
        {
            RecalculateBoundsHeight(containerHeight);
        }
    }

    protected internal virtual void SetExactWidth(float width)
    {
        DefineOuterBoundsWidth(MathHelper.Clamp(width, MinOuterWidth, MaxOuterWidth));
    }

    protected internal virtual void SetExactHeight(float height)
    {
        DefineOuterBoundsHeight(MathHelper.Clamp(height, MinOuterHeight, MaxOuterHeight));
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

    protected internal void ComputeWidthConstraint(float containerWidth)
    {
        switch (BoxSizing)
        {
            default:
            case BoxSizing.Border:
                MinWidthValue = Math.Max(_minWidth.CalculateSize(containerWidth), Padding.Horizontal + Border * 2);
                MaxWidthValue = Math.Max(_maxWidth.CalculateSize(containerWidth), Padding.Horizontal + Border * 2);
                MinInnerWidth = MinWidthValue - Border * 2 - Padding.Horizontal;
                MaxInnerWidth = MaxWidthValue - Border * 2 - Padding.Horizontal;
                MinOuterWidth = MinWidthValue + Margin.Horizontal;
                MaxOuterWidth = MaxWidthValue + Margin.Horizontal;
                break;
            case BoxSizing.Content:
                MinWidthValue = _minWidth.CalculateSize(containerWidth);
                MaxWidthValue = _maxWidth.CalculateSize(containerWidth);
                MinInnerWidth = MinWidthValue;
                MaxInnerWidth = MaxWidthValue;
                MinOuterWidth = MinWidthValue + Border * 2 + Padding.Horizontal + Margin.Horizontal;
                MaxOuterWidth = MaxWidthValue + Border * 2 + Padding.Horizontal + Margin.Horizontal;
                break;
        }
    }

    protected internal void ComputeHeightConstraint(float containerHeight)
    {
        switch (BoxSizing)
        {
            default:
            case BoxSizing.Border:
                MinHeightValue = Math.Max(_minHeight.CalculateSize(containerHeight), Padding.Vertical + Border * 2);
                MaxHeightValue = Math.Max(_maxHeight.CalculateSize(containerHeight), Padding.Vertical + Border * 2);
                MinInnerHeight = MinHeightValue - Border * 2 - Padding.Vertical;
                MaxInnerHeight = MaxHeightValue - Border * 2 - Padding.Vertical;
                MinOuterHeight = MinHeightValue + Margin.Vertical;
                MaxOuterHeight = MaxHeightValue + Margin.Vertical;
                break;
            case BoxSizing.Content:
                MinHeightValue = _minHeight.CalculateSize(containerHeight);
                MaxHeightValue = _maxHeight.CalculateSize(containerHeight);
                MinInnerHeight = MinHeightValue;
                MaxInnerHeight = MaxHeightValue;
                MinOuterHeight = MinHeightValue + Border * 2 + Padding.Vertical + Margin.Vertical;
                MaxOuterHeight = MaxHeightValue + Border * 2 + Padding.Vertical + Margin.Vertical;
                break;
        }
    }

    #endregion

    protected void RecalculateBoundsWidth(float containerWidth)
    {
        WidthValue = _width.CalculateSize(containerWidth);
        WidthValue = MathHelper.Clamp(WidthValue, MinWidthValue, MaxWidthValue);

        switch (BoxSizing)
        {
            default:
            case BoxSizing.Border:
                DefineBoundsWidth(WidthValue);
                break;
            case BoxSizing.Content:
                DefineInnerBoundsWidth(WidthValue);
                break;
        }
    }

    protected void RecalculateBoundsHeight(float containerHeight)
    {
        HeightValue = _height.CalculateSize(containerHeight);
        HeightValue = MathHelper.Clamp(HeightValue, MinHeightValue, MaxHeightValue);

        switch (BoxSizing)
        {
            default:
            case BoxSizing.Border:
                DefineBoundsHeight(HeightValue);
                break;
            case BoxSizing.Content:
                DefineInnerBoundsHeight(HeightValue);
                break;
        }
    }

    #region SetBoundsSize

    protected void DefineOuterBoundsWidth(float width)
    {
        OuterBounds.Width = width;
        Bounds.Width = width -= Margin.Horizontal;
        InnerBounds.Width = width - Padding.Horizontal - Border * 2;
    }

    protected void DefineOuterBoundsHeight(float height)
    {
        OuterBounds.Height = height;
        Bounds.Height = height -= Margin.Vertical;
        InnerBounds.Height = height - Padding.Vertical - Border * 2;
    }

    protected void DefineBoundsWidth(float width)
    {
        Bounds.Width = width;
        InnerBounds.Width = width - Padding.Horizontal - Border * 2;
        OuterBounds.Width = width + Margin.Horizontal;
    }

    protected void DefineBoundsHeight(float height)
    {
        Bounds.Height = height;
        InnerBounds.Height = height - Padding.Vertical - Border * 2;
        OuterBounds.Height = height + Margin.Vertical;
    }

    protected void DefineInnerBoundsWidth(float width)
    {
        InnerBounds.Width = width;
        Bounds.Width = width += Padding.Horizontal + Border * 2;
        OuterBounds.Width = width + Margin.Horizontal;
    }

    protected void DefineInnerBoundsHeight(float height)
    {
        InnerBounds.Height = height;
        Bounds.Height = height += Padding.Vertical + Border * 2;
        OuterBounds.Height = height + Margin.Vertical;
    }

    #endregion
}