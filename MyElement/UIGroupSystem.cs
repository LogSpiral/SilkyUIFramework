using SilkyUIFramework.Core;

namespace SilkyUIFramework.MyElement;

public class UIGroupSystem : ModSystem
{
    private readonly ViewGroup Root = new();

    public override void Load()
    {
        Root.SetWidth(0f, 1f);
        Root.SetHeight(0f, 1f);

        for (var i = 0; i < 4; i++)
        {
            var col = new ViewGroup
            {
                AutomaticHeight = true,
            };
            col.SetWidth(100f);
            col.SetGap(4, 4);
            col.Padding = new(4f);
            col.LayoutDirection = LayoutDirection.Column;
            Root.AppendChild(col);

            for (var e = 0; e < 4; e++)
            {
                var block = new UIView();
                block.SetWidth(100f);
                block.SetHeight(50f);
                col.AppendChild(block);
            }
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        Root.BoxSizing = BoxSizing.Content;
        Root.Padding = new Margin(10f);
        Root.LayoutDirection = LayoutDirection.Row;
        Root.SetGap(10f, 10f);
        Root.CrossAlignment = CrossAlignment.Stretch;
        Root.CrossContentAlignment = CrossContentAlignment.Stretch;

        // 子元素
        var groups = Root.ReadOnlyChildren.OfType<ViewGroup>().ToList();

        for (var i = 0; i < groups.Count; i++)
        {
            var column = groups[i];
            column.FlexGrow = 1f;
            column.FlexShrink = 1f;
            column.CrossAlignment = CrossAlignment.Start;
            column.MainAlignment = MainAlignment.Start;
            if (i == 0)
            {
                column.FlexWrap = true;
                column.LayoutDirection = LayoutDirection.Column;
                column.MainAlignment = MainAlignment.Center;
                column.CrossAlignment = CrossAlignment.Stretch;
                column.CrossContentAlignment = CrossContentAlignment.Stretch;
                column.FlexGrow = 1f;
                column.FlexShrink = 1f;
                column.AutomaticWidth = false;
                column.AutomaticHeight = false;
                column.SetWidth(300f);
                column.SetHeight(800f);
            }
            else
            {
                column.AutomaticWidth = true;
                column.AutomaticHeight = true;
            }
            column.SetGap(10f, 10f);
            column.Padding = new Margin(10f);

            foreach (var item in column.ReadOnlyChildren)
            {
                item.FlexGrow = 1f;
                item.FlexShrink = 1f;
                item.SetWidth(100f);
                item.SetHeight(100f);
            }
        }

        Root.Positioning = Positioning.Relative;
        Root.SetLeft(Main.rand.NextFloat(0.001f), 0f, 0.5f);
        Root.SetTop(0f, 0f, 0.5f);

        Root.SetWidth(Main.rand.NextFloat(0.001f), 0.5f);
        Root.SetHeight(0f, 0.5f);

        Root.AutomaticWidth = true;
        Root.AutomaticHeight = true;

        // 更新 UI 的各种状态，比如动画
        Root.HandleUpdateStatus(gameTime);
        // Bounds and Layout
        Root.UpdateBounds();
        // 位置脏标记检测
        Root.UpdatePosition();
        // UI 中的普通更新
        Root.HandleUpdate(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var otherLayer = new OtherLayer(Root, "other ui", InterfaceScaleType.UI);
        layers.Insert(0, otherLayer);
    }
}

public class OtherLayer(ViewGroup rootElement, string name, InterfaceScaleType scaleType)
    : GameInterfaceLayer(name, scaleType)
{
    public readonly ViewGroup RootElement = rootElement;

    public override bool DrawSelf()
    {
        RootElement.HandleDraw(Main.gameTimeCache, Main.spriteBatch);

        return true;
    }
}