namespace SilkyUIFramework;

public class OtherSystem : ModSystem
{
    public readonly Other RootElement = new();

    public readonly Other child1 = new();
    public readonly Other child2 = new();

    public readonly Other child3 = new();
    public readonly Other child4 = new();

    public override void Load()
    {
        RootElement.WidthUnit = new Unit(0, 1);
        RootElement.HeightUnit = new Unit(0, 1);

        child1.SetSize(0f, 0f, 1f, 1f);
        child2.SetSize(0f, 0f, 1f, 1f);

        child3.SetWidth(200f, 0f);
        child3.SetHeight(200f, 0f);
        child2.AppendChildren(child3);

        child4.SetWidth(200f, 0f);
        child4.SetHeight(200f, 0f);
        child2.AppendChildren(child4);

        child1.BackgroundColor = Color.Blue * 0.1f;
        child2.BackgroundColor = Color.White * 0.1f;
        child3.BackgroundColor = Color.Yellow * 0.1f;
        child4.BackgroundColor = Color.Red * 0.1f;

        for (int i = 0; i < 10; i++)
        {
            var col = new Other();
            col.SetWidth(100f);
            col.AutoHeight();
            col.Gap = new Vector2(2);
            col.LayoutDirection = LayoutDirection.Column;
            RootElement.AppendChild(col);

            for (int e = 0; e < 10; e++)
            {
                var el = new Other();
                el.SetSize(10f, 10f);
                col.BackgroundColor = Color.Red * 0.1f;
                col.AppendChild(el);
            }
        }

        // RootElement.AppendChildren(child1, child2);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        RootElement.Gap = new Vector2(2f);
        RootElement.Padding = new Margin(2f);
        RootElement.LayoutDirection = LayoutDirection.Row;

        RootElement.SetLeft(0f, 0.25f);
        RootElement.SetTop(0f, 0.25f);

        RootElement.SetWidth(Main.rand.NextFloat(0.001f), 0.5f);
        RootElement.SetHeight(0f, 0.5f);

        RootElement.AutoWidth(false);
        RootElement.AutoHeight(false);

        // child1.SetWidth(4f, 0f);
        // child1.SetHeight(500f, 0f);

        // child2.SetWidth(10f, 0f);
        // child2.SetHeight(100f, 0f);
        // child2.LayoutDirection = LayoutDirection.Column;

        // child2.Padding = new Margin(2f);
        // child2.Gap = new Vector2(0f);
        // child2.AutoSize();

        // child3.SetWidth(4f, 0f);
        // child3.SetHeight(100f, 0f);

        // child4.SetWidth(4);
        // child4.SetHeight(100);

        RootElement.LayoutDirtyCheck();
        RootElement.PositionDirtyCheck();
        RootElement.Update(gameTime);
    }

    public static readonly List<ColorBlockVertextType> Vertices = [];

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var OtherLayer = new OtherLayer(RootElement, "other ui", InterfaceScaleType.UI);
        layers.Insert(0, OtherLayer);
    }
}

public class OtherLayer(Other rootElement, string name, InterfaceScaleType scaleType) : GameInterfaceLayer(name, scaleType)
{
    public readonly Other RootElement = rootElement;

    public override bool DrawSelf()
    {
        OtherSystem.Vertices.Clear();

        RootElement.Draw(Main.gameTimeCache, Main.spriteBatch);

        var device = Main.graphics.GraphicsDevice;

        var effect = ModAsset.ColorBlock.Value;
        var matrix = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width / 2, device.Viewport.Height / 2, 0, 0, 1);
        effect.Parameters["MatrixTransform"].SetValue(matrix);
        effect.CurrentTechnique.Passes[0].Apply();

        _vertices = [.. OtherSystem.Vertices];
        device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length / 3);

        return true;
    }

    private ColorBlockVertextType[] _vertices = null;
}