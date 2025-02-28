namespace SilkyUIFramework;

public interface IView
{
    Bounds Bounds { get; }
    Bounds OuterBounds { get; }
    Bounds InnerBounds { get; }

    Size Gap { get; }

    void Update(GameTime gameTime);
    void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}
