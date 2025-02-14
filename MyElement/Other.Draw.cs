namespace SilkyUIFramework;

public partial class Other
{
    public event Action<GameTime, SpriteBatch> OnDraw;

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        OnDraw?.Invoke(gameTime, spriteBatch);
        DrawSelf(gameTime, spriteBatch);
        DrawChildren(gameTime, spriteBatch);
    }

    public Color BackgroundColor = Color.White * 0.25f;

    /// <summary>
    /// 先看效果，随便画画
    /// </summary>
    public virtual void DrawSelf(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var position = _bounds.Position;
        var size = _bounds.Size;
        var color = BackgroundColor;

        OtherSystem.Vertices.Add(new(position, color));
        OtherSystem.Vertices.Add(new(position.X + size.Width, position.Y, color));
        OtherSystem.Vertices.Add(new(position.X, position.Y + size.Height, color));

        OtherSystem.Vertices.Add(new(position.X, position.Y + size.Height, color));
        OtherSystem.Vertices.Add(new(position.X + size.Width, position.Y, color));
        OtherSystem.Vertices.Add(new(position + size, color));
    }

    public static void DrawBox(SpriteBatch spriteBatch, Vector2 position, Vector2 size, Color color)
    {
        // var texture2D = TextureAssets.MagicPixel.Value;
        // var sourceRectangle = new Rectangle(0, 0, 1, 1);
        // spriteBatch.Draw(texture2D, position, sourceRectangle, color, 0f, Vector2.Zero, size, 0, 0f);
    }

    public virtual void DrawChildren(GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach (var child in _children)
        {
            child.Draw(gameTime, spriteBatch);
        }
    }
}