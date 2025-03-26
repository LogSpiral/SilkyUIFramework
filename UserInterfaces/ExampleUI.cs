namespace SilkyUIFramework.UserInterfaces;

[RegisterUI("Vanilla: Radial Hotbars", "SilkyUI: ExampleUI")]
public class ExampleUI : BasicBody
{
    public SUIDraggableView DraggableView { get; protected set; }

    protected override void OnInitialize()
    {
        FitWidth = true;
        FitHeight = true;
        SetGap(10f);

        SetLeft(0f, 0f, 0.5f);
        SetTop(0f, 0f, 0.5f);

        MainAlignment = MainAlignment.Center;
        CrossAlignment = CrossAlignment.Stretch;

        BorderColor = Color.Black;

        var backgroundColor = new Color(63, 65, 151);
        var borderColor = new Color(18, 18, 38);

        DraggableView = new SUIDraggableView(this)
        {
            LayoutType = LayoutType.Flexbox,
            FlexDirection = FlexDirection.Row,
            MainAlignment = MainAlignment.Start,
            CrossAlignment = CrossAlignment.Center,
            CrossContentAlignment = CrossContentAlignment.Center
        }.Join(this);
        DraggableView.SetHeight(50f);

        var textView1 = new UITextView()
        {
            Text = "Example UI 1",
            BackgroundColor = Color.White * 0.5f,
        }.Join(DraggableView);

        var textView3 = new UITextView()
        {
            Text = "Example UI 3",
            BackgroundColor = Color.White * 0.5f,
        }.Join(DraggableView);

        var editText = new SUIEditText()
        {
            BackgroundColor = backgroundColor * 0.25f,
            Border = 2f,
            BorderColor = borderColor,
        }.Join(this);
        editText.SetHeight(50f);

        var blockContainer = new SUIScrollView()
        {
            BackgroundColor = Color.Yellow * 0.25f,
            Padding = 2f,
            Mask = {
                Padding = 2f,
            BackgroundColor = Color.Black * 0.25f,
            },
            Container = {
                BackgroundColor = Color.Red * 0.25f,
                Padding = new Margin(12f),
                LayoutType = LayoutType.Flexbox,
                FlexWrap = true,
                FlexDirection = FlexDirection.Row,
                MainAlignment = MainAlignment.SpaceBetween,
                CrossAlignment = CrossAlignment.Start,
                //TemplateColumns = [..TemplateUnit.Repeat(5, 0f, 1f)],
                //TemplateRows = [..TemplateUnit.Repeat(3, 0f, 1f)],
            },
        }.Join(this);
        blockContainer.SetSize(500f, 300f);
        blockContainer.SetGap(10f);

        blockContainer.ScrollBar.OnUpdateStatus += _ =>
        {
            blockContainer.Container.MainAlignment = MainAlignment.SpaceBetween;
            blockContainer.ScrollBar.SetWidth(blockContainer.ScrollBar.HoverTimer.Lerp(4f, 40f));
        };

        var textView2 = new UITextView()
        {
            Text = "YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM YGCHXBM",
            FitWidth = false,
            WordWrap = true,
            Padding = 2,
        }.Join(blockContainer.Container);
        textView2.SetWidth(0f, 1f);

        for (int i = 0; i < 2000; i++)
        {
            // 创建并添加
            var block = new SUIItemSlot
            {
                ItemInteractive = true,
                BorderRadius = new Vector4(8f),
                Border = 2f,
                BorderColor = borderColor * 0.5f,
                BackgroundColor = backgroundColor * 0.25f,
            }.Join(blockContainer.Container);
            block.DisplayItemStack = true;
            block.Item = new Item(i + 1);
            block.Item.stack = block.Item.maxStack;
            block.SetSize(50f, 50f, 0f, 0f);
        }

        var container2 = new UIElementGroup
        {
            LayoutType = LayoutType.Flexbox,
            FlexDirection = FlexDirection.Row,
            MainAlignment = MainAlignment.SpaceEvenly,
            BackgroundColor = Color.Red * 0.25f,
            FitWidth = true,
            FitHeight = true,
            FlexGrow = 1f,
            OverflowHidden = true,
            Padding = 10f,
        }.Join(this);
        container2.SetWidth(500f);

        var box5 = new UIView
        { BackgroundColor = Color.White * 0.5f }.Join(container2);
        box5.SetSize(100f, 40f);

        var box6 = new UIView
        { BackgroundColor = Color.White * 0.5f }.Join(container2);
        box6.SetSize(100f, 100f);

        var box7 = new UIView
        { BackgroundColor = Color.White * 0.5f }.Join(container2);
        box7.SetSize(100f, 80f);

        var box8 = new UIView
        { BackgroundColor = Color.White * 0.5f }.Join(container2);
        box8.SetSize(100f, 100f);

        var container3 = new UIElementGroup
        {
            LayoutType = LayoutType.Flexbox,
            FlexDirection = FlexDirection.Row,
            MainAlignment = MainAlignment.SpaceBetween,
            CrossAlignment = CrossAlignment.End,
            BackgroundColor = Color.Black * 0.25f,
            FitWidth = true,
            FitHeight = true,
            FlexGrow = 1f,
            OverflowHidden = true,
            Padding = 10f,
        }.Join(this);
        container3.SetWidth(500f);

        var box9 = new UIView
        { BackgroundColor = Color.White * 0.5f }.Join(container3);
        box9.SetSize(100f, 100f);

        var box10 = new UIView
        { BackgroundColor = Color.White * 0.5f }.Join(container3);
        box10.SetSize(100f, 80f);

        var box11 = new UIView
        { BackgroundColor = Color.White * 0.5f }.Join(container3);
        box11.SetSize(100f, 100f);

        var box12 = new UIView
        { BackgroundColor = Color.White * 0.5f }.Join(container3);
        box12.SetSize(100f, 20f);
    }

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
        base.HandleDraw(gameTime, spriteBatch);
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {

        UseRenderTarget = true;
        Opacity = HoverTimer.Lerp(0.5f, 1f);
        base.Draw(gameTime, spriteBatch);
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        if (IsMouseHovering)
        {
            // 锁定滚动条
            PlayerInput.LockVanillaMouseScroll("SilkyUIFramework");
            Main.LocalPlayer.mouseInterface = true;
        }

        base.UpdateStatus(gameTime);
    }
}