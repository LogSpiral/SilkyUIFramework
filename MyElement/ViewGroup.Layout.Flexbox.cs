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
        if (!_flexWrap || !MustWrap || LayoutElements.Count <= 0) return;

        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                WrapElements(InnerBounds.Width, Gap.Width,
                    LayoutElements.Select(el =>
                    {
                        var bounds = el.GetOuterBounds();
                        return (el, bounds.Width, bounds.Height);
                    }).ToArray());

                return;
            }
            case LayoutDirection.Column:
            {
                WrapElements(InnerBounds.Height, Gap.Height,
                    LayoutElements.Select(el =>
                    {
                        var bounds = el.GetOuterBounds();
                        return (el, bounds.Height, bounds.Width);
                    }).ToArray());

                return;
            }
        }
    }

    private void WrapElements(float maxSize, float gap,
        (UIView Element, float MainAxisSize, float CrossAxisSize)[] items)
    {
        var isRow = LayoutDirection == LayoutDirection.Row;
        var track = new FlexboxAxis(isRow, items[0].Element)
        {
            MainAxisSize = items[0].MainAxisSize,
            CrossAxisSize = items[0].CrossAxisSize
        };

        for (var i = 1; i < items.Length; i++)
        {
            var item = items[i];
            track.MainAxisSize = item.MainAxisSize + gap;
            track.CrossAxisSize = MathHelper.Max(track.CrossAxisSize, item.CrossAxisSize);

            if (track.MainAxisSize > maxSize)
            {
                _flexboxAxes.Add(track);
                track = new FlexboxAxis(isRow, item.Element)
                {
                    MainAxisSize = item.MainAxisSize,
                    CrossAxisSize = item.CrossAxisSize
                };
                continue;
            }

            track.Elements.Add(item.Element);
        }

        if (track.Elements.Count > 0) _flexboxAxes.Add(track);
    }

    private Size FlexboxMeasure()
    {
        if (!_flexWrap || !MustWrap) return FlexboxMeasure(LayoutElements);

        switch (LayoutDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                var width = 0f;
                var height = 0f;

                foreach (var track in _flexboxAxes)
                {
                    width = Math.Max(width, track.MainAxisSize);
                    height += track.CrossAxisSize;
                }

                height += (_flexboxAxes.Count - 1) * Gap.Height;

                return new Size(width, height);
            }
            case LayoutDirection.Column:
            {
                var width = 0f;
                var height = 0f;

                foreach (var track in _flexboxAxes)
                {
                    width += track.CrossAxisSize;
                    height = Math.Max(height, track.MainAxisSize);
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
                foreach (var size in elements.Select(el => el.GetOuterBounds().Size))
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
                foreach (var size in elements.Select(el => el.GetOuterBounds().Size))
                {
                    width = Math.Max(width, size.Width);
                    height += size.Height;
                }

                height += (elements.Count - 1) * Gap.Height;

                return new Size(width, height);
            }
        }
    }

    public void FlexboxLayout() { }
}