using SilkyUIFramework.Graphics2D;

namespace SilkyUIFramework.Extensions;

public static class SDFGraphicsVertexTypeExtensions
{
    public static void SDFVertexTypeRectangle(this List<SDFGraphicsVertexType> vertices,
        Vector2 pos, Vector2 size, Vector2 coordQ1, Vector2 coordQ2, float rounded)
    {
        var leftTop =
            new SDFGraphicsVertexType(pos, coordQ1, rounded);

        var rightTop =
            new SDFGraphicsVertexType(pos + new Vector2(size.X, 0f),
                new Vector2(coordQ2.X, coordQ1.Y), rounded);

        var leftBottom =
            new SDFGraphicsVertexType(pos + new Vector2(0f, size.Y),
                new Vector2(coordQ1.X, coordQ2.Y), rounded);

        var rightBottom =
            new SDFGraphicsVertexType(pos + size, coordQ2, rounded);

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