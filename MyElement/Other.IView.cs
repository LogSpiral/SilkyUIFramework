namespace SilkyUIFramework;

public partial class Other : IView
{
    /// <summary>
    /// 无效化: 交互事件, 更新, 绘制, 布局
    /// </summary>
    public bool Invalidate
    {
        get => _invalidate;
        set
        {
            if (_invalidate == value) return;
            _invalidate = value;
            MarkLayoutDirty();
        }
    }
    private bool _invalidate;

    private Bounds _bounds;
    public Bounds Bounds => _bounds;

    private Bounds _innerBounds;
    public Bounds InnerBounds => _innerBounds;

    private Bounds _outerBounds;
    public Bounds OuterBounds => _outerBounds;

    public virtual void Update(GameTime gameTime)
    {

    }
    public event Action<GameTime, SpriteBatch> DrawAction;

    public void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawAction?.Invoke(gameTime, spriteBatch);
        Draw(gameTime, spriteBatch);
        DrawSelf(gameTime, spriteBatch);
        DrawChildren(gameTime, spriteBatch);
    }

    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {

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

        // OtherSystem.Vertices.AddRange([
        //     new(position, color),
        //     new(position.X + size.Width, position.Y, color),
        //     new(position.X, position.Y + size.Height, color),
        //     new(position.X, position.Y + size.Height, color),
        //     new(position.X + size.Width, position.Y, color),
        //     new(position + size, color)
        // ]);
    }

    public static void DrawBox(SpriteBatch spriteBatch, Vector2 position, Vector2 size, Color color)
    {
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, position, new(0, 0, 1, 1), color, 0f, Vector2.Zero, size, 0, 0f);
    }

    public virtual void DrawChildren(GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach (var child in _children)
        {
            child.HandleDraw(gameTime, spriteBatch);
        }
    }
}