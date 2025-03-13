namespace SilkyUIFramework.Graphics2D;

public static class SDFGraphics
{
    private struct VertexPosCoord(Vector2 pos, Vector2 coord) : IVertexType
    {
        private static readonly VertexDeclaration _vertexDeclaration = new(
        [
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        ]);

        public Vector2 Pos = pos;
        public Vector2 Coord = coord;

        public readonly VertexDeclaration VertexDeclaration => _vertexDeclaration;
    }

    /// <summary>
    /// 生成矩形顶点
    /// </summary>
    private static VertexPosCoord[] GenerateRectangle(Vector2 pos, Vector2 size)
    {
        var vertices = new VertexPosCoord[6];
        vertices[0] = new VertexPosCoord(pos, new Vector2(0, 0));
        vertices[1] = new VertexPosCoord(pos + new Vector2(size.X, 0), new Vector2(size.X, 0));
        vertices[2] = new VertexPosCoord(pos + new Vector2(0, size.Y), new Vector2(0, size.Y));
        vertices[3] = new VertexPosCoord(pos + new Vector2(0, size.Y), new Vector2(0, size.Y));
        vertices[4] = new VertexPosCoord(pos + new Vector2(size.X, 0), new Vector2(size.X, 0));
        vertices[5] = new VertexPosCoord(pos + size, size);
        return vertices;
    }

    /// <summary>
    /// 根据位置和尺寸生成由六个顶点组成的矩形并绘制，然后恢复
    /// </summary>
    private static void DrawRectanglePrimitives(Vector2 position, Vector2 size)
    {
        var vertices = GenerateRectangle(position, size);
        Main.graphics.GraphicsDevice.DrawUserPrimitives(0, vertices, 0, vertices.Length / 3);
        Main.spriteBatch.spriteEffectPass.Apply();
    }

    /// 绘制叉号
    public static void HasBorderCross(Vector2 pos, float size, float round, Color backgroundColor,
        float border, Color borderColor, Matrix matrix)
    {
        MatrixHelper.Transform2SDFMatrix(ref matrix);
        Effect effect = ModAsset.SDFGraphics.Value;
        effect.Parameters["uTransform"].SetValue(matrix);
        effect.Parameters["uSizeOver2"].SetValue(new Vector2(size) / 2f);
        effect.Parameters["uBorder"].SetValue(border);
        effect.Parameters["uRound"].SetValue(round);
        effect.Parameters["uBorderColor"].SetValue(borderColor.ToVector4());
        effect.Parameters["uBackgroundColor"].SetValue(backgroundColor.ToVector4());
        const float root2Over2 = 1.414213562373f / 2f;
        effect.Parameters["uSmoothstepRange"].SetValue(new Vector2(-root2Over2, root2Over2) / Main.UIScale);
        effect.CurrentTechnique.Passes["HasBorderCross"].Apply();
        DrawRectanglePrimitives(pos, new Vector2(size));
    }

    public static void HasBorderRound(Vector2 pos, float size, Color background,
        float border, Color borderColor, Matrix matrix)
    {
        MatrixHelper.Transform2SDFMatrix(ref matrix);
        Effect effect = ModAsset.SDFGraphics.Value;
        effect.Parameters["uTransform"].SetValue(matrix);
        effect.Parameters["uBackgroundColor"].SetValue(background.ToVector4());
        effect.Parameters["uBorder"].SetValue(border);
        effect.Parameters["uBorderColor"].SetValue(borderColor.ToVector4());
        effect.CurrentTechnique.Passes["HasBorderRound"].Apply();
        DrawRectanglePrimitives(pos, new Vector2(size));
    }

    public static void NoBorderRound(Vector2 pos, float size, Color background, Matrix matrix)
    {
        MatrixHelper.Transform2SDFMatrix(ref matrix);
        Effect effect = ModAsset.SDFGraphics.Value;
        effect.Parameters["uTransform"].SetValue(matrix);
        effect.Parameters["uSizeOver2"].SetValue(new Vector2(size) / 2f);
        effect.Parameters["uBackgroundColor"].SetValue(background.ToVector4());
        effect.CurrentTechnique.Passes["NoBorderRound"].Apply();
        DrawRectanglePrimitives(pos, new Vector2(size));
    }

    /// <summary>
    /// 绘制一条线，无边框
    /// </summary>
    public static void NoBorderLine(Vector2 start, Vector2 end, float width, Color background, Matrix matrix)
    {
        MatrixHelper.Transform2SDFMatrix(ref matrix);
        Vector2 min = Vector2.Min(start, end);
        Vector2 max = Vector2.Max(start, end);
        Vector2 size = max - min + new Vector2(width * 2);

        start += new Vector2(width) - min;
        end += new Vector2(width) - min;
        Effect effect = ModAsset.SDFGraphics.Value;
        effect.Parameters["uTransform"].SetValue(matrix);
        effect.Parameters["uStart"].SetValue(start);
        effect.Parameters["uEnd"].SetValue(end);
        effect.Parameters["uLineWidth"].SetValue(width);
        effect.Parameters["uBackgroundColor"].SetValue(background.ToVector4());
        effect.CurrentTechnique.Passes["NoBorderLine"].Apply();
        DrawRectanglePrimitives(min - new Vector2(width), size);
    }

    /// <summary>
    /// 绘制一条线，有边框
    /// </summary>
    public static void HasBorderLine(Vector2 start, Vector2 end, float width, Color background,
        float border, Color borderColor, Matrix matrix)
    {
        MatrixHelper.Transform2SDFMatrix(ref matrix);
        Vector2 min = Vector2.Min(start, end);
        Vector2 max = Vector2.Max(start, end);
        Vector2 size = max - min + new Vector2(width * 2);

        start += new Vector2(width) - min;
        end += new Vector2(width) - min;
        Effect effect = ModAsset.SDFGraphics.Value;
        effect.Parameters["uTransform"].SetValue(matrix);
        effect.Parameters["uStart"].SetValue(start);
        effect.Parameters["uEnd"].SetValue(end);
        effect.Parameters["uLineWidth"].SetValue(width);
        effect.Parameters["uBackgroundColor"].SetValue(background.ToVector4());
        effect.Parameters["uBorder"].SetValue(border);
        effect.Parameters["uBorderColor"].SetValue(borderColor.ToVector4());
        effect.CurrentTechnique.Passes["HasBorderLine"].Apply();
        DrawRectanglePrimitives(min - new Vector2(width), size);
    }
}