namespace SilkyUIFramework;

public class RoundedRectangle
{
    public float Border;
    public Color BorderColor = Color.Transparent;
    public Color BgColor = Color.Transparent;
    public Vector4 CornerRadius = Vector4.Zero;

    public void Draw(Vector2 position, Vector2 size, bool withoutBorder, Matrix matrix)
    {
        if (Border > 0)
        {
            if (BorderColor == Color.Transparent || withoutBorder)
            {
                if (BgColor != Color.Transparent)
                    SDFRectangle.DrawWithoutBorder(position + new Vector2(Border), size - new Vector2(Border * 2f),
                        CornerRadius - new Vector4(Border), BgColor, matrix);
            }
            else
            {
                SDFRectangle.DrawWithBorder(position, size, CornerRadius, BgColor, Border, BorderColor, matrix);
            }
        }
        else if (BgColor != Color.Transparent)
        {
            SDFRectangle.DrawWithoutBorder(position, size, CornerRadius, BgColor, matrix);
        }
    }

    public void DrawOnlyBorder(Vector2 position, Vector2 size, Matrix matrix) =>
        SDFRectangle.DrawWithBorder(position, size, CornerRadius, Color.Transparent, Border, BorderColor, matrix);

    public void CopyStyle(RoundedRectangle roundedRectangle)
    {
        CornerRadius = roundedRectangle.CornerRadius;
        Border = roundedRectangle.Border;
        BgColor = roundedRectangle.BgColor;
        BorderColor = roundedRectangle.BorderColor;

        ShadowExpand = roundedRectangle.ShadowExpand;
        ShadowWidth = roundedRectangle.ShadowWidth;
        ShadowColor = roundedRectangle.ShadowColor;
    }

    public float ShadowExpand = 10f;
    public float ShadowWidth = 10f;
    public Color ShadowColor = Color.Transparent;

    public void DrawShadow(Vector2 position, Vector2 size, Matrix matrix)
    {
        if (ShadowColor == Color.Transparent) return;

        position -= new Vector2(ShadowExpand);
        size += new Vector2(ShadowExpand * 2);
        // var cornerRadius = CornerRadius + new Vector4(ShadowExpand / 2f);
        SDFRectangle.DrawShadow(position, size, CornerRadius, ShadowColor, ShadowWidth, matrix);
    }

    public RoundedRectangle Clone() => MemberwiseClone() as RoundedRectangle;

    public override string ToString()
    {
        return $"border: {Border}, bgColor: {BgColor}, borderColor: {BorderColor}, cornerRadius: {CornerRadius}";
    }
}