using SilkyUIFramework.Animation;

namespace SilkyUIFramework.BasicElements;

/// <summary>
/// 拨动开关
/// </summary>
public class SUIToggleSwitch : View
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

    public readonly RoundedRectangle Bead = new();
    public readonly AnimationTimer SwitchTimer = new(3);

    public virtual void StatusChanged(bool value)
    {
        OnStatusChanges?.Invoke(value);
        if (value) SwitchTimer.StartForwardUpdate();
        else SwitchTimer.StartReverseUpdate();
    }

    public SUIToggleSwitch()
    {
        SetPadding(2f);
        SetSize(36f, 20f);

        CornerRadius = new Vector4(10f);
        Border = 2f;
        BorderColor = new Color(18, 18, 38) * 0.75f;
        Bead.BgColor = new Color(18, 18, 38) * 0.75f;
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        Status = !Status;
        base.LeftMouseDown(evt);
    }

    protected override void UpdateAnimationTimer(GameTime gameTime)
    {
        base.UpdateAnimationTimer(gameTime);
        SwitchTimer.Update((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var position = _innerDimensions.Position();
        var size = _innerDimensions.Size();
        var beadSize = new Vector2(MathHelper.Min(size.X, size.Y));
        var end = _innerDimensions.RightBottom() - beadSize;

        Bead.CornerRadius = new Vector4(beadSize.Y / 2f);
        Bead.Draw(SwitchTimer.Lerp(position, end), beadSize, false, FinalMatrix);
    }
}