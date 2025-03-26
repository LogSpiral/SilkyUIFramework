namespace SilkyUIFramework;

public class RectangleRender
{
    public float Border = 0f;
    public Color BorderColor = Color.Transparent;
    public Color BackgroundColor = Color.Transparent;
    public Vector4 BorderRadius = Vector4.Zero;

    public void Draw(Vector2 position, Vector2 size, bool noBorder, Matrix matrix)
    {
        if (Border > 0f)
        {
            if (BorderColor == Color.Transparent || noBorder)
            {
                if (BackgroundColor != Color.Transparent)
                    SDFRectangle.DrawNoBorder(position + new Vector2(Border), size - new Vector2(Border * 2f),
                        BorderRadius - new Vector4(Border), BackgroundColor, matrix);
            }
            else
            {
                SDFRectangle.DrawHasBorder(position, size, BorderRadius, BackgroundColor, Border, BorderColor, matrix);
            }
        }
        else if (BackgroundColor != Color.Transparent)
        {
            SDFRectangle.DrawNoBorder(position, size, BorderRadius, BackgroundColor, matrix);
        }
    }

    public void DrawOnlyBorder(Vector2 position, Vector2 size, Matrix matrix) =>
        SDFRectangle.DrawHasBorder(position, size, BorderRadius, Color.Transparent, Border, BorderColor, matrix);

    public void CopyStyle(RectangleRender roundedRectangle)
    {
        BorderRadius = roundedRectangle.BorderRadius;
        Border = roundedRectangle.Border;
        BackgroundColor = roundedRectangle.BackgroundColor;
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
        SDFRectangle.DrawShadow(position, size, BorderRadius, ShadowColor, ShadowWidth, matrix);
    }

    public RectangleRender Clone() => MemberwiseClone() as RectangleRender;

    public override string ToString()
    {
        return $"border: {Border}, bgColor: {BackgroundColor}, borderColor: {BorderColor}, cornerRadius: {BorderRadius}";
    }
}