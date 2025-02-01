namespace SilkyUIFramework.Graphics2D;

public struct SDFGraphicsVertexType(Vector2 position, Vector2 coord, float cornerRadius) : IVertexType
{
    public Vector2 Position = position;
    public Vector2 Coord = coord;
    public float CornerRadius = cornerRadius;

    private static readonly VertexDeclaration VertexDeclaration = new([
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(16, VertexElementFormat.Single, VertexElementUsage.Color, 0)
        ]);

    readonly VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}