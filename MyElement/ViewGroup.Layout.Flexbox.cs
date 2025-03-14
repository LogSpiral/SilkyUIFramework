using SilkyUIFramework.MyElement;

namespace SilkyUIFramework;

public partial class ViewGroup
{
    public bool FlexWrap
    {
        get => _flexWrap;
        set
        {
            if (value == _flexWrap) return;
            _flexWrap = value;
            MarkDirty();
        }
    }

    private bool _flexWrap;

    /// <summary>
    /// 如果主轴方向上容器大小为自适应, 则不换行
    /// </summary>
    public bool MustWrap => LayoutDirection == LayoutDirection.Column ? !AutomaticHeight : !AutomaticWidth;

    public FlexDirection FlexDirection
    {
        get => _flexDirection;
        set
        {
            if (value == _flexDirection) return;
            _flexDirection = value;
            MarkDirty();
        }
    }

    private FlexDirection _flexDirection;

    public MainAlignment MainAlignment
    {
        get => _mainAlignment;
        set
        {
            if (value == _mainAlignment) return;
            _mainAlignment = value;
            MarkDirty();
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
            MarkDirty();
        }
    }

    private CrossAlignment _crossAlignment;

    public CrossContentAlignment CrossContentAlignment
    {
        get => _crossContentAlignment;
        set
        {
            if (value == _crossContentAlignment) return;
            _crossContentAlignment = value;
            MarkDirty();
        }
    }

    private CrossContentAlignment _crossContentAlignment;

    private readonly List<FlexboxAxis> _flexboxAxes = [];

    /// <summary>
    /// 对子元素进行换行
    /// </summary>
    private void WrapChildren()
    {
        _flexboxAxes.Clear();
        if (LayoutElements.Count <= 0) return;

        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                WrapElements((FlexWrap && MustWrap) ? InnerBounds.Width : MaxInnerWidth, Gap.Width,
                    [.. LayoutElements.Select(el =>
                    {
                        var bounds = el.OuterBounds;
                        return (el, bounds.Width, bounds.Height);
                    })]);

                return;
            }
            case LayoutDirection.Column:
            {
                WrapElements((FlexWrap && MustWrap) ? InnerBounds.Height : MaxInnerHeight, Gap.Height,
                    [.. LayoutElements.Select(el =>
                    {
                        var bounds = el.OuterBounds;
                        return (el, bounds.Height, bounds.Width);
                    })]);

                return;
            }
        }
    }

    private void WrapElements(float maxSize, float gap,
        (UIView Element, float MainAxisSize, float CrossAxisSize)[] items)
    {
        var flexboxAxis = new FlexboxAxis(items[0].Element)
        {
            MainAxisSize = items[0].MainAxisSize,
            CrossAxisSize = items[0].CrossAxisSize
        };

        for (var i = 1; i < items.Length; i++)
        {
            var (element, mainAxisSize, crossAxisSize) = items[i];
            flexboxAxis.MainAxisSize += mainAxisSize + gap;
            flexboxAxis.CrossAxisSize = MathHelper.Max(flexboxAxis.CrossAxisSize, crossAxisSize);

            if (flexboxAxis.MainAxisSize > maxSize)
            {
                _flexboxAxes.Add(flexboxAxis);
                flexboxAxis = new FlexboxAxis(element)
                {
                    MainAxisSize = mainAxisSize,
                    CrossAxisSize = crossAxisSize
                };
                continue;
            }

            flexboxAxis.Elements.Add(element);
        }

        if (flexboxAxis.Elements.Count > 0) _flexboxAxes.Add(flexboxAxis);
    }

    private Size MeasureAxes()
    {
        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                var width = 0f;
                var height = 0f;

                foreach (var axis in _flexboxAxes)
                {
                    width = Math.Max(width, axis.MainAxisSize);
                    height += axis.CrossAxisSize;
                }

                height += (_flexboxAxes.Count - 1) * Gap.Height;

                return new Size(width, height);
            }
            case LayoutDirection.Column:
            {
                var width = 0f;
                var height = 0f;

                foreach (var axis in _flexboxAxes)
                {
                    width += axis.CrossAxisSize;
                    height = Math.Max(height, axis.MainAxisSize);
                }

                width += (_flexboxAxes.Count - 1) * Gap.Width;

                return new Size(width, height);
            }
        }
    }

    /// <summary>
    /// 按照 <see cref="LayoutDirection"/> 计算一组 View 元素共同占用的大小
    /// </summary>
    private Size FlexboxMeasure(List<UIView> elements)
    {
        if (elements.Count <= 0) return Size.Zero;

        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                var width = 0f;
                var height = 0f;
                foreach (var size in elements.Select(el => el.OuterBounds.Size))
                {
                    width += size.Width;
                    height = Math.Max(height, size.Height);
                }

                width += (elements.Count - 1) * Gap.Width;

                return new Size(width, height);
            }
            case LayoutDirection.Column:
            {
                var width = 0f;
                var height = 0f;
                foreach (var size in elements.Select(el => el.OuterBounds.Size))
                {
                    width = Math.Max(width, size.Width);
                    height += size.Height;
                }

                height += (elements.Count - 1) * Gap.Height;

                return new Size(width, height);
            }
        }
    }

    public void FlexboxLayout()
    {
        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                var top = 0f;
                foreach (var axis in _flexboxAxes)
                {
                    var left = 0f;
                    foreach (var element in axis.Elements)
                    {
                        element.SetLayoutOffset(left, top);
                        left += element.OuterBounds.Width + Gap.Width;
                    }
                    top += axis.CrossAxisSize;
                }
                break;
            }
            case LayoutDirection.Column:
            {
                var left = 0f;
                foreach (var axis in _flexboxAxes)
                {
                    var top = 0f;
                    foreach (var element in axis.Elements)
                    {
                        element.SetLayoutOffset(left, top);
                        top += element.OuterBounds.Height + Gap.Height;
                    }
                    left += axis.CrossAxisSize;
                }
                break;
            }
        }
    }
}