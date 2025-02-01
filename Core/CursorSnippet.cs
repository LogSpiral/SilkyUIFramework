using ReLogic.Content;
using Terraria.UI.Chat;

namespace SilkyUIFramework.Core;

public class CursorSnippet(SUIEditText editText) : TextSnippet
{
    public override float GetStringLength(DynamicSpriteFont font) => 0f;

    public override bool UniqueDraw(bool justCheckingString,
        out Vector2 size,
        SpriteBatch spriteBatch, Vector2 position = new(), Color color = new(), float scale = 1)
    {
        size = new Vector2(0, 20f) * editText.TextScale;
        if (editText.CanDrawCursor)
        {
            spriteBatch?.Draw(TextureAssets.MagicPixel.Value, position,
                new Rectangle(0, 0, 2, (int)(20 * editText.TextScale)),
                editText.CursorFlashColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            // SDFRectangle.HasBorder(position - new Vector2(0, 0), new Vector2(4, 20) * scale, new Vector4(2f) * scale,
            //     Color.White, 1f, Color.Black, editText.FinalMatrix);
        }

        return true;
    }
}