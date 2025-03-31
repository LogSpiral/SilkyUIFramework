namespace SilkyUIFramework.Extensions;

public static class SDFGraphicsVertexTypeExtensions
{
    public static void SDFVertexTypeRectangle(this List<SDFGraphicsVertexType> vertices,
        Vector2 pos, Vector2 size, Vector2 coordQ1, Vector2 coordQ2, float borderRadius)
    {
        var leftTop = new SDFGraphicsVertexType(pos, Vector2.Zero, coordQ1, borderRadius);

        var rightTop =
            new SDFGraphicsVertexType(pos + new Vector2(size.X, 0f),
            Vector2.Zero, new Vector2(coordQ2.X, coordQ1.Y), borderRadius);

        var leftBottom =
            new SDFGraphicsVertexType(pos + new Vector2(0f, size.Y),
            Vector2.Zero, new Vector2(coordQ1.X, coordQ2.Y), borderRadius);

        var rightBottom =
            new SDFGraphicsVertexType(pos + size, Vector2.Zero, coordQ2, borderRadius);

        // 三角形 ↖️
        vertices.Add(leftTop);
        vertices.Add(rightTop);
        vertices.Add(leftBottom);

        // 三角形 ↘️
        vertices.Add(leftBottom);
        vertices.Add(rightTop);
        vertices.Add(rightBottom);
    }
}