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

        child1.WidthUnit = new Unit(0, 0.25f);
        child1.HeightUnit = new Unit(0, 0.25f);

        child2.WidthUnit = new Unit(0, 0.25f);
        child2.HeightUnit = new Unit(0, 0.25f);

        child3.SetWidth(200f, 0f);
        child3.SetHeight(200f, 0f);
        child2.AppendChildren(child3);

        child4.SetWidth(200f, 0f);
        child4.SetHeight(200f, 0f);
        child2.AppendChildren(child4);

        for (int i = 0; i < 200; i++)
        {
            var el = new Other();
            el.AutoSize();
            el.Padding = new Margin(2);
            el.Gap = new Vector2(2);
            el.LayoutDirection = LayoutDirection.Column;
            RootElement.AppendChild(el);

            for (int e = 0; e < 150; e++)
            {
                var el2 = new Other();
                el2.SetSize(2f, 2f);
                el.BackgroundColor = Color.Red * 0.25f;
                el.AppendChild(el2);
            }
        }

        // RootElement.AppendChildren(child1, child2);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        RootElement.Gap = new Vector2(2f);
        RootElement.Padding = new Margin(2f);
        RootElement.LayoutDirection = LayoutDirection.Row;

        RootElement.SetLeft(0f, 0f);
        RootElement.SetTop(0f, 0f);

        // RootElement.SetWidth(Main.rand.NextFloat(0f, 0.01f), 0.5f);
        RootElement.SetHeight(0f, 0.5f);

        RootElement.AutoWidth(false);
        RootElement.AutoHeight(false);

        child1.SetWidth(4f, 0f);
        child1.SetHeight(500f, 0f);

        child2.SetWidth(10f, 0f);
        child2.SetHeight(100f, 0f);
        child2.LayoutDirection = LayoutDirection.Column;

        child2.Padding = new Margin(2f);
        child2.Gap = new Vector2(0f);
        child2.AutoSize();

        child3.SetWidth(4f, 0f);
        child3.SetHeight(100f, 0f);

        child4.SetWidth(4);
        child4.SetHeight(100);

        child1.BackgroundColor = Color.Blue * 0.25f;
        child2.BackgroundColor = Color.White * 0.25f;
        child3.BackgroundColor = Color.Yellow * 0.25f;
        child4.BackgroundColor = Color.Red * 0.25f;

        RootElement.LayoutDirtyCheck();
        RootElement.PositionDirtyCheck();
        RootElement.Update(gameTime);
    }

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
        RootElement.Draw(Main.gameTimeCache, Main.spriteBatch);
        return true;
    }
}