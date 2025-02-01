namespace SilkyUIFramework.Graphics2D;

public static class SDFRectangle
{
    public static readonly EffectPass SpriteEffectPass = Main.spriteBatch.spriteEffectPass;
    public static readonly GraphicsDevice GraphicsDevice = Main.graphics.GraphicsDevice;
    private static Effect Effect => ModAsset.SDFRectangle.Value;

    public static void DrawWithBorder(Vector2 position, Vector2 size,
        Vector4 rounded, Color backgroundColor, float border, Color borderColor, Matrix matrix)
    {
        MatrixHelper.Transform2SDFMatrix(ref matrix);

        float innerShrinkage = 1 / Main.UIScale;

        SetSmoothstepRange();
        SetMatrixWithBgColor(matrix, backgroundColor);
        SetInnerShrinkage(innerShrinkage);

        Effect.Parameters["uBorder"].SetValue(border);
        Effect.Parameters["uBorderColor"].SetValue(borderColor.ToVector4());

        Effect.CurrentTechnique.Passes["HasBorder"].Apply();
        DrawRectanglePrimitives(
            position - new Vector2(innerShrinkage),
            size + new Vector2(innerShrinkage * 2),
            rounded + new Vector4(innerShrinkage)
        );
    }

    public static void DrawWithoutBorder(Vector2 position, Vector2 size,
        Vector4 rounded, Color backgroundColor, Matrix matrix)
    {
        MatrixHelper.Transform2SDFMatrix(ref matrix);

        float innerShrinkage = 1 / Main.UIScale;

        SetSmoothstepRange();
        SetMatrixWithBgColor(matrix, backgroundColor);
        SetInnerShrinkage(innerShrinkage);

        Effect.CurrentTechnique.Passes["NoBorder"].Apply();
        DrawRectanglePrimitives(
            position - new Vector2(innerShrinkage),
            size + new Vector2(innerShrinkage * 2),
            rounded + new Vector4(innerShrinkage)
        );
    }

    public static void DrawShadow(Vector2 position, Vector2 size,
        Vector4 rounded, Color backgroundColor, float shadow, Matrix matrix)
    {
        MatrixHelper.Transform2SDFMatrix(ref matrix);

        SetSmoothstepRange();
        SetMatrixWithBgColor(matrix, backgroundColor);
        Effect.Parameters["uShadowSize"].SetValue(shadow);

        Effect.CurrentTechnique.Passes["Shadow"].Apply();
        DrawRectanglePrimitives(position, size, rounded + new Vector4(shadow));
    }

    #region SET

    private static void SetInnerShrinkage(float innerShrinkage)
    {
        Effect.Parameters["uInnerShrinkage"].SetValue(innerShrinkage);
    }

    private static void SetMatrixWithBgColor(Matrix matrix, Color backgroundColor)
    {
        Effect.Parameters["uTransformMatrix"].SetValue(matrix);
        Effect.Parameters["uBackgroundColor"].SetValue(backgroundColor.ToVector4());
    }

    private static void SetSmoothstepRange()
    {
        const float root2Over2 = 1.414213562373f / 2f;
        Effect.Parameters["uSmoothstepRange"].SetValue(new Vector2(-root2Over2, root2Over2) / Main.UIScale);
    }

    #endregion

    private static void DrawRectanglePrimitives(Vector2 position, Vector2 size, Vector4 rounded)
    {
        size /= 2f;

        List<SDFGraphicsVertexType> vertices = [];

        var coordQ1 = new Vector2(rounded.X) - size;
        var coordQ2 = new Vector2(rounded.X);
        vertices.SDFVertexTypeRectangle(position, size, coordQ2, coordQ1, rounded.X);

        coordQ1 = new Vector2(rounded.Y) - size;
        coordQ2 = new Vector2(rounded.Y);
        vertices.SDFVertexTypeRectangle(position + new Vector2(size.X, 0f), size, new Vector2(coordQ1.X, coordQ2.Y),
            new Vector2(coordQ2.X, coordQ1.Y), rounded.Y);

        coordQ1 = new Vector2(rounded.Z) - size;
        coordQ2 = new Vector2(rounded.Z);
        vertices.SDFVertexTypeRectangle(position + new Vector2(0f, size.Y), size, new Vector2(coordQ2.X, coordQ1.Y),
            new Vector2(coordQ1.X, coordQ2.Y), rounded.Z);

        coordQ1 = new Vector2(rounded.W) - size;
        coordQ2 = new Vector2(rounded.W);
        vertices.SDFVertexTypeRectangle(position + size, size, coordQ1, coordQ2, rounded.W);

        GraphicsDevice.DrawUserPrimitives(0, vertices.ToArray(), 0, vertices.Count / 3);

        SpriteEffectPass.Apply();
    }
}