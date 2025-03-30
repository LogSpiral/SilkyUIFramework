
namespace SilkyUIFramework.UserInterfaces;

#if DEBUG

[RegisterUI("Vanilla: Radial Hotbars", "SilkyUI: MenuUI", int.MinValue)]
public class MenuUI : BasicBody
{
    public SUIDraggableView DraggableView { get; private set; }
    protected override void OnInitialize()
    {
        return;
        DraggableView = new SUIDraggableView(this)
        { }.Join(this);
        DraggableView.SetWidth(0f, 1f);
        DraggableView.SetHeight(30f, 0f);
    }

    public void Open()
    {
        DraggableView.DragOffset = new Vector2(0f, 0f);
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);
    }
}

#endif