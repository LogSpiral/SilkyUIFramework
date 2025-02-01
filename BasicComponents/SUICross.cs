using SilkyUIFramework.BasicElements;
using SilkyUIFramework.Graphics2D;

namespace SilkyUIFramework.BasicComponents;

public class SUICross(Color backgroundColor, Color borderColor) : View
{
    public float
        CrossSize = 24f,
        CrossRounded = 4,
        CrossBorder = 2;

    public Color
        CrossBorderColor = borderColor,
        CrossBorderHoverColor = borderColor,
        CrossBackgroundColor = backgroundColor,
        CrossBackgroundHoverColor = backgroundColor;

    public Vector2 CrossOffset = Vector2.Zero;

    public override void MouseOver(UIMouseEvent evt)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        base.MouseOver(evt);
    }

    public override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);

        var position = GetDimensions().Position();
        var size = GetDimensions().Size();

        var crossBorderColor = HoverTimer.Lerp(CrossBorderColor, CrossBorderHoverColor);
        var crossBackground = HoverTimer.Lerp(CrossBackgroundColor, CrossBackgroundHoverColor);
        var crossPosition = position + (size - new Vector2(CrossSize)) / 2f + CrossOffset;

        SDFGraphics.HasBorderCross(crossPosition, CrossSize, CrossRounded,
            crossBackground, CrossBorder, crossBorderColor, FinalMatrix);
    }
}