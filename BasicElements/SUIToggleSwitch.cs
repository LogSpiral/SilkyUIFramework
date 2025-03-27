using SilkyUIFramework.Animation;

namespace SilkyUIFramework.BasicElements;

/// <summary>
/// 拨动开关
/// </summary>
public class SUIToggleSwitch : UIView
{
    /// <summary> 状态改变时触发 </summary>
    public event Action<bool> OnStatusChanges;

    private bool _status;

    /// <summary> 状态 </summary>
    public virtual bool Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            StatusChanged(value);
        }
    }

    public readonly RectangleRender Bead = new();
    public readonly AnimationTimer SwitchTimer = new(3);

    public virtual void StatusChanged(bool value)
    {
        OnStatusChanges?.Invoke(value);
        if (value) SwitchTimer.StartUpdate();
        else SwitchTimer.StartReverseUpdate();
    }

    public SUIToggleSwitch()
    {
        Padding = new Margin(2f);
        SetSize(36f, 20f);

        //CornerRadius = new Vector4(10f);
        Border = 2f;
        BorderColor = new Color(18, 18, 38) * 0.75f;
        Bead.BackgroundColor = new Color(18, 18, 38) * 0.75f;
    }

    public override void OnLeftMouseDown(UIMouseEvent evt)
    {
        Status = !Status;
        base.OnLeftMouseDown(evt);
    }

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        SwitchTimer.Update(gameTime);
        base.HandleDraw(gameTime, spriteBatch);
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);

        var position = InnerBounds.Position;
        var size = InnerBounds.Size;
        var beadSize = new Vector2(MathHelper.Min(size.Width, size.Height));
        var end = InnerBounds.RightBottom - beadSize;

        Bead.BorderRadius = new Vector4(beadSize.Y / 2f);
        Bead.Draw(SwitchTimer.Lerp(position, end), beadSize, false, SilkyUI.TransformMatrix);
    }
}