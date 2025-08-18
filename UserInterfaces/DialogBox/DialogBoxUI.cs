using SilkyUIFramework.Animation;

namespace SilkyUIFramework.UserInterfaces.DialogBox;

[RegisterGlobalUI("DialogBoxUI", 2000)]
internal class DialogBoxUI : BasicBody
{
    public static bool ShowUI { get; set; }
    public override bool Enabled
    {
        get
        {
            ShowUI = false;
            if (ShowUI) return true;
            return !SwitchTimer.IsReverseCompleted;
        }
        set => ShowUI = value;
    }
    public override bool IsInteractable => SwitchTimer.IsForward;

    public readonly AnimationTimer SwitchTimer = new(3);
    private UIElementGroup DialogContainer { get; set; }

    protected override void OnInitialize()
    {
        SetSize(0f, 0f, 1f, 1f);

        SetLeft(0f, 0f, 0f);
        SetTop(0f, 0f, 0f);

        Border = 0;
        BorderColor = Color.Transparent;
        BackgroundColor = Color.Transparent;

        DialogContainer = new UIElementGroup()
        {
            FitWidth = true,
            FitHeight = true,
            Border = 2,
            BorderRadius = new Vector4(8f),
            BorderColor = SUIColor.Border * 0.5f,
            BackgroundColor = SUIColor.Background * 0.5f,
        }.Join(this);
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        DialogContainer.FitWidth = false;
        DialogContainer.FitHeight = false;

        //DialogContainer.SetWidth(100f);
        DialogContainer.SetLeft(0f, 0f, 0.5f);
        DialogContainer.SetTop(0f, 0f, 0.5f);
        DialogContainer.SetSize(320f, 180f);

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
                DialogContainer.Bounds.Position * Main.UIScale, DialogContainer.Bounds.Size * Main.UIScale, DialogContainer.BorderRadius * Main.UIScale, Matrix.Identity);
        }

        base.Draw(gameTime, spriteBatch);
    }
}
