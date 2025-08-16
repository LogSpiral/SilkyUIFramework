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

#if true

[RegisterGlobalUI("MouseMenuUI", 1000)]
public partial class MouseMenuUI(ILog Logger) : BasicBody, IMouseMenu
{
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

    protected override void OnInitialize()
    {
        InitializeComponent();

        Logger.Info("MouseMenuUI loading completed content.");

        SetLeft(0f, 0f, 0f);
        SetTop(0f, 0f, 0f);

        // Microsoft.Xna.Framework.Vector4

        MenuContainer.BorderColor = SUIColor.Border * 0.75f;
        MenuContainer.BackgroundColor = SUIColor.Background * 0.75f;

        ScrollView.Mask.MaxHeight = new Dimension(300f, 0f);
        ScrollView.Mask.FitWidth = true;
        ScrollView.Mask.FitHeight = true;

        ScrollView.Container.FlexWrap = false;
        ScrollView.Container.FitWidth = true;
        ScrollView.Container.FitHeight = true;
        ScrollView.Container.FlexDirection = FlexDirection.Column;
        ScrollView.Container.MainAlignment = MainAlignment.Start;
        ScrollView.Container.CrossContentAlignment = CrossContentAlignment.Stretch;
        ScrollView.Container.CrossAlignment = CrossAlignment.Stretch;
        ScrollView.Container.Gap = new Size(4f);
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

        RenderTargetMatrix = Matrix.CreateTranslation(0, SwitchTimer.Lerp(10f, 0), 0);

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

#endif