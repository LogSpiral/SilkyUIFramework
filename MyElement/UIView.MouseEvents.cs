namespace SilkyUIFramework.MyElement;

public partial class UIView
{
    public delegate void MouseEventHandler(UIMouseEvent evt, UIView listeningElement);
    public delegate void MouseWheelEventHandler(UIScrollWheelEvent evt, UIView listeningElement);

    // 鼠标事件
    public event MouseEventHandler MouseLeftDown;
    public event MouseEventHandler MouseLeftUp;
    public event MouseEventHandler MouseLeftClick;

    public event MouseEventHandler MouseRightDown;
    public event MouseEventHandler MouseRightUp;
    public event MouseEventHandler MouseRightClick;

    public event MouseEventHandler MouseMiddleDown;
    public event MouseEventHandler MouseMiddleUp;
    public event MouseEventHandler MouseMiddleClick;

    // 焦点事件
    public event MouseEventHandler GotFocus;
    public event MouseEventHandler LostFocus;

    // 悬浮事件
    public event MouseEventHandler MouseEnter;
    public event MouseEventHandler MouseLeave;
    public event MouseEventHandler MouseMove;

    public event MouseWheelEventHandler MouseWheel;

    // 鼠标事件方法
    public virtual void OnMouseLeftDown(UIMouseEvent evt)
    {
        MouseLeftDown?.Invoke(evt, this);
        Parent?.OnMouseLeftDown(evt);
    }

    public virtual void OnMouseLeftUp(UIMouseEvent evt)
    {
        MouseLeftUp?.Invoke(evt, this);
        Parent?.OnMouseLeftUp(evt);
    }

    public virtual void OnMouseLeftClick(UIMouseEvent evt)
    {
        MouseLeftClick?.Invoke(evt, this);
        Parent?.OnMouseLeftClick(evt);
    }

    public virtual void OnMouseRightDown(UIMouseEvent evt)
    {
        MouseRightDown?.Invoke(evt, this);
        Parent?.OnMouseRightDown(evt);
    }

    public virtual void OnMouseRightUp(UIMouseEvent evt)
    {
        MouseRightUp?.Invoke(evt, this);
        Parent?.OnMouseRightUp(evt);
    }

    public virtual void OnMouseRightClick(UIMouseEvent evt)
    {
        MouseRightClick?.Invoke(evt, this);
        Parent?.OnMouseRightClick(evt);
    }

    public virtual void OnMouseMiddleDown(UIMouseEvent evt)
    {
        MouseMiddleDown?.Invoke(evt, this);
        Parent?.OnMouseMiddleDown(evt);
    }

    public virtual void OnMouseMiddleUp(UIMouseEvent evt)
    {
        MouseMiddleUp?.Invoke(evt, this);
        Parent?.OnMouseMiddleUp(evt);
    }

    public virtual void OnMouseMiddleClick(UIMouseEvent evt)
    {
        MouseMiddleClick?.Invoke(evt, this);
        Parent?.OnMouseMiddleClick(evt);
    }

    // 焦点事件方法
    public virtual void OnGotFocus(UIMouseEvent evt)
    {
        GotFocus?.Invoke(evt, this);
        Parent?.OnGotFocus(evt);
    }

    public virtual void OnLostFocus(UIMouseEvent evt)
    {
        LostFocus?.Invoke(evt, this);
        Parent?.OnLostFocus(evt);
    }

    // 悬浮事件方法
    public virtual void OnMouseEnter(UIMouseEvent evt)
    {
        MouseEnter?.Invoke(evt, this);
        Parent?.OnMouseEnter(evt);
    }

    public virtual void OnMouseLeave(UIMouseEvent evt)
    {
        MouseLeave?.Invoke(evt, this);
        Parent?.OnMouseLeave(evt);
    }

    public virtual void OnMouseMove(UIMouseEvent evt)
    {
        MouseMove?.Invoke(evt, this);
        Parent?.OnMouseMove(evt);
    }

    public virtual void OnMouseWheel(UIScrollWheelEvent evt)
    {
        MouseWheel?.Invoke(evt, this);
        Parent?.OnMouseWheel(evt);
    }
}