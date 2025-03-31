namespace SilkyUIFramework.BasicElements;

public class SUIScrollView : UIElementGroup
{
    public readonly Direction Direction;

    public readonly SUIScrollbar ScrollBar;
    public readonly SUIScrollMask Mask;
    public readonly SUIScrollContainer Container;

    public SUIScrollView(Direction direction = Direction.Vertical)
    {
        Direction = direction;
        SetGap(8f);

        Mask = new SUIScrollMask(this)
        {
            HiddenBox = HiddenBox.Inner,
            FlexGrow = 1f,
            FlexShrink = 1f,
        };
        Mask.Join(this);
        Mask.SetSize(0f, 0f, 1f, 1f);

        Container = new SUIScrollContainer(this)
        {
            LayoutType = LayoutType.Flexbox,
            FlexDirection = FlexDirection.Row,
            MainAlignment = MainAlignment.SpaceBetween,
            FlexWrap = true,
            FitWidth = false,
            FitHeight = true,
        }.Join(Mask);
        Container.SetWidth(0f, 1f);
        Container.SetHeight(0f, 1f);
        Container.SetGap(8f);

        ScrollBar = new SUIScrollbar(direction, Container)
        {
            BorderRadius = new Vector4(2f),
            BackgroundColor = Color.Black * 0.25f,
            FitWidth = false,
        }.Join(this);
        ScrollBar.SetSize(8f, 0f, 0f, 1f);
        ScrollBar.OnCurrentScrollPositionChanged += Container.UpdateScrollPosition;

        switch (Direction)
        {
            case Direction.Horizontal:
                FlexDirection = FlexDirection.Column;
                ScrollBar.SetSize(0f, 4f, 1f, 0f);
                break;
            default:
            case Direction.Vertical:
                FlexDirection = FlexDirection.Row;
                ScrollBar.SetSize(4f, 0f, 0f, 1f);
                break;
        }
    }

    public override void OnMouseWheel(UIScrollWheelEvent evt)
    {
        switch (Direction)
        {
            case Direction.Horizontal:
                ScrollBar.HScrollBy(-evt.ScrollDelta);
                break;
            default:
            case Direction.Vertical:
                ScrollBar.VScrollBy(-evt.ScrollDelta);
                break;
        }

        base.OnMouseWheel(evt);
    }
}

/// <summary> 滚动方向 </summary>
public enum Direction
{
    Horizontal,
    Vertical,
}