namespace SilkyUIFramework.BasicElements;

/// <summary>
/// 可拖动视图
/// </summary>
public class SUIDraggableView : UIElementGroup
{
    public bool Dragging { get; protected set; }
    public Vector2 DragIncrement { get; set; } = Vector2.Zero;
    public Vector2 MouseDragOffset { get; set; } = Vector2.Zero;
    public bool OccupyMouseInterface { get; set; } = true;

    public readonly UIElementGroup ControlTarget;

    public SUIDraggableView(UIElementGroup controlTarget)
    {
        ControlTarget = controlTarget;
        BackgroundColor = SUIColor.Background * 0.75f;
    }

    public override void OnLeftMouseDown(UIMouseEvent evt)
    {
        Main.NewText(evt.Source == this);
        if (evt.Source == this)
        {
            MouseDragOffset = new Vector2(Main.mouseX, Main.mouseY) - ControlTarget.DragOffset;
            Dragging = true;
        }

        base.OnLeftMouseDown(evt);
    }

    public override void OnLeftMouseUp(UIMouseEvent evt)
    {
        Dragging = false;

        base.OnLeftMouseUp(evt);
    }

    public override void HandleUpdateStatus(GameTime gameTime)
    {
        if (Dragging)
        {
            Main.NewText(123);
            var x = Main.mouseX - MouseDragOffset.X;
            var y = Main.mouseY - MouseDragOffset.Y;
            if (DragIncrement.X != 0) x -= x % DragIncrement.X;
            if (DragIncrement.Y != 0) y -= y % DragIncrement.Y;
            ControlTarget.DragOffset = new Vector2(x, y);
        }

        base.HandleUpdateStatus(gameTime);
    }
}