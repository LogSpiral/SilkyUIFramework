using SilkyUIFramework.Animation;

namespace SilkyUIFramework.UserInterfaces;

// [AutoloadUI("Vanilla: Radial Hotbars", "SilkyUI: ExampleUI")]
public class ExampleUI : BasicBody
{
    public SUIDraggableView MainPanel { get; private set; }

    private static readonly int[] RoshanBadges =
    [
        1, 4, 6, 7, 10, 489, 490, 491, 2998, 1, 4, 6, 7, 10, 489, 490, 491, 2998,
    ];

    public override void OnInitialize()
    {
        var backgroundColor = new Color(63, 65, 151);
        var borderColor = new Color(18, 18, 38);

        // 面板
        MainPanel = new SUIDraggableView
        {
            Display = Display.Flexbox,
            Gap = new Vector2(10f),
            FlexWrap = false,
            LayoutDirection = LayoutDirection.Column,
            MainAlignment = MainAlignment.Start,
            FinallyDrawBorder = true,
            DragIncrement = new Vector2(5f)
        }.Join(this);
        MainPanel.SetHeight(500f);
        MainPanel.SetPadding(12f);

        var blockContainer = new SUIScrollView()
        {
            OverflowHidden = true,
            CornerRadius = new Vector4(12f),
            Border = 2,
            BorderColor = Color.Black * 0.5f,
            BgColor = Color.Black * 0.2f,
            Container = {
                LayoutDirection = LayoutDirection.Row,
                Display = Display.Grid,
                CrossAlignment = CrossAlignment.Start,
                TemplateColumns = [..TemplateUnit.Repeat(5, 0f, 1f)],
                TemplateRows = [..TemplateUnit.Repeat(3, 0f, 1f)],
            },
        }.Join(MainPanel);
        blockContainer.SetSize(500f, 300f);
        blockContainer.SetPadding(12f);

        for (int i = 0; i < 35; i++)
        {
            // 创建并添加
            var block = new SUIItemSlot
            {
                ItemInteractive = false,
                CornerRadius = new Vector4(8f),
                Border = 2f,
                BorderColor = borderColor * 0.5f,
                BgColor = backgroundColor * 0.25f,
            }.Join(blockContainer);
            block.DisplayItemStack = true;
            block.Item = new Item(i + 1);
            block.Item.stack = block.Item.maxStack;
            block.SetSize(0f, 0f, 1f, 1f);
        }

        var container2 = new View
        {
            Invalidate = true,
            Display = Display.Flexbox,
            LayoutDirection = LayoutDirection.Row,
            MainAlignment = MainAlignment.SpaceEvenly,
            BgColor = Color.Red * 0.25f,
            // Weight 测试
            SpecifyHeight = true,
            FlexWeight = { Enable = true, Value = 1 },
            OverflowHidden = true,
        }.Join(MainPanel);
        container2.SetWidth(500f);

        var box5 = new View
        { BgColor = Color.White * 0.5f }.Join(container2);
        box5.SetSize(100f, 40f);

        var box6 = new View
        { BgColor = Color.White * 0.5f }.Join(container2);
        box6.SetSize(100f, 100f);

        var box7 = new View
        { BgColor = Color.White * 0.5f }.Join(container2);
        box7.SetSize(100f, 80f);

        var box8 = new View
        { BgColor = Color.White * 0.5f }.Join(container2);
        box8.SetSize(100f, 100f);

        var container3 = new View
        {
            Display = Display.Flexbox,
            LayoutDirection = LayoutDirection.Row,
            MainAlignment = MainAlignment.SpaceBetween,
            CrossAlignment = CrossAlignment.End,
            BgColor = Color.Black * 0.25f,
            SpecifyHeight = true,
            FlexWeight = { Enable = true, Value = 2 },
            OverflowHidden = true,
        }.Join(MainPanel);
        container3.SetWidth(500f);

        var box9 = new View
        { BgColor = Color.White * 0.5f }.Join(container3);
        box9.SetSize(100f, 100f);

        var box10 = new View
        { BgColor = Color.White * 0.5f }.Join(container3);
        box10.SetSize(100f, 80f);

        var box11 = new View
        { BgColor = Color.White * 0.5f }.Join(container3);
        box11.SetSize(100f, 100f);

        var box12 = new View
        { BgColor = Color.White * 0.5f }.Join(container3);
        box12.SetSize(100f, 20f);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        UseRenderTarget = false;
        HoverTimer.Speed = 10f;
        Opacity = MainPanel.HoverTimer.Lerp(0.75f, 1f);
        base.Draw(spriteBatch);

        // var nineGrid = new NineGrid();
        // nineGrid.Draw(spriteBatch, MainPanel._dimensions.Position(), MainPanel._dimensions.Size(), Color.White);
    }

    /// <summary>
    /// 使用矩阵缩放动画
    /// </summary>
    public static void ScalingAnimationUsingMatrix(View view, AnimationTimer timer, float target)
    {
        if (view == null) return;
        var scale = 1f + (target = timer.Lerp(0f, target));
        var center = view.GetDimensions().Center();
        var offset = center * target;
        view.TransformMatrix =
            Matrix.CreateScale(scale, scale, 1f) *
            Matrix.CreateTranslation(-offset.X, -offset.Y, 0);
    }
}