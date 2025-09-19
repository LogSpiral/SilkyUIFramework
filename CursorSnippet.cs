using Terraria.UI.Chat;

namespace SilkyUIFramework;

public class CursorSnippet : TextSnippet
{
    public SUIEditText EditText;
    public int Height { get; set; } = 20;

    public CursorSnippet(SUIEditText editText)
    {
        Text = " ";
        EditText = editText;
    }

    public override float GetStringLength(DynamicSpriteFont font) => 0f;

    public override bool UniqueDraw(bool justCheckingString,
        out Vector2 size, SpriteBatch spriteBatch, Vector2 position = new(), Color color = new(), float scale = 1)
    {
        size = new Vector2(0, Height * scale);

        if (EditText.CanDrawCursor)
        {
            spriteBatch?.Draw(TextureAssets.MagicPixel.Value, position,
                new Rectangle(0, 0, 2, (int)Math.Round(Height * scale)), EditText.CursorFlashColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        return true;
    }
}