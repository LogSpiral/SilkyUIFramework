namespace SilkyUIFramework.BasicElements;

/// <summary>
/// 可拖动视图
/// </summary>
public class SUIDraggableView : View
{
    public bool Draggable { get; set; }
    public bool Dragging { get; protected set; }
    public Vector2 DragIncrement { get; set; } = Vector2.Zero;
    public Vector2 MouseDragOffset { get; set; } = Vector2.Zero;
    public bool OccupyMouseInterface { get; set; } = true;

    public SUIDraggableView(bool draggable = true)
    {
        Draggable = draggable;

        Border = 2;
        BorderColor = new Color(18, 18, 38) * 0.75f;
        BgColor = new Color(63, 65, 151) * 0.75f;
        CornerRadius = new Vector4(12);

        RoundedRectangle.ShadowColor = new Color(18, 18, 38) * 0.1f;
        RoundedRectangle.ShadowExpand = 50f;
        RoundedRectangle.ShadowWidth = 50f;

        SetPadding(12f);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        if (!Draggable ||
            (evt.Target != this && evt.Target is not View { DragIgnore: true } &&
             !evt.Target.GetType().IsAssignableFrom(typeof(UIElement)))) return;

        MouseDragOffset = new Vector2(Main.mouseX, Main.mouseY) - DragOffset;
        Dragging = true;
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        base.LeftMouseUp(evt);
        Dragging = false;
    }

    public override void Update(GameTime gameTime)
    {
        if (IsMouseHovering)
        {
            // 锁定滚动条
            PlayerInput.LockVanillaMouseScroll("SilkyUIFramework");
            // 锁定鼠标操作
            if (OccupyMouseInterface)
                Main.LocalPlayer.mouseInterface = true;
        }

        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (Dragging)
        {
            var x = Main.mouseX - MouseDragOffset.X;
            var y = Main.mouseY - MouseDragOffset.Y;
            if (DragIncrement.X != 0) x -= x % DragIncrement.X;
            if (DragIncrement.Y != 0) y -= y % DragIncrement.Y;
            DragOffset = new Vector2(x, y);
        }

        base.Draw(spriteBatch);
    }
}