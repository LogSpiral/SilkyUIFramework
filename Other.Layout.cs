using Microsoft.CodeAnalysis;
using SilkyUIFramework.Core;

namespace SilkyUIFramework;

public enum LayoutType
{
    Flexbox, Grid, Custom, None,
}

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public partial class Other
{
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

    public Vector2 Gap
    {
        get => _gap;
        set
        {
            if (value == _gap) return;
            _gap = value;
            BubbleMarkerDirty();
        }
    }
    private Vector2 _gap;

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

    public void CalculateBoxSize(Size size, out Size box, out Size outerBox, out Size innerBox)
    {
        switch (BoxSizing)
        {
            default:
            case BoxSizing.BorderBox:
            {
                box = size;
                outerBox = size + Margin;
                innerBox = size - Padding;
                break;
            }
            case BoxSizing.ContentBox:
            {
                innerBox = size;
                box = innerBox + Padding;
                outerBox = box + Margin;
                break;
            }
        }

        box = Size.Max(Size.Zero, box);
        outerBox = Size.Max(Size.Zero, outerBox);
        innerBox = Size.Max(Size.Zero, innerBox);
    }

    /// <summary>
    /// 修剪
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="forcedWidth">父元素强制为子元素设定的宽度 (是否接受取决于子元素)</param>
    /// <param name="forcedHeight">父元素强制为子元素设定的高度 (是否接受取决于子元素)</param>
    /// <returns></returns>
    protected virtual Size Trim(Size space, float? forcedWidth = null, float? forcedHeight = null)
    {
        // 被布局管理的元素, 始终不应该自行调用 Trim 方法.
        // 如果元素使用 绝对定位, 则无所谓.

        // return Size.Zero;

        if (forcedWidth.HasValue)
        {
            _outerBounds.Width = forcedWidth.Value;
            _bounds.Width = forcedWidth.Value - Margin.Width;
            _innerBounds.Width = forcedWidth.Value - Margin.Width - Padding.Width;
        }

        if (forcedHeight.HasValue)
        {
            _outerBounds.Height = forcedHeight.Value;
            _bounds.Height = forcedHeight.Value - Margin.Height;
            _innerBounds.Height = forcedHeight.Value - Margin.Height - Padding.Height;
        }

        // 如果我现在要实现 Flexbox 的根据最大的子元素拉伸其余子元素的行为是不是就可以了
        switch (LayoutDirection)
        {
            // 横向排列
            case LayoutDirection.Row:
            {
                var quantity = _children.Count;
                var sumGap = Gap.X * (quantity - 1);
                var room = _innerBounds.Width - sumGap;

                var sumWidth = _children.Sum(child => child._outerBounds.Width);

                // 子元素占用空间超越了父元素的容积
                // 对子元素宽度进行压缩
                if (true || sumWidth + sumGap > _innerBounds.Width)
                {
                    for (int i = 0; i < _children.Count; i++)
                    {
                        var child = _children[i];
                        child.Trim(_innerBounds.Size, room * child._outerBounds.Width / sumWidth, _innerBounds.Height);
                    }
                }
                // 否则仅改变子元素高度
                else
                {
                    for (int i = 0; i < _children.Count; i++)
                    {
                        var child = _children[i];
                        child.Trim(_innerBounds.Size, forcedHeight: _innerBounds.Height);
                    }
                }
                break;
            }
            default:
            case LayoutDirection.Column:
            {
                var quantity = _children.Count;
                var sumGap = Gap.Y * (quantity - 1);
                var room = _innerBounds.Height - sumGap;

                var sumHeight = _children.Sum(child => child._outerBounds.Height);

                // 子元素占用空间超越了父元素的容积
                // 对子元素高度进行压缩
                if (true ||sumHeight + sumGap > _innerBounds.Height)
                {
                    for (int i = 0; i < _children.Count; i++)
                    {
                        var child = _children[i];
                        child.Trim(_innerBounds.Size, _innerBounds.Width, room * child._outerBounds.Height / sumHeight);
                    }
                }
                // 否则仅改变子元素宽度
                else
                {
                    for (int i = 0; i < _children.Count; i++)
                    {
                        var child = _children[i];
                        child.Trim(_innerBounds.Size, _innerBounds.Width);
                    }
                }
                break;
            }
        }

        return new Size();
    }

    /// <summary>
    /// 测量, 测量期间不会对元素进行任何限制
    /// 测量的大小仅反应元素自身的预期, 并非最终大小
    /// </summary>
    protected virtual Size Measure(Size space)
    {
        var widthValue = WidthIsAuto ? 0f : WidthUnit.GetValue(space.Width);
        var heightValue = HeightIsAuto ? 0f : HeightUnit.GetValue(space.Height);

        if (WidthIsAuto && HeightIsAuto)
        {
            _bounds.Size = Size.Zero;
            _outerBounds.Size = Size.Zero;
            _innerBounds.Size = Size.Zero;
        }
        else
        {
            CalculateBoxSize(new Size(widthValue, heightValue), out var box, out var outerBox, out var innerBox);
            _bounds.Size = box;
            _outerBounds.Size = outerBox;
            _innerBounds.Size = innerBox;
        }

        if (_children.Count < 1) return _outerBounds.Size;

        foreach (var child in _children)
        {
            child.Measure(_innerBounds.Size);
        }

        if (LayoutType == LayoutType.Flexbox)
        {
            switch (LayoutDirection)
            {
                // 这段是错的，有空修
                case LayoutDirection.Row:
                {
                    if (WidthIsAuto)
                    {
                        widthValue = _children.Sum(child => child.OuterBounds.Width) + (_children.Count - 1) * Gap.X;
                        _innerBounds.Width = widthValue;
                        _bounds.Width = widthValue + Padding.Width;
                        _outerBounds.Width = widthValue + Padding.Width + Margin.Width;
                    }
                    if (HeightIsAuto)
                    {
                        heightValue = _children.Max(child => child.OuterBounds.Height);
                        _innerBounds.Height = heightValue;
                        _bounds.Height = heightValue + Padding.Height;
                        _outerBounds.Height = heightValue + Padding.Height + Margin.Height;
                    }
                    break;
                }
                default:
                case LayoutDirection.Column:
                {
                    if (WidthIsAuto)
                    {
                        widthValue = _children.Max(child => child.OuterBounds.Width);
                        _innerBounds.Width = widthValue;
                        _bounds.Width = widthValue + Padding.Width;
                        _outerBounds.Width = widthValue + Padding.Width + Margin.Width;
                    }
                    if (HeightIsAuto)
                    {
                        heightValue = _children.Sum(child => child.OuterBounds.Height) + (_children.Count - 1) * Gap.Y;
                        _innerBounds.Height = heightValue;
                        _bounds.Height = heightValue + Padding.Height;
                        _outerBounds.Height = heightValue + Padding.Height + Margin.Height;
                    }
                    break;
                }
            }
        }

        return _outerBounds.Size;
    }

    public readonly Margin Border = new();
    protected void Arrange()
    {
        switch (LayoutDirection)
        {
            // 行
            case LayoutDirection.Row:
            {
                float leftOffset = 0f;
                for (int i = 0; i < _children.Count; i++)
                {
                    _children[i].LayoutOffset = new Vector2(leftOffset, 0);
                    _children[i].Arrange();
                    leftOffset += Gap.X;
                    leftOffset += _children[i]._outerBounds.Width;
                }
                break;
            }
            // 列
            default:
            case LayoutDirection.Column:
            {
                float topOffset = 0f;
                for (int i = 0; i < _children.Count; i++)
                {
                    _children[i].LayoutOffset = new Vector2(0, topOffset);
                    _children[i].Arrange();
                    topOffset += Gap.Y;
                    topOffset += _children[i]._outerBounds.Height;
                }
                break;
            }
        }

        IsLayoutDirty = false;
    }
}