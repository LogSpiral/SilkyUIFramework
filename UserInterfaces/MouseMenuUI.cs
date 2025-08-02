using log4net;
using SilkyUIFramework.Animation;

namespace SilkyUIFramework.UserInterfaces;

public delegate bool MouseMenuCallback(string name, int index);

public interface IMouseMenu
{
    void OpenMenu(MouseAnchor mouseAnchor, Vector2 mousePosition, List<string> contents, MouseMenuCallback callback);
}

public enum MouseAnchor
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

[RegisterGlobalUI("MenuUI", 1)]
public class MouseMenuUI(ILog logger) : BasicBody, IMouseMenu
{
    private ILog Logger { get; } = logger;

    public static bool ShowUI { get; set; }
    public override bool Enabled
    {
        get
        {
            if (ShowUI) return true;
            return !SwitchTimer.IsReverseCompleted;
        }
        set => ShowUI = value;
    }
    public override bool IsInteractable => SwitchTimer.IsForward;

    private UIElementGroup MenuContainer { get; set; }
    private SUIScrollView ScrollView { get; set; }

    protected override void OnInitialize()
    {
        Enabled = false;

        SetSize(0f, 0f, 1f, 1f);

        SetLeft(0f, 0f, 0f);
        SetTop(0f, 0f, 0f);

        Border = 0;
        BorderColor = Color.Transparent;
        BackgroundColor = Color.Transparent;

        MenuContainer = new UIElementGroup()
        {
            FitWidth = true,
            FitHeight = true,
            Border = 2,
            BorderRadius = new Vector4(8f),
            BorderColor = SUIColor.Border * 0.5f,
            BackgroundColor = SUIColor.Background * 0.5f,
        }.Join(this);

        ScrollView = new SUIScrollView(Direction.Vertical)
        {
            FitWidth = true,
            FitHeight = true,
            Padding = new Margin(4f),
            Width = new Dimension(0f, 1f),
            Height = new Dimension(0f, 1f),
            Gap = new Size(4f),
            Mask = {
                MaxHeight = new Dimension(300f, 0f),
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
        }.Join(MenuContainer);
    }

    public override void OnLeftMouseDown(UIMouseEvent evt)
    {
        if (evt.Source == this) Enabled = false;

        base.OnLeftMouseDown(evt);
    }

    public override void OnRightMouseDown(UIMouseEvent evt)
    {
        //if (evt.Source == this) Enabled = false;

        base.OnRightMouseDown(evt);
    }

    protected override void Update(GameTime gameTime)
    {
        MenuContainer.BackgroundColor = SUIColor.Background * 0.75f;
        base.Update(gameTime);
    }

    public readonly AnimationTimer SwitchTimer = new(3);

    protected override void UpdateStatus(GameTime gameTime)
    {
        if (ShowUI) SwitchTimer.StartUpdate();
        else SwitchTimer.StartReverseUpdate();

        SwitchTimer.Update(gameTime);

        UseRenderTarget = SwitchTimer.IsUpdating;
        Opacity = SwitchTimer.Lerp(0f, 1f);

        var bounds = MenuContainer.Bounds;
        //var center = bounds.Center * Main.UIScale;
        RenderTargetMatrix = Matrix.CreateTranslation(0, SwitchTimer.Lerp(10f, 0), 0);
        //Matrix.CreateTranslation(-center.X, -center.Y, 0) *
        //Matrix.CreateScale(SwitchTimer.Lerp(0.95f, 1f), SwitchTimer.Lerp(0.95f, 1f), 1) *
        //Matrix.CreateTranslation(center.X, center.Y, 0);

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
                MenuContainer.Bounds.Position * Main.UIScale, MenuContainer.Bounds.Size * Main.UIScale, MenuContainer.BorderRadius * Main.UIScale, Matrix.Identity);
        }

        base.Draw(gameTime, spriteBatch);
    }

    public void OpenMenu(MouseAnchor mouseAnchor, Vector2 mousePosition, List<string> contents, MouseMenuCallback callback)
    {
        Enabled = true;

        switch (mouseAnchor)
        {
            case MouseAnchor.TopLeft:
            {
                MenuContainer.SetLeft(mousePosition.X, 0, 0);
                MenuContainer.SetTop(mousePosition.Y, 0, 0);
                break;
            }
            case MouseAnchor.TopRight:
            {
                MenuContainer.SetLeft(mousePosition.X, -1f, 1f);
                MenuContainer.SetTop(mousePosition.Y, 0, 0);
                break;
            }
            case MouseAnchor.BottomLeft:
            {
                MenuContainer.SetLeft(mousePosition.X, 0, 0);
                MenuContainer.SetTop(mousePosition.Y, -1f, 1f);
                break;
            }
            case MouseAnchor.BottomRight:
            {
                MenuContainer.SetLeft(mousePosition.X, -1f, 1f);
                MenuContainer.SetTop(mousePosition.Y, -1f, 1f);
                break;
            }
        }

        ScrollView.Container.RemoveAllChildren();

        for (int i = 0; i < contents.Count; i++)
        {
            var text = new MouseMenuItem(contents[i], i).Join(ScrollView.Container);
            text.MouseMenuCallback = callback;
        }
    }
}
