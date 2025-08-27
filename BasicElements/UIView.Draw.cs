namespace SilkyUIFramework.BasicElements;

public partial class UIView
{
    public HiddenBox HiddenBox { get; set; } = HiddenBox.Inner;

    public RectangleRender RectangleRender = new();

    public bool FinallyDrawBorder { get; set; }

    public Color BackgroundColor
    {
        get => RectangleRender.BackgroundColor;
        set => RectangleRender.BackgroundColor = value;
    }

    public Vector4 BorderRadius
    {
        get => RectangleRender.BorderRadius;
        set => RectangleRender.BorderRadius = value;
    }

    public Color BorderColor
    {
        get => RectangleRender.BorderColor;
        set => RectangleRender.BorderColor = value;
    }
    protected virtual void UpdateStatus(GameTime gameTime)
    {
        if (IsMouseHovering)
        {
            if (!HoverTimer.IsForward)
                HoverTimer.StartUpdate();
        }
        else
        {
            if (!HoverTimer.IsReverse)
                HoverTimer.StartReverseUpdate();
        }

        HoverTimer.Update(gameTime);
    }

    protected virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var position = Bounds.Position;
        var size = Bounds.Size;
        RectangleRender.DrawShadow(position, size, SilkyUI.TransformMatrix);
        RectangleRender.Draw(position, size, FinallyDrawBorder, SilkyUI.TransformMatrix);
    }

    public virtual void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawAction?.Invoke(gameTime, spriteBatch);
        Draw(gameTime, spriteBatch);

        var position = Bounds.Position;
        var size = Bounds.Size;
        RectangleRender.DrawOnlyBorder(position, size, SilkyUI.TransformMatrix);
    }
}