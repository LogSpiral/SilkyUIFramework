namespace SilkyUIFramework.UserInterfaces;

#if DEBUG

[RegisterUI("Vanilla: Radial Hotbars", "SilkyUI: MenuUI", int.MinValue)]
public class MenuUI : BasicBody
{
    public SUIDraggableView DraggableView { get; private set; }

    protected override void OnInitialize()
    {
        SetLeft(alignment: 0.5f);
        SetTop(alignment: 0.5f);

        BorderRadius = new Vector4(8f);
        DraggableView = new SUIDraggableView(this)
        {
            BorderRadius = new Vector4(6f, 5f, 0, 0),
        }.Join(this);
        DraggableView.SetWidth(0f, 1f);
        DraggableView.SetHeight(40f, 0f);
    }

    public void Open()
    {
        DraggableView.DragOffset = new Vector2(0f, 0f);
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        RectangleRender.ShadowColor = SUIColor.Border * 0.15f; // HoverTimer.Lerp(0f, 0.25f);
        RectangleRender.ShadowSize = 0f; // HoverTimer.Lerp(0f, 20f);
        RectangleRender.ShadowBlurSize = 0f; // HoverTimer.Lerp(0f, 20f);

        base.UpdateStatus(gameTime);
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (BlurMakeSystem.BlurAvailable)
        {
            if (BlurMakeSystem.SingleBlur)
            {
                var batch = Main.spriteBatch;
                batch.End();
                BlurMakeSystem.KawaseBlur();
                batch.Begin();
            }

            SDFRectangle.SampleVersion(BlurMakeSystem.BlurRenderTarget,
                Bounds.Position * Main.UIScale, Bounds.Size * Main.UIScale, BorderRadius * Main.UIScale, Matrix.Identity);
        }

        base.Draw(gameTime, spriteBatch);
    }
}

#endif