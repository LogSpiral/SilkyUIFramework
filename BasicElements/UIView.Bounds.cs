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

    public void SetMargin(float margin) => Margin = new Margin(margin);

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

    public Dimension Width
    {
        get => _width;
        set { SetWidth(value.Pixels, value.Percent); }
    }

    public Dimension MinWidth
    {
        get => _minWidth;
        set { SetMinWidth(value.Pixels, value.Percent); }
    }

    public Dimension MaxWidth
    {
        get => _maxWidth;
        set { SetMaxWidth(value.Pixels, value.Percent); }
    }

    public Dimension Height
    {
        get => _height;
        set { SetHeight(value.Pixels, value.Percent); }
    }

    public Dimension MinHeight
    {
        get => _minHeight;
        set { SetMinHeight(value.Pixels, value.Percent); }
    }

    public Dimension MaxHeight
    {
        get => _maxHeight;
        set { SetMaxHeight(value.Pixels, value.Percent); }
    }

    #endregion

    /// <summary>
    /// 获取可用空间
    /// </summary>
    protected Size GetInnerSpace() =>
        new(FitWidth ? 0 : InnerBounds.Width, FitHeight ? 0 : InnerBounds.Height);

    /// <summary>
    /// 给自己用的
    /// </summary>
    protected Size GetParentInnerSpace() =>
        Parent?.GetInnerSpace() ?? GraphicsDeviceHelper.GetBackBufferSizeByUIScale();

    public virtual void UpdateLayout()
    {
        if (!LayoutIsDirty) return;

        var container = GetParentInnerSpace();
        Prepare(container.Width, container.Height);
        RecalculateWidth();
        RecalculateHeight();
        CleanupDirtyMark();
    }

    public virtual void Prepare(float? width, float? height)
    {
        CalculateWidthConstraints(width ?? 0);
        CalculateHeightConstraints(height ?? 0);

        if (FitWidth)
            SetInnerBoundsWidth(MathHelper.Clamp(0f, MinInnerWidth, MaxInnerWidth));
        else
            CalculateBoundsWidth(width ?? 0);

        if (FitHeight)
            SetInnerBoundsHeight(MathHelper.Clamp(0f, MinInnerHeight, MaxInnerHeight));
        else
            CalculateBoundsHeight(height ?? 0);
    }

    public virtual void RefreshWidth(float containerWidth)
    {
        CalculateWidthConstraints(containerWidth);

        if (FitWidth) return;
        CalculateBoundsWidth(containerWidth);
    }

    public virtual void RecalculateWidth() { }

    public virtual void RecalculateHeight() { }

    public virtual void RefreshHeight(float containerHeight)
    {
        CalculateHeightConstraints(containerHeight);

        if (FitHeight) return;
        CalculateBoundsHeight(containerHeight);
    }

    #region 声明和计算约束

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

    protected internal void CalculateWidthConstraints(float containerWidth)
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

    protected internal void CalculateHeightConstraints(float containerHeight)
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

    protected void CalculateBoundsWidth(float containerWidth)
    {
        WidthValue = _width.CalculateSize(containerWidth);
        WidthValue = MathHelper.Clamp(WidthValue, MinWidthValue, MaxWidthValue);

        switch (BoxSizing)
        {
            case BoxSizing.Border:
                SetBoundsWidth(WidthValue);
                break;
            case BoxSizing.Content:
                SetInnerBoundsWidth(WidthValue);
                break;
            default: goto case BoxSizing.Border;
        }
    }

    protected void CalculateBoundsHeight(float containerHeight)
    {
        HeightValue = _height.CalculateSize(containerHeight);
        HeightValue = MathHelper.Clamp(HeightValue, MinHeightValue, MaxHeightValue);

        switch (BoxSizing)
        {
            case BoxSizing.Border:
                SetBoundsHeight(HeightValue);
                break;
            case BoxSizing.Content:
                SetInnerBoundsHeight(HeightValue);
                break;
            default: goto case BoxSizing.Border;
        }
    }

    #region 设置 Bounds 的方法，包括 OuterBounds, Bounds, InnerBounds

    protected internal void SetOuterBoundsWidth(float width)
    {
        OuterBounds.Width = width;
        Bounds.Width = width -= Margin.Horizontal;
        InnerBounds.Width = width - Padding.Horizontal - Border * 2;
    }

    protected internal void SetOuterBoundsHeight(float height)
    {
        OuterBounds.Height = height;
        Bounds.Height = height -= Margin.Vertical;
        InnerBounds.Height = height - Padding.Vertical - Border * 2;
    }

    protected internal void SetBoundsWidth(float width)
    {
        Bounds.Width = width;
        InnerBounds.Width = width - Padding.Horizontal - Border * 2;
        OuterBounds.Width = width + Margin.Horizontal;
    }

    protected internal void SetBoundsHeight(float height)
    {
        Bounds.Height = height;
        InnerBounds.Height = height - Padding.Vertical - Border * 2;
        OuterBounds.Height = height + Margin.Vertical;
    }

    protected internal void SetInnerBoundsWidth(float width)
    {
        InnerBounds.Width = width;
        Bounds.Width = width += Padding.Horizontal + Border * 2;
        OuterBounds.Width = width + Margin.Horizontal;
    }

    protected internal void SetInnerBoundsHeight(float height)
    {
        InnerBounds.Height = height;
        Bounds.Height = height += Padding.Vertical + Border * 2;
        OuterBounds.Height = height + Margin.Vertical;
    }

    #endregion
}