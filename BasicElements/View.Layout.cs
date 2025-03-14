using Microsoft.Xna.Framework.Content;
using SilkyUIFramework.Core;

namespace SilkyUIFramework.BasicElements;

public enum Display
{
    Flow,
    Flexbox,
    Grid,
}

/// <summary> 布局方向 </summary>
public enum LayoutDirection
{
    Row,
    Column
}

public partial class View
{

    /// <summary> 盒子模型计算方式 </summary>
    public BoxSizing BoxSizing { get; set; } = BoxSizing.Border;

    /// <summary> 元素定位 </summary>
    public Positioning Positioning { get; set; } = Positioning.Relative;

    public Display Display { get; set; } = Display.Flow;

    /// <summary> Flex 布局方向 </summary>
    public LayoutDirection LayoutDirection { get; set; } = LayoutDirection.Row;

    public LayoutDirection ArrangeDirection { get; protected set; }

    /// <summary> 文档流元素 </summary>
    protected readonly List<View> FlowElements = [];

    /// <summary> 非文档流元素 </summary>
    protected readonly List<UIElement> AbsoluteElements = [];

    protected virtual Vector2 GetContainer()
    {
        if (Positioning is not Positioning.Absolute && Parent is View { Display: Display.Grid } parent)
            return parent.GetTemplateValue(FlowIndex);

        return Parent is null ? SilkyUIHelper.GetScreenScaledSize() : Parent._innerDimensions.Size();
    }

    protected Constraints Constraints;

    public int FlowIndex { get; protected set; }

    protected readonly List<ElementGroup> ElementGroups = [];

    protected void RecalculateSize(Vector2 container)
    {
        #region Min Max

        Constraints.MinWidth = MinWidth.GetValue(container.X);
        Constraints.MaxWidth = MaxWidth.GetValue(container.X);
        Constraints.MinHeight = MinHeight.GetValue(container.Y);
        Constraints.MaxHeight = MaxHeight.GetValue(container.Y);

        #endregion

        #region width height

        float width, height;
        if (Positioning is not Positioning.Absolute && FlexWeight.Enable && Parent is View { Display: Display.Flexbox, FlexMainAxisFixed: true } parent)
        {
            switch (parent.LayoutDirection)
            {
                default:
                case LayoutDirection.Row:
                    width = FlexWeight.Value * parent.Portion;
                    height = SpecifyHeight ? Constraints.ClampHeight(Height.GetValue(container.Y)) : 0;
                    break;
                case LayoutDirection.Column:
                    width = SpecifyWidth ? Constraints.ClampWidth(Width.GetValue(container.X)) : 0;
                    height = FlexWeight.Value * parent.Portion;
                    break;
            }
        }
        else
        {
            width = SpecifyWidth ? Constraints.ClampWidth(Width.GetValue(container.X)) : 0;
            height = SpecifyHeight ? Constraints.ClampHeight(Height.GetValue(container.Y)) : 0;
        }

        #endregion

        var outerSize = GetOuterSize(width, height);

        _outerDimensions.Width = outerSize.X;
        _outerDimensions.Height = outerSize.Y;

        _dimensions.Width = _outerDimensions.Width - MarginLeft - MarginRight;
        _dimensions.Height = _outerDimensions.Height - MarginTop - MarginBottom;

        _innerDimensions.Width = _dimensions.Width - this.HPadding() - Border * 2;
        _innerDimensions.Height = _dimensions.Height - this.VPadding() - Border * 2;
    }

    /// <summary> 排列流元素 </summary>
    protected virtual void ArrangeElements()
    {
        if (FlowElements.Count == 0)
            return;
        ElementGroups.Clear();

        switch (Display)
        {
            default:
                break;
            case Display.Flow:
                ArrangeByFlow();
                break;
            case Display.Flexbox:
                SortElementGroupsByFlexbox();
                ArrangeElementsByFlexbox();
                break;
            case Display.Grid:
                SortElementGroupsByGrid();
                ArrangeElementsByGird();
                break;
        }
    }

    public bool FlexMainAxisFixed
    {
        get
        {
            return LayoutDirection switch
            {
                LayoutDirection.Row => SpecifyWidth,
                LayoutDirection.Column => SpecifyHeight,
                _ => false,
            };
        }
    }

    public override void Recalculate()
    {
        var container = GetContainer();
        var position = new Vector2(Left.GetValue(container.X), Top.GetValue(container.Y));

        // 计算自身大小，如果是自适应大小，这一步计算的结果会是 0
        RecalculateSize(container);

        if (Display is Display.Grid)
        {
            // 计算 Grid 布局的 [行,列]
            RecalculateTemplateUnitsValue();
        }

        // 对元素进行分类: 相对定位、绝对定位
        ClassifyElements();

        // 计算子元素
        RecalculateChildren();

        // 排列流元素
        ArrangeElements();

        // 计算自身大小那一步如果是自适应大小，则计算结果是零
        // 到了这一步子元素大小已经计算好了，此刻再设定自身大小
        AdaptSizeByContent();

        #region HAlign VAlign

        switch (Positioning)
        {
            default:
            case Positioning.Absolute:
                position += (container - _outerDimensions.Size()) * new Vector2(HAlign, VAlign);
                break;
            case Positioning.Sticky:
            case Positioning.Relative:
            {
                if (Parent is not View parent) break;
                var offset = Vector2.Zero;
                if (parent.SpecifyWidth)
                    offset.X += (container.X - _outerDimensions.Width) * HAlign;
                if (parent.SpecifyHeight)
                    offset.Y += (container.Y - _outerDimensions.Height) * VAlign;
                position += offset;
                break;
            }
        }

        Position = position;

        #endregion

        AbsoluteElements.ForEach(element => element.Recalculate());
    }

    #region CalculateOuterDimensions

    protected virtual Vector2 GetOuterSize(float width, float height)
    {
        return BoxSizing switch
        {
            BoxSizing.Border => new Vector2(
                width + this.HMargin(),
                height + this.VMargin()),
            BoxSizing.Content => new Vector2(
                width + this.HPadding() + this.HMargin() + Border * 2f,
                height + this.VPadding() + this.VMargin() + Border * 2f),
            _ => new Vector2(
                width + this.HMargin(),
                height + this.VMargin())
        };
    }

    #endregion

    /// <summary>
    /// FlexWeight 计算使用
    /// </summary>
    protected virtual float Portion { get; set; }

    public override void RecalculateChildren()
    {
        // Flexbox 布局, 不换行, 固定主轴大小时, 子元素 FlexWeight 属性生效
        if (Display is Display.Flexbox && !FlexWrap && FlexMainAxisFixed)
        {
            // 分出来使用 weight 和不使用的元素
            var generalElements = new List<View>();
            var weightElements = new List<View>();

            var sumWeight = 0f;
            FlowElements.ForEach(element =>
            {
                if (element.FlexWeight.Enable)
                {
                    sumWeight += element.FlexWeight.Value;
                    weightElements.Add(element);
                }
                else
                {
                    element.Recalculate();
                    generalElements.Add(element);
                }
            });

            if (sumWeight != 0)
            {
                switch (LayoutDirection)
                {
                    default:
                    case LayoutDirection.Row:
                        var sumWidth = generalElements.Sum(element => element._outerDimensions.Width);
                        var sumGapWidth = (FlowElements.Count - 1) * Gap.X;
                        Portion = (_innerDimensions.Width - sumWidth - sumGapWidth) / sumWeight;
                        break;
                    case LayoutDirection.Column:
                        var sumHeight = generalElements.Sum(element => element._outerDimensions.Height);
                        var sumGapHeight = (FlowElements.Count - 1) * Gap.Y;
                        Portion = (_innerDimensions.Height - sumHeight - sumGapHeight) / sumWeight;
                        break;
                }
            }

            weightElements.ForEach(element => element.Recalculate());
            return;
        }

        FlowElements?.ForEach(element => element.Recalculate());
    }

    public virtual Vector2 CalculateContentSize()
    {
        return Display switch
        {
            Display.Flexbox => CalculateContentSizeByFlexbox(),
            Display.Grid => CalculateContentSizeByGrid(),
            _ => CalculateContentSizeByFlow(),
        };
    }

    /// <summary> 根据内容调整尺寸 </summary>
    protected virtual void AdaptSizeByContent()
    {
        if (SpecifyWidth && SpecifyHeight) return;
        var content = CalculateContentSize();

        if (!SpecifyWidth)
        {
            _dimensions.Width = Constraints.ClampWidth(content.X + PaddingLeft + PaddingRight + Border * 2);
            _outerDimensions.Width = _dimensions.Width + MarginLeft + MarginRight;
            _innerDimensions.Width = _dimensions.Width - PaddingLeft - PaddingRight - Border * 2;
        }

        if (SpecifyHeight) return;
        _dimensions.Height = Constraints.ClampHeight(content.Y + PaddingTop + PaddingBottom + Border * 2);
        _outerDimensions.Height = _dimensions.Height + MarginTop + MarginBottom;
        _innerDimensions.Height = _dimensions.Height - PaddingLeft - PaddingRight - Border * 2;
    }

    #region Apply Position ScrollPosition Offset

    protected bool PositionChanged = true;

    private Vector2 _position;
    private Vector2 _offset;
    private Vector2 _scrollPosition;
    private Vector2 _dragOffset;

    protected Vector2 Position
    {
        get => _position;
        set
        {
            if (_position == value)
                return;
            _position = value;
            PositionChanged = true;
        }
    }

    public Vector2 Offset
    {
        get => _offset;
        set
        {
            if (_offset == value)
                return;
            _offset = value;
            PositionChanged = true;
        }
    }

    public Vector2 DragOffset
    {
        get => _dragOffset;
        set
        {
            if (_dragOffset == value)
                return;
            _dragOffset = value;
            PositionChanged = true;
        }
    }

    public Vector2 ScrollPosition
    {
        get => _scrollPosition;
        set
        {
            if (_scrollPosition == value)
                return;
            _scrollPosition = value;
            PositionChanged = true;
        }
    }

    public Vector2 GetStartPoint() =>
        Parent is not { } uie ? Vector2.Zero : uie._innerDimensions.Position();

    public Vector2 GetParentScrollPosition() =>
        Parent is View view ? view.ScrollPosition : Vector2.Zero;

    private void TrackPositionChange()
    {
        if (PositionChanged) ApplyPosition(GetStartPoint(), GetContainer(), GetParentScrollPosition());
        foreach (var child in Elements.OfType<View>())
            child.TrackPositionChange();
    }

    public StickyType StickyType { get; set; } = StickyType.Top;
    public Vector4 Sticky { get; set; }

    public void ApplyPosition(Vector2 start, Vector2 container, Vector2 scroll)
    {
        _outerDimensions.X = start.X + scroll.X + Position.X + Offset.X + DragOffset.X;
        _outerDimensions.Y = start.Y + scroll.Y + Position.Y + Offset.Y + DragOffset.Y;

        if (Positioning is Positioning.Sticky)
        {
            switch (StickyType)
            {
                case StickyType.Left:
                    _outerDimensions.X = Math.Max(_outerDimensions.X, start.X + Sticky.X);
                    break;
                default:
                case StickyType.Top:
                    _outerDimensions.Y = Math.Max(_outerDimensions.Y, start.Y + Sticky.Y);
                    break;
                case StickyType.Right:
                    _outerDimensions.X = Math.Min(_outerDimensions.X,
                        start.X + container.X - _outerDimensions.Width - Sticky.Z);
                    break;
                case StickyType.Bottom:
                    _outerDimensions.Y = Math.Min(_outerDimensions.Y,
                        start.Y + container.Y - _outerDimensions.Height - Sticky.W);
                    break;
            }
        }

        _dimensions.X = _outerDimensions.X + MarginLeft;
        _dimensions.Y = _outerDimensions.Y + MarginTop;

        _innerDimensions.X = _dimensions.X + Border + PaddingLeft;
        _innerDimensions.Y = _dimensions.Y + Border + PaddingTop;

        foreach (var child in Elements.OfType<View>())
            child.ApplyPosition(_innerDimensions.Position(), _innerDimensions.Size(), ScrollPosition);
        PositionChanged = false;
    }

    #endregion
}