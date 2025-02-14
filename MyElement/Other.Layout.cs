using Microsoft.CodeAnalysis;
using SilkyUIFramework.Core;

namespace SilkyUIFramework;

public enum LayoutType
{
    Flexbox, Custom, None,
}

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public partial class Other
{
    #region Properties Fields

    public LayoutType LayoutType
    {
        get => _layoutType;
        set
        {
            if (value == _layoutType) return;
            _layoutType = value;
            BubbleMarkerDirty();
        }
    }
    private LayoutType _layoutType;

    public void AutoWidth(bool auto = true) => WidthIsAuto = auto;
    public void AutoHeight(bool auto = true) => HeightIsAuto = auto;
    public void AutoSize(bool auto = true)
    {
        _widthIsAuto = auto;
        _heightIsAuto = auto;
        BubbleMarkerDirty();
    }

    public bool WidthIsAuto
    {
        get => _widthIsAuto;
        set
        {
            if (value == _widthIsAuto) return;
            _widthIsAuto = value;
            BubbleMarkerDirty();
        }
    }
    private bool _widthIsAuto;
    public bool HeightIsAuto
    {
        get => _heightIsAuto;
        set
        {
            if (value == _heightIsAuto) return;
            _heightIsAuto = value;
            BubbleMarkerDirty();
        }
    }
    private bool _heightIsAuto;

    public void SetSize(float widthPixel, float heightPixel, float? widthPercent = null, float? heightPercent = null)
    {
        if (widthPixel == _widthUnit.Pixels && (!widthPercent.HasValue || widthPercent.Value == _widthUnit.Percent) &&
            heightPixel == _heightUnit.Pixels && (!heightPercent.HasValue || heightPercent.Value == _heightUnit.Percent))
        {
            return;
        }

        _widthUnit.Set(widthPixel, widthPercent);
        _heightUnit.Set(heightPixel, heightPercent);
        BubbleMarkerDirty();
    }

    public void SetWidth(float pixel, float? percent = null)
    {
        if (pixel == _widthUnit.Pixels && (!percent.HasValue || percent.Value == _widthUnit.Percent))
        {
            return;
        }

        _widthUnit.Set(pixel, percent);
        BubbleMarkerDirty();
    }

    public void SetHeight(float pixel, float? percent = null)
    {
        if (pixel == _heightUnit.Pixels && (!percent.HasValue || percent.Value == _heightUnit.Percent))
        {
            return;
        }

        _heightUnit.Set(pixel, percent);
        BubbleMarkerDirty();
    }

    public void SetLeft(float pixel, float? percent = null)
    {
        if (pixel == _leftUnit.Pixels && (!percent.HasValue || percent.Value == _leftUnit.Percent))
        {
            return;
        }

        _leftUnit.Set(pixel, percent);
        IsPositionDirty = true;
    }

    public void SetTop(float pixel, float? percent = null)
    {
        if (pixel == _topUnit.Pixels && (!percent.HasValue || percent.Value == _topUnit.Percent))
        {
            return;
        }

        _topUnit.Set(pixel, percent);
        IsPositionDirty = true;
    }

    public Unit WidthUnit
    {
        get => _widthUnit;
        set
        {
            if (value == _widthUnit) return;
            _widthUnit = value;
            BubbleMarkerDirty();
        }
    }
    private Unit _widthUnit;

    public Unit HeightUnit
    {
        get => _heightUnit;
        set
        {
            if (value == _heightUnit) return;
            _heightUnit = value;
            BubbleMarkerDirty();
        }
    }
    private Unit _heightUnit;

    public Unit MinHeightUnit
    {
        get => _minHeightUnit;
        set
        {
            if (value == _minHeightUnit) return;
            _minHeightUnit = value;
            BubbleMarkerDirty();
        }
    }
    private Unit _minHeightUnit;

    public Unit MaxHeightUnit
    {
        get => _maxHeightUnit;
        set
        {
            if (value == _maxHeightUnit) return;
            _maxHeightUnit = value;
            BubbleMarkerDirty();
        }
    }
    private Unit _maxHeightUnit;

    public Size Gap
    {
        get => _gap;
        set
        {
            if (value == _gap) return;
            _gap = value;
            BubbleMarkerDirty();
        }
    }
    private Size _gap;

    public LayoutDirection LayoutDirection
    {
        get => _layoutDirection;
        set
        {
            if (value == _layoutDirection) return;
            _layoutDirection = value;
            BubbleMarkerDirty();
        }
    }
    private LayoutDirection _layoutDirection;

    public BoxSizing BoxSizing
    {
        get => _boxSizing;
        set
        {
            if (value == _boxSizing) return;
            _boxSizing = value;
            BubbleMarkerDirty();
        }
    }
    private BoxSizing _boxSizing;

    public Margin Margin
    {
        get => _margin;
        set
        {
            if (value == _margin) return;
            _margin = value;
            BubbleMarkerDirty();
            IsPositionDirty = true;
        }
    }
    private Margin _margin;

    public Margin Border
    {
        get => _border;
        set
        {
            if (value == _border) return;
            _border = value;
            BubbleMarkerDirty();
            IsPositionDirty = true;
        }
    }
    private Margin _border;

    public Margin Padding
    {
        get => _padding;
        set
        {
            if (value == _padding) return;
            _padding = value;
            BubbleMarkerDirty();
            IsPositionDirty = true;
        }
    }
    private Margin _padding;

    #endregion

    public MainAlignment MainAlignment
    {
        get => _mainAlignment;
        set
        {
            if (value == _mainAlignment) return;
            _mainAlignment = value;
            BubbleMarkerDirty();
        }
    }
    private MainAlignment _mainAlignment;

    public CrossAlignment CrossAlignment
    {
        get => _crossAlignment;
        set
        {
            if (value == _crossAlignment) return;
            _crossAlignment = value;
            BubbleMarkerDirty();
        }
    }
    private CrossAlignment _crossAlignment;

    /// <summary>
    /// 修剪
    /// </summary>
    /// <param name="container">父元素分配的空间</param>
    /// <param name="assignWidth">父元素直接分配宽度</param>
    /// <param name="assignHeight">父元素直接分配高度</param>
    /// <returns></returns>
    protected virtual Size Trim(Size container, float? assignWidth = null, float? assignHeight = null)
    {
        // 被布局管理的元素, 始终不应该自行调用 Trim 方法.
        // 如果元素使用 绝对定位, 则无所谓.

        #region  AssignSize

        if (assignWidth.HasValue)
        {
            SetOuterBoundsWidth(assignWidth.Value);
        }
        // else if (WidthUnit.Percent != 0 && container.Width != ContainerForMeasur.Width)
        // {
        //     switch (BoxSizing)
        //     {
        //         default:
        //         case BoxSizing.BorderBox:
        //             SetBoundsWidth(WidthUnit.GetValue(container.Width));
        //             break;
        //         case BoxSizing.ContentBox:
        //             SetInnerBoundsWidth(WidthUnit.GetValue(container.Width));
        //             break;
        //     }
        // }

        if (assignHeight.HasValue)
        {
            SetOuterBoundsHeight(assignHeight.Value);
        }
        // else if (HeightUnit.Percent != 0 && container.Height != ContainerForMeasur.Height)
        // {
        //     switch (BoxSizing)
        //     {
        //         default:
        //         case BoxSizing.BorderBox:
        //             SetBoundsHeight(HeightUnit.GetValue(container.Height));
        //             break;
        //         case BoxSizing.ContentBox:
        //             SetInnerBoundsHeight(HeightUnit.GetValue(container.Height));
        //             break;
        //     }
        // }

        #endregion

        // 如果我现在要实现 Flexbox 的根据最大的子元素拉伸其余子元素的行为是不是就可以了
        switch (LayoutDirection)
        {
            // 横向排列
            case LayoutDirection.Row:
            {
                var space = _innerBounds.Width - Gap.Width * (LayoutElements.Count - 1);
                var sumWidth = LayoutElements.Sum(child => child._outerBounds.Width);

                // 元素溢出父元素，对子元素宽度进行压缩
                if (sumWidth > space)
                {
                    var each = space / sumWidth;
                    foreach (var child in LayoutElements)
                    {
                        child.Trim(_innerBounds.Size, each * child._outerBounds.Width, _innerBounds.Height);
                    }
                }
                // 否则仅改变子元素高度
                else
                {
                    foreach (var child in LayoutElements)
                    {
                        child.Trim(_innerBounds.Size, null, _innerBounds.Height);
                    }
                }
                break;
            }
            default:
            case LayoutDirection.Column:
            {
                var space = _innerBounds.Height - Gap.Height * (LayoutElements.Count - 1);
                var sumHeight = LayoutElements.Sum(child => child._outerBounds.Height);

                // 元素溢出父元素，对子元素高度进行压缩
                if (sumHeight > space)
                {
                    var each = space / sumHeight;
                    foreach (var child in LayoutElements)
                    {
                        child.Trim(_innerBounds.Size, _innerBounds.Width, each * child._outerBounds.Height);
                    }
                }
                // 否则仅改变子元素宽度
                else
                {
                    foreach (var child in LayoutElements)
                    {
                        child.Trim(_innerBounds.Size, _innerBounds.Width);
                    }
                }
                break;
            }
        }

        // FlexboxMeasure();

        foreach (var child in FreeElements)
        {
            child.Trim(_innerBounds.Size);
        }

        return _outerBounds.Size;
    }

    protected Size ContainerForMeasur;

    /// <summary>
    /// 测量: 测量期间不会对元素进行任何限制
    /// 测量结果: 仅反应元素自身的预期, 并非最终大小
    /// </summary>
    protected virtual Size Measure(Size container)
    {
        Classify(); // 分类

        #region Set Bounds

        if (!WidthIsAuto)
        {
            switch (BoxSizing)
            {
                default:
                case BoxSizing.BorderBox:
                    SetBoundsWidth(WidthUnit.GetValue(container.Width));
                    break;
                case BoxSizing.ContentBox:
                    SetInnerBoundsWidth(WidthUnit.GetValue(container.Width));
                    break;
            }
        }

        if (!HeightIsAuto)
        {
            switch (BoxSizing)
            {
                default:
                case BoxSizing.BorderBox:
                    SetBoundsHeight(HeightUnit.GetValue(container.Height));
                    break;
                case BoxSizing.ContentBox:
                    SetInnerBoundsHeight(HeightUnit.GetValue(container.Height));
                    break;
            }
        }

        if (LayoutElements.Count > 0)
        {
            var subcontainer = new Size(WidthIsAuto ? 0 : _innerBounds.Width, HeightIsAuto ? 0 : _innerBounds.Height);
            foreach (var child in LayoutElements) // 布局元素
            {
                child.Measure(subcontainer);
            }
        }
        else
        {
            switch (BoxSizing)
            {
                default:
                case BoxSizing.BorderBox:
                    SetBoundsHeight(HeightUnit.GetValue(container.Height));
                    break;
                case BoxSizing.ContentBox:
                    SetInnerBoundsHeight(HeightUnit.GetValue(container.Height));
                    break;
            }
        }

        #endregion

        if (LayoutElements.Count < 1) goto end;

        // 无需测量 绝对定位元素, 因为它们不会影响父元素大小
        FlexboxMeasure();

    end:
        ContainerForMeasur = container;
        return _outerBounds.Size;
    }

    protected void FlexboxMeasure()
    {
        if (LayoutType is not LayoutType.Flexbox) return;

        switch (LayoutDirection)
        {
            case LayoutDirection.Row:
                if (WidthIsAuto) SetBoundsWidth(LayoutElements.Sum(e => e._outerBounds.Width) + (LayoutElements.Count - 1) * Gap.Width);
                if (HeightIsAuto) SetBoundsHeight(LayoutElements.Max(e => e._outerBounds.Height));
                break;
            default:
            case LayoutDirection.Column:
                if (WidthIsAuto) SetBoundsWidth(LayoutElements.Max(e => e._outerBounds.Width));
                if (HeightIsAuto) SetBoundsHeight(LayoutElements.Sum(e => e._outerBounds.Height) + (LayoutElements.Count - 1) * Gap.Height);
                break;
        }
    }

    #region SetBoundsSize

    protected void SetOuterBoundsWidth(float width)
    {
        _outerBounds.Width = width;
        _bounds.Width = width - Margin.Width;
        _innerBounds.Width = _bounds.Width - Padding.Width - Border.Width;
    }

    protected void SetOuterBoundsHeight(float height)
    {
        _outerBounds.Height = height;
        _bounds.Height = height - Margin.Height;
        _innerBounds.Height = _bounds.Height - Padding.Height - Border.Height;
    }

    protected void SetBoundsWidth(float width)
    {
        _innerBounds.Width = width;
        _bounds.Width = width + Padding.Width;
        _outerBounds.Width = width + Padding.Width + Margin.Width;
    }

    protected void SetBoundsHeight(float height)
    {
        _innerBounds.Height = height;
        _bounds.Height = height + Padding.Height;
        _outerBounds.Height = height + Padding.Height + Margin.Height;
    }

    protected void SetInnerBoundsWidth(float width)
    {
        _innerBounds.Width = width;
        _bounds.Width = width + Padding.Width;
        _outerBounds.Width = width + Padding.Width + Margin.Width;
    }

    protected void SetInnerBoundsHeight(float height)
    {
        _innerBounds.Height = height;
        _bounds.Height = height + Padding.Height;
        _outerBounds.Height = height + Padding.Height + Margin.Height;
    }

    #endregion

    protected virtual void CalculateSizeWhenAuto()
    {
        // 不换行情况
        switch (LayoutDirection)
        {
            case LayoutDirection.Row:
            {
                if (WidthIsAuto) SetBoundsWidth(LayoutElements.Sum(child => child._outerBounds.Width) + (LayoutElements.Count - 1) * Gap.Width);
                if (HeightIsAuto) SetBoundsHeight(LayoutElements.Max(child => child._outerBounds.Height));
                break;
            }
            default:
            case LayoutDirection.Column:
            {
                if (WidthIsAuto) SetBoundsWidth(LayoutElements.Max(child => child._outerBounds.Width));
                if (HeightIsAuto) SetBoundsHeight(LayoutElements.Sum(child => child._outerBounds.Height) + (LayoutElements.Count - 1) * Gap.Height);
                break;
            }
        }
    }

    protected void Arrange()
    {
        switch (LayoutDirection)
        {
            // 行
            case LayoutDirection.Row:
            {
                float leftOffset = 0f;
                foreach (Other layoutElement in LayoutElements)
                {
                    layoutElement.SetPositionInLayout(leftOffset, 0);
                    leftOffset += Gap.Width;
                    leftOffset += layoutElement._outerBounds.Width;
                }
                break;
            }
            // 列
            default:
            case LayoutDirection.Column:
            {
                float topOffset = 0f;
                foreach (Other layoutElement in LayoutElements)
                {
                    layoutElement.SetPositionInLayout(0, topOffset);
                    topOffset += Gap.Height;
                    topOffset += layoutElement._outerBounds.Height;
                }
                break;
            }
        }

        foreach (var child in _children)
        {
            child.Arrange();
        }

        IsLayoutDirty = false;
    }
}