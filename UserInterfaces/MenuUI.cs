using log4net;

namespace SilkyUIFramework.UserInterfaces;

public delegate void MenuClickCallback(string name, int index);


public interface IMouseMenu
{
    void OpenMenu(Vector2 mousePosition, List<string> contents, MenuClickCallback menuClickCallback);
}

#if DEBUG && true

[RegisterGlobalUI("MenuUI", 1)]
public class MenuUI(ILog logger) : BasicBody, IMouseMenu
{
    private ILog Logger { get; } = logger;

    private SUIScrollView ScrollView { get; set; }

    protected override void OnInitialize()
    {
        SetLeft(alignment: 0.5f);
        SetTop(alignment: 0.5f);

        FitWidth = true;
        FitHeight = true;

        MaxHeight = new Dimension(300f);

        BorderRadius = new Vector4(8f);

        ScrollView = new SUIScrollView(Direction.Vertical)
        {
            FitWidth = true,
            FitHeight = true,
            Padding = new Margin(4f),
            Width = new Dimension(0f, 1f),
            Height = new Dimension(0f, 1f),
            Gap = new Size(4f),
            Mask = {
                FitWidth = true,
                FitHeight = true,
            },
            Container = {
                FlexWrap = true,
                FitWidth = true,
                FitHeight = true,
                FlexDirection = FlexDirection.Column,
                MainAlignment = MainAlignment.Start,
                CrossContentAlignment = CrossContentAlignment.Stretch,
                CrossAlignment = CrossAlignment.Stretch,
                Gap = new Size(4f),
            }
        }.Join(this);

        for (int i = 0; i < 20; i++)
        {
            var text = new UITextView()
            {
                Padding = new Margin(8f, 4f),
                Text = $"Mouse Menu Item {i + 1}",
                BorderRadius = new Vector4(4f),
            }.Join(ScrollView.Container);
            text.OnUpdate += gt =>
            {
                text.BackgroundColor = Color.Black * text.HoverTimer.Lerp(0.25f, 0.5f);
            };
        }
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        ScrollView.Mask.MaxHeight = new Dimension(300f, 0f);

        BorderColor = SUIColor.Border * 0.5f;

        BackgroundColor = SUIColor.Background * 0.25f;

    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        RectangleRender.ShadowColor = SUIColor.Border * 0.25f; // HoverTimer.Lerp(0f, 0.25f);
        RectangleRender.ShadowSize = 2f; // HoverTimer.Lerp(0f, 20f);
        RectangleRender.ShadowBlurSize = 2f; // HoverTimer.Lerp(0f, 20f);

        base.UpdateStatus(gameTime);
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (BlurMakeSystem.BlurAvailable && !Main.gameMenu)
        {
            if (BlurMakeSystem.SingleBlur)
            {
                var batch = Main.spriteBatch;
                batch.End();
                BlurMakeSystem.KawaseBlur();
                batch.Begin(0, null, SamplerState.PointClamp, null, SilkyUI.RasterizerStateForOverflowHidden, null, SilkyUI.TransformMatrix);
            }

            SDFRectangle.SampleVersion(BlurMakeSystem.BlurRenderTarget,
                Bounds.Position * Main.UIScale, Bounds.Size * Main.UIScale, BorderRadius * Main.UIScale, Matrix.Identity);
        }

        base.Draw(gameTime, spriteBatch);
    }

    private MenuClickCallback MenuClickCallback { get; set; }

    public void OpenMenu(Vector2 mousePosition, List<string> contents, MenuClickCallback menuClickCallback)
    {
        SetLeft(mousePosition.X, alignment: 0f);
        SetTop(mousePosition.Y, alignment: 0f);

        GenerateMenuItems(contents);
        MenuClickCallback = menuClickCallback;
    }

    private void GenerateMenuItems(List<string> contents)
    {
        ScrollView.Container.RemoveAllChildren();

        for (int i = 0; i < contents.Count; i++)
        {
            var index = i;
            var text = new UITextView()
            {
                Text = contents[i],
                TextScale = 0.8f,
                Padding = new Margin(8f, 4f),
                BorderRadius = new Vector4(4f),
            }.Join(ScrollView.Container);
            text.LeftMouseDown += (sender, evt) =>
            {
                MenuClickCallback?.Invoke(contents[i], index);
            };
            text.OnUpdate += gt =>
            {
                text.BackgroundColor = Color.Black * text.HoverTimer.Lerp(0.25f, 0.5f);
            };
        }
    }
}

#endif