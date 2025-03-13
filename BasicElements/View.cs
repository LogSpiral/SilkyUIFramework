using System.Xml.Serialization;

namespace SilkyUIFramework.BasicElements;

/// <summary>
/// 所有 SilkyUI 元素的父类
/// </summary>
public partial class View : UIElement
{
    public View()
    {
        MaxWidth = MaxHeight = new StyleDimension(114514f, 0f);

        OnLeftMouseDown += (_, _) => LeftMousePressed = true;
        OnRightMouseDown += (_, _) => RightMousePressed = true;
        OnMiddleMouseDown += (_, _) => MiddleMousePressed = true;

        OnLeftMouseUp += (_, _) => LeftMousePressed = false;
        OnRightMouseUp += (_, _) => RightMousePressed = false;
        OnMiddleMouseUp += (_, _) => MiddleMousePressed = false;
    }

    public virtual Vector2 InputMethodPosition => _dimensions.LeftBottom() + new Vector2(0f, 32f);

    public virtual bool OccupyPlayerInput { get; set; }

    public virtual void HandlePlayerInput(bool inputMethodStatus) { }

    public virtual UIElement GetElementAtFromView(Vector2 point)
    {
        if (Invalidate) return null;

        if (OverflowHidden && !ContainsPoint(point)) return null;

        var children =
            GetChildrenByZIndexSort().OfType<View>().Where(el => !el.IgnoresMouseInteraction);

        foreach (var child in children.Reverse())
        {
            if (child.GetElementAt(point) is { } target) return target;
        }

        if (IgnoresMouseInteraction) return null;

        return ContainsPoint(point) ? this : null;
    }

    /// <summary> 判断点是否在元素内, 会计算<see cref="FinalMatrix"/> </summary>
    public override bool ContainsPoint(Vector2 point) =>
        base.ContainsPoint(Vector2.Transform(point, Matrix.Invert(FinalMatrix)));

    /// <summary> 鼠标悬停声音 </summary>
    public SoundStyle? MouseOverSound { get; set; }

    /// <summary> 使用 <see cref="SoundID.MenuTick"/> </summary>
    public void UseMenuTickSoundForMouseOver() => MouseOverSound = SoundID.MenuTick;

    public override void MouseOver(UIMouseEvent evt)
    {
        if (MouseOverSound != null)
            SoundEngine.PlaySound(MouseOverSound);
        base.MouseOver(evt);
    }

    public virtual View AppendFromView(View child)
    {
        child.Remove();
        child.Parent = this;
        Elements.Add(child);
        Recalculate();
        return this;
    }


    public event MouseEvent GotFocus;
    public event MouseEvent LostFocus;

    public bool IsFocus { get; set; }
    public virtual void OnGotFocus(UIMouseEvent evt)
    {
        IsFocus = true;
        GotFocus?.Invoke(evt, this);
        (Parent as View)?.OnGotFocus(evt);
    }

    public virtual void OnLostFocus(UIMouseEvent evt)
    {
        IsFocus = false;
        LostFocus?.Invoke(evt, this);
        (Parent as View)?.OnLostFocus(evt);
    }
}