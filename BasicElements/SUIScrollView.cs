namespace SilkyUIFramework.BasicElements;

public class SUIScrollContainer : View
{
    public SUIScrollContainer()
    {
        OverflowHidden = true;
    }

    public override View AppendFromView(View child)
    {
        child.Remove();
        child.Parent = this;
        Elements.Add(child);
        if (Parent is SUIScrollView sUIScrollView)
            sUIScrollView.Recalculate();
        else Recalculate();
        return this;
    }
}

public class SUIScrollView : View
{
    public readonly Direction Direction;

    public readonly SUIScrollContainer Container;
    public readonly SUIScrollbar ScrollBar;

    public SUIScrollView(Direction direction = Direction.Vertical)
    {
        Direction = direction;

        Display = Display.Flexbox;
        FlexWrap = false;
        Gap = new Vector2(8f);

        Container = new SUIScrollContainer
        {
            FlexWeight = { Enable = true, Value = 1 },
            Gap = new Vector2(8f),
            Display = Display.Flexbox,
            MainAlignment = MainAlignment.SpaceBetween,
            FlexWrap = true,
        }.Join(this);
        Container.SetSize(0f, 0f, 1f, 1f);

        ScrollBar = new SUIScrollbar(direction, Container)
        {
            CornerRadius = new Vector4(2f),
            BgColor = Color.Black * 0.25f,
        }.Join(this);
        ScrollBar.OnCurrentScrollPositionChanged += UpdateScrollPosition;

        switch (Direction)
        {
            case Direction.Horizontal:
                LayoutDirection = LayoutDirection.Column;
                Container.LayoutDirection = LayoutDirection.Column;
                ScrollBar.SetSize(0f, 4f, 1f, 0f);
                break;
            default:
            case Direction.Vertical:
                LayoutDirection = LayoutDirection.Row;
                Container.LayoutDirection = LayoutDirection.Row;
                ScrollBar.SetSize(4f, 0f, 0f, 1f);
                break;
        }
    }

    public void UpdateScrollPosition(Vector2 currentScrollPosition) =>
        Container.ScrollPosition = -currentScrollPosition;

    public override void Recalculate()
    {
        base.Recalculate();

        if (Container == null) return;

        var content = Container.CalculateContentSize();
        switch (Direction)
        {
            case Direction.Horizontal:
                ScrollBar?.SetHScrollRange(Container._innerDimensions.Width, content.X);
                break;
            default:
            case Direction.Vertical:
                ScrollBar?.SetVScrollRange(Container._innerDimensions.Height, content.Y);
                break;
        }
    }

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        switch (Direction)
        {
            case Direction.Horizontal:
                ScrollBar.HScrollBy(-evt.ScrollWheelValue);
                break;
            default:
            case Direction.Vertical:
                ScrollBar.VScrollBy(-evt.ScrollWheelValue);
                break;
        }

        base.ScrollWheel(evt);
    }

    public override View AppendFromView(View child)
    {
        return child is SUIScrollbar or SUIScrollContainer
            ? base.AppendFromView(child)
            : Container?.AppendFromView(child);
    }
}