using ReLogic.Content;

namespace SilkyUIFramework.Components;

public enum TileMode
{
    Adaptive,

}

public sealed class NinePatch
{
    public Asset<Texture2D> Texture2D { get; set; }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

    public Rectangle TopLeft => new(0, 0, Left, Top);
    public Rectangle TopRight => new(Width - Right, 0, Right, Top);
    public Rectangle BottomLeft => new(0, Height - Bottom, Left, Bottom);
    public Rectangle BottomRight => new(Width - Right, Height - Bottom, Right, Bottom);

    public Rectangle LeftBorder => new(0, Top, Left, Height - Bottom);
    public Rectangle TopBorder => new(Left, 0, Width - Left - Right, Top);
    public Rectangle RightBorder => new(Width - Right, Top, Right, Height - Top - Bottom);
    public Rectangle BottomBorder => new(Left, Height - Bottom, Width - Left - Right, Bottom);

    public Rectangle MiddleArea => new(Left, Top, Width - Right, Height - Bottom);

    public void Draw(SpriteBatch batch, Vector2 position, Color color, Vector2 origin, Vector2 scale, in Matrix matrix)
    {
        var texture = Texture2D.Value;
        if (texture == null) return;
        Width = texture.Width;
        Height = texture.Height;

        var states = batch.BackupStates();
        batch.End();

        // 循环
        batch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, null, matrix);
        // 不循环
        batch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, matrix);

        // 四角
        batch.Draw(texture, position, TopLeft, color, 0f, origin, scale, 0, 0f);
        batch.Draw(texture, position, TopRight, color, 0f, origin, scale, 0, 0f);
        batch.Draw(texture, position, BottomLeft, color, 0f, origin, scale, 0, 0f);
        batch.Draw(texture, position, BottomRight, color, 0f, origin, scale, 0, 0f);

        // 四边
        batch.Draw(texture, position, LeftBorder, color, 0f, origin, scale, 0, 0f);
        batch.Draw(texture, position, TopBorder, color, 0f, origin, scale, 0, 0f);
        batch.Draw(texture, position, RightBorder, color, 0f, origin, scale, 0, 0f);
        batch.Draw(texture, position, BottomBorder, color, 0f, origin, scale, 0, 0f);

        // 中间
        batch.Draw(texture, position, MiddleArea, color, 0f, origin, scale, 0, 0f);

        batch.End();
        states.Begin();
    }
}
