namespace SilkyUIFramework.BasicElements;

#if false

public class UIElementGroupSystem : ModSystem
{
    private readonly UIElementGroup Root = new();
    public UITextView TextView;

    public override void Load()
    {
        Root.SetWidth(0f, 1f);
        Root.SetHeight(0f, 1f);

        TextView = new UITextView
        {
            FitWidth = true,
            Text = "Hello World! Hello World! Hello World! Hello World!",
        };
        Root.AppendChild(TextView);

        for (var i = 0; i < 8; i++)
        {
            var col = new UIElementGroup
            {
                FitHeight = true,
            };
            col.SetWidth(100f);
            col.SetGap(4, 4);
            col.Padding = new Margin(4f);
            col.FlexDirection = FlexDirection.Column;
            Root.AppendChild(col);

            for (var e = 0; e < 5; e++)
            {
                var block = new UIView();
                block.SetSize(100f, 50f);
                col.AppendChild(block);
            }
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        Root.BoxSizing = BoxSizing.Content;
        Root.Padding = new Margin(10f);
        Root.FlexDirection = FlexDirection.Row;
        Root.SetGap(10f, 10f);
        Root.CrossAlignment = CrossAlignment.Stretch;
        Root.CrossContentAlignment = CrossContentAlignment.Stretch;

        TextView.Text = "Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World!";
        TextView.FitWidth = true;
        TextView.FitHeight = true;
        TextView.WordWrap = true;
        TextView.FlexGrow = 10f;
        TextView.SetMaxWidth(300f);

        // 子元素
        var groups = Root.Children.OfType<UIElementGroup>().ToArray();

        for (var i = 0; i < groups.Length; i++)
        {
            var column = groups[i];
            column.FlexGrow = 1f;
            column.FlexShrink = 1f;
            column.FitWidth = true;
            column.FitHeight = false;
            column.CrossAlignment = CrossAlignment.Start;
            column.MainAlignment = MainAlignment.Start;
            column.FlexWrap = false;

            if (i == 0)
            {
                column.FitWidth = true;
                column.FlexDirection = FlexDirection.Row;
                column.MainAlignment = MainAlignment.Center;
                column.CrossAlignment = CrossAlignment.Stretch;
                column.CrossContentAlignment = CrossContentAlignment.Stretch;
                column.SetWidth(500f);
                column.SetHeight(500f);
            }
            else
            {
                column.FlexDirection = FlexDirection.Column;
                column.CrossAlignment = CrossAlignment.Center;
                column.CrossContentAlignment = CrossContentAlignment.Center;
                column.SetWidth(500f);
                column.SetHeight(500f);
            }

            column.SetGap(2f, 2f);
            column.Padding = new Margin(10f);

            for (int j = 0; j < column.Children.Count; j++)
            {
                var item = column.Children[j];
                item.FitWidth = false;
                item.FitHeight = false;
                if (j == 0)
                {
                    item.FlexGrow = 1f;
                    item.FlexShrink = 1f;
                    item.SetWidth(40f, 0f);
                    item.SetHeight(1f, 0f);
                }
                else
                {
                    item.FlexGrow = 1f;
                    item.FlexShrink = 1f;
                    item.SetWidth(40f, 0f);
                    item.SetHeight(1f, 0f);
                }
            }
        }

        Root.Gap = new Size(2, 2);
        Root.Positioning = Positioning.Relative;
        Root.SetLeft(Main.rand.NextFloat(0.001f), 0f, 0.5f);
        Root.SetTop(0f, 0f, 0.5f);

        Root.SetWidth(Main.rand.NextFloat(0.001f), 0.75f);
        Root.SetHeight(0f, 0.75f);

        Root.FitWidth = true;
        Root.FitHeight = true;

        // 更新 UI 的各种状态，比如动画
        Root.HandleUpdateStatus(gameTime);
        // Bounds and Layout
        Root.UpdateLayout();
        // 位置脏标记检测
        Root.UpdatePosition();
        // UI 中的普通更新
        Root.HandleUpdate(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        layers.Insert(0, new OtherLayer(Root, "TEST UI", InterfaceScaleType.UI));
    }
}

public class OtherLayer(UIElementGroup root, string name, InterfaceScaleType scaleType)
    : GameInterfaceLayer(name, scaleType)
{
    public readonly UIElementGroup Root = root;

    public override bool DrawSelf()
    {
        Root.HandleDraw(Main.gameTimeCache, Main.spriteBatch);
        return true;
    }
}

#endif