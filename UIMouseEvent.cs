namespace SilkyUIFramework;

public class UIMouseEvent(UIView source, Vector2 position)
{
    public UIView Source { get; } = source;
    public Vector2 MousePosition { get; } = position;
}

public class UIScrollWheelEvent(UIView source, Vector2 position, int scrollDelta) : UIMouseEvent(source, position)
{
    public int ScrollDelta { get; } = scrollDelta;
}