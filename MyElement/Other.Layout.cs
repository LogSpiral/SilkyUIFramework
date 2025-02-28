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
            MarkLayoutDirty();
        }
    }
    private LayoutType _layoutType;

    public void AutoWidth(bool auto = true) => WidthIsAuto = auto;
    public void AutoHeight(bool auto = true) => HeightIsAuto = auto;
    public void AutoSize(bool autoWidth = true, bool autoHeight = true)
    {
        if (autoWidth == WidthIsAuto && autoHeight == HeightIsAuto) return;

        _widthIsAuto = autoWidth;
        _heightIsAuto = autoHeight;
        MarkLayoutDirty();
    }

    public bool WidthIsAuto
    {
        get => _widthIsAuto;
        set
        {
            if (value == _widthIsAuto) return;
            _widthIsAuto = value;
            MarkLayoutDirty();
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
            MarkLayoutDirty();
        }
    }
    private bool _heightIsAuto;

    public void SetMinSize(float minWidthPixel, float minHeightPixel, float? minWidthPercent = null, float? minHeightPercent = null)
    {
        if (minWidthPixel == _minWidthUnit.Pixels && (!minWidthPercent.HasValue || minWidthPercent.Value == _minWidthUnit.Percent) &&
            minHeightPixel == _minHeightUnit.Pixels && (!minHeightPercent.HasValue || minHeightPercent.Value == _minHeightUnit.Percent))
        {
            return;
        }

        _minWidthUnit.Set(minWidthPixel, minWidthPercent);
        _minHeightUnit.Set(minHeightPixel, minHeightPercent);
        MarkLayoutDirty();
    }

    public void SetMaxSize(float maxWidthPixel, float maxHeightPixel, float? maxWidthPercent = null, float? maxHeightPercent = null)
    {
        if (maxWidthPixel == _maxWidthUnit.Pixels && (!maxWidthPercent.HasValue || maxWidthPercent.Value == _maxWidthUnit.Percent) &&
            maxHeightPixel == _maxHeightUnit.Pixels && (!maxHeightPercent.HasValue || maxHeightPercent.Value == _maxHeightUnit.Percent))
        {
            return;
        }

        _maxWidthUnit.Set(maxWidthPixel, maxWidthPercent);
        _maxHeightUnit.Set(maxHeightPixel, maxHeightPercent);
        MarkLayoutDirty();
    }

    public void SetSize(float widthPixel, float heightPixel, float? widthPercent = null, float? heightPercent = null)
    {
        if (widthPixel == _widthUnit.Pixels && (!widthPercent.HasValue || widthPercent.Value == _widthUnit.Percent) &&
            heightPixel == _heightUnit.Pixels && (!heightPercent.HasValue || heightPercent.Value == _heightUnit.Percent))
        {
            return;
        }

        _widthUnit.Set(widthPixel, widthPercent);
        _heightUnit.Set(heightPixel, heightPercent);
        MarkLayoutDirty();
    }

    public void SetWidth(float pixel, float? percent = null)
    {
        if (pixel == _widthUnit.Pixels && (!percent.HasValue || percent.Value == _widthUnit.Percent))
        {
            return;
        }

        _widthUnit.Set(pixel, percent);
        MarkLayoutDirty();
    }

    public void SetHeight(float pixel, float? percent = null)
    {
        if (pixel == _heightUnit.Pixels && (!percent.HasValue || percent.Value == _heightUnit.Percent))
        {
            return;
        }

        _heightUnit.Set(pixel, percent);
        MarkLayoutDirty();
    }

    public void SetLeft(float pixel, float? percent = null)
    {
        if (pixel == _leftUnit.Pixels && (!percent.HasValue || percent.Value == _leftUnit.Percent))
        {
            return;
        }

        _leftUnit.Set(pixel, percent);
        PositionDirty = true;
    }

    public void SetTop(float pixel, float? percent = null)
    {
        if (pixel == _topUnit.Pixels && (!percent.HasValue || percent.Value == _topUnit.Percent))
        {
            return;
        }

        _topUnit.Set(pixel, percent);
        PositionDirty = true;
    }

    public Size Gap
    {
        get => _gap;
        set
        {
            if (value == _gap) return;
            _gap = value;
            MarkLayoutDirty();
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
            MarkLayoutDirty();
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
            MarkLayoutDirty();
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
            MarkLayoutDirty();
            PositionDirty = true;
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
            MarkLayoutDirty();
            PositionDirty = true;
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
            MarkLayoutDirty();
            PositionDirty = true;
        }
    }
    private Margin _padding;

    #endregion

    /// <summary>
    /// 修剪
    /// </summary>
    /// <param name="container">父元素分配的空间</param>
    /// <param name="assignWidth">父元素直接分配宽度</param>
    /// <param name="assignHeight">父元素直接分配高度</param>
    /// <returns></returns>
    protected virtual Size Trim(Size container, float? assignWidth = null, float? assignHeight = null)
    {

        // 暂且让 assign 优先级大于 min max (因为考虑 min max 写起来麻烦)
        #region AssignSize

        if (_containerForMeasure != container)
            UpdateConstraints(container);

        if (assignWidth.HasValue)
        {
            SetOuterBoundsWidth(assignWidth.Value);
        }
        else if (!WidthIsAuto) UpdateBoundsWidth(container.Width);

        if (assignHeight.HasValue)
        {
            SetOuterBoundsHeight(assignHeight.Value);
        }
        else if (!HeightIsAuto) UpdateBoundsHeight(container.Height);

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
                if (true || sumWidth > space)
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
                if (true || sumHeight > space)
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

    protected Size LastContainer;

    /// <summary>
    /// 测量: 测量期间不会对元素进行任何限制
    /// 测量结果: 仅反应元素自身的预期, 并非最终大小
    /// </summary>
    protected virtual Size Measure(Size container)
    {
        Classify(); // 分类

        #region Set Bounds

        UpdateConstraints(container);
        _containerForMeasure = container;

        if (!WidthIsAuto) UpdateBoundsWidth(container.Width);
        if (!HeightIsAuto) UpdateBoundsHeight(container.Height);

        if (LayoutElements.Count > 0)
        {
            var subcontainer = new Size(WidthIsAuto ? 0 : _innerBounds.Width, HeightIsAuto ? 0 : _innerBounds.Height);
            foreach (var child in LayoutElements) // 布局元素
            {
                child.Measure(subcontainer);
            }
        }

        #endregion

        if (LayoutElements.Count > 0)
        {
            FlexboxMeasureContent();
        }

        LastContainer = container;
        return _outerBounds.Size;
    }

    protected void FlexboxMeasureContent()
    {
        if (LayoutType is not LayoutType.Flexbox) return;

        switch (LayoutDirection)
        {
            case LayoutDirection.Row:
                if (WidthIsAuto)
                {
                    SetBoundsWidth(LayoutElements.Sum(e => e._outerBounds.Width) + (LayoutElements.Count - 1) * Gap.Width);
                }
                if (HeightIsAuto)
                {
                    SetBoundsHeight(LayoutElements.Max(e => e._outerBounds.Height));
                }
                break;
            default:
            case LayoutDirection.Column:
                if (WidthIsAuto)
                {
                    SetBoundsWidth(LayoutElements.Max(e => e._outerBounds.Width));
                }
                if (HeightIsAuto)
                {
                    SetBoundsHeight(LayoutElements.Sum(e => e._outerBounds.Height) + (LayoutElements.Count - 1) * Gap.Height);
                }
                break;
        }
    }

    #region SetBoundsSize

    protected void SetOuterBoundsWidth(float width)
    {
        _outerBounds.Width = Math.Max(0, width);
        _bounds.Width = Math.Max(0, width - Margin.Width);
        _innerBounds.Width = Math.Max(0, _bounds.Width - Padding.Width - Border.Width);
    }

    protected void SetOuterBoundsHeight(float height)
    {
        _outerBounds.Height = Math.Max(0, height);
        _bounds.Height = Math.Max(0, height - Margin.Height);
        _innerBounds.Height = Math.Max(0, _bounds.Height - Padding.Height - Border.Height);
    }

    protected void SetBoundsWidth(float width)
    {
        _innerBounds.Width = Math.Max(0, width);
        _bounds.Width = Math.Max(0, width + Padding.Width);
        _outerBounds.Width = Math.Max(0, width + Padding.Width + Margin.Width);
    }

    protected void SetBoundsHeight(float height)
    {
        _innerBounds.Height = Math.Max(0, height);
        _bounds.Height = Math.Max(0, height + Padding.Height);
        _outerBounds.Height = Math.Max(0, height + Padding.Height + Margin.Height);
    }

    protected void SetInnerBoundsWidth(float width)
    {
        _innerBounds.Width = Math.Max(0, width);
        _bounds.Width = Math.Max(0, width + Padding.Width);
        _outerBounds.Width = Math.Max(0, width + Padding.Width + Margin.Width);
    }

    protected void SetInnerBoundsHeight(float height)
    {
        _innerBounds.Height = Math.Max(0, height);
        _bounds.Height = Math.Max(0, height + Padding.Height);
        _outerBounds.Height = Math.Max(0, height + Padding.Height + Margin.Height);
    }

    #endregion

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
                    layoutElement.SetLayoutPosition(leftOffset, 0);
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
                    layoutElement.SetLayoutPosition(0, topOffset);
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

        LayoutDirty = false;
    }
}