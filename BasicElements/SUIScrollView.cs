using System.ComponentModel;

namespace SilkyUIFramework.BasicElements;

public class SUIScrollMask : UIElementGroup
{
    public SUIScrollMask()
    {
        OverflowHidden = true;
    }

    public override UIView GetElementAt(Vector2 mousePosition)
    {
        if (Invalid) return null;

        if (!ContainsPoint(mousePosition)) return null;

        foreach (var child in ElementsSortedByZIndex.Reverse<UIView>())
        {
            var target = child.GetElementAt(mousePosition);
            if (target != null) return target;
        }

        // 所有子元素都不符合条件, 如果自身不忽略鼠标交互, 则返回自己
        return IgnoreMouseInteraction ? null : this;
    }
}

public class SUIScrollContainer(SUIScrollMask mask) : UIElementGroup
{
    public readonly SUIScrollMask Mask = mask;

    public void UpdateScrollPosition(Vector2 currentScrollPosition) => ScrollOffset = -currentScrollPosition;

    public override void DrawChildren(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var innerBounds = Mask.InnerBounds;
        foreach (var child in ElementsSortedByZIndex.Reverse<UIView>().Where(el => el.OuterBounds.Intersects(innerBounds)))
        {
            child.HandleDraw(gameTime, spriteBatch);
        }
    }
}

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

        Mask = new SUIScrollMask
        {
            HiddenBox = HiddenBox.Inner,
            FlexGrow = 1f,
            FlexShrink = 1f,
        };
        Mask.Join(this);
        Mask.SetSize(0f, 0f, 1f, 1f);

        Container = new SUIScrollContainer(Mask)
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

    public override void RecalculateChildrenHeight()
    {
        base.RecalculateChildrenHeight();

        switch (Direction)
        {
            case Direction.Horizontal:
                ScrollBar?.SetHScrollRange(Mask.InnerBounds.Width, Container.OuterBounds.Width);
                break;
            default:
            case Direction.Vertical:
                ScrollBar?.SetVScrollRange(Mask.InnerBounds.Height, Container.OuterBounds.Height);
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