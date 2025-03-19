//using Microsoft.Xna.Framework.Input;
//using ReLogic.Localization.IME;
//using SilkyUIFramework.MyElement;

//namespace SilkyUIFramework.SS;

//public class SilkyUIOther
//{
//    public static SilkyUIManager Manager => SilkyUIManager.Instance;

//    public UIView BasicBody { get; private set; }
//    public Matrix TransformMatrix { get; set; }
//    public Vector2 MousePosition { get; private set; }
//    public UIView MouseHoverTarget { get; private set; }
//    public UIView MouseFocusTarget { get; private set; }
//    public UIView LastHoverTarget { get; private set; }
//    public MouseTarget LastMouseTargets { get; } = new();

//    private readonly MouseStatus _mouseStatus = new();
//    private readonly MouseStatus _lastMouseStatus = new();

//    public void SetFocus(UIView element)
//    {
//        var target = MouseFocusTarget;
//        MouseFocusTarget = element;
//        target?.OnLostFocus(new(MouseFocusTarget, MousePosition));
//        MouseFocusTarget?.OnGotFocus(new(MouseFocusTarget, MousePosition));
//    }

//    public void SetBasicBody(UIView basicBody = null)
//    {
//        if (BasicBody == basicBody) return;
//        BasicBody = basicBody;

//        if (BasicBody == null) return;
//        BasicBody.SilkyUI = this;
//        BasicBody.Activate();
//        BasicBody.Recalculate();
//    }

//    public bool Update(GameTime gameTime, bool hasHoverTarget, bool hasFocusTarget)
//    {
//        if (hasFocusTarget || Keys.Escape.JustPressed())
//        {
//            SetFocus(null);
//        }

//        if (BasicBody is null or { Enabled: false }) return false;

//        MousePosition = new Vector2(Main.mouseX, Main.mouseY);

//        _lastMouseStatus.SetState(_mouseStatus);
//        _mouseStatus.SetState(Main.mouseLeft, Main.mouseMiddle, Main.mouseRight);

//        try
//        {
//            // Update HoverTarget
//            LastHoverTarget = MouseHoverTarget;
//            if (!hasHoverTarget && BasicBody.IsInteractable)
//            {
//                MouseHoverTarget = BasicBody.GetElementAt(Vector2.Transform(MousePosition, TransformMatrix));
//            }
//            else
//            {
//                MouseHoverTarget = null;
//            }

//            // 当切换悬浮目标
//            if (MouseHoverTarget != LastHoverTarget)
//            {
//                LastHoverTarget?.OnMouseLeave(new(LastHoverTarget, MousePosition));
//                MouseHoverTarget?.OnMouseEnter(new(MouseHoverTarget, MousePosition));
//            }

//            // 遍历三种鼠标按键：左键、右键和中键
//            foreach (MouseButtonType mouseButton in Enum.GetValues(typeof(MouseButtonType)))
//            {
//                if (_mouseStatus[mouseButton])
//                {
//                    if (!_lastMouseStatus[mouseButton])
//                    {
//                        // 如果目标元素存在且可以被优先处理，则将视图置于顶层
//                        if (MouseHoverTarget is not null)
//                        {
//                            Manager.CurrentUserInterfaceMoveToTop();
//                        }

//                        // 设置焦点元素
//                        if (!hasFocusTarget) SetFocus(MouseHoverTarget);
//                        HandleMouseEvent(MouseEventType.Down, mouseButton);
//                    }
//                }
//                else
//                {
//                    if (_lastMouseStatus[mouseButton])
//                    {
//                        HandleMouseEvent(MouseEventType.Up, mouseButton);
//                    }
//                }
//            }

//            // 滚动
//            if (PlayerInput.ScrollWheelDeltaForUI != 0)
//            {
//                MouseHoverTarget?.OnMouseWheel(new(MouseHoverTarget, MousePosition, PlayerInput.ScrollWheelDeltaForUI));
//            }

//            BasicBody.HandleUpdate(gameTime);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine(ex);
//        }

//        return true;
//    }

//    #region HandleMouseEvent

//    #region Get Event

//    private static Action<MyElement.UIMouseEvent> GetMouseDownEvent(MouseButtonType mouseButtonType, UIView element)
//    {
//        if (element is null) return null;

//        return mouseButtonType switch
//        {
//            MouseButtonType.Left => element.OnMouseLeftDown,
//            MouseButtonType.Right => element.OnMouseRightDown,
//            MouseButtonType.Middle => element.OnMouseMiddleDown,
//            _ => null,
//        };
//    }

//    private static Action<MyElement.UIMouseEvent> GetMouseUpEvent(MouseButtonType mouseButtonType, UIView element)
//    {
//        if (element is null) return null;

//        return mouseButtonType switch
//        {
//            MouseButtonType.Left => element.OnMouseLeftUp,
//            MouseButtonType.Right => element.OnMouseRightUp,
//            MouseButtonType.Middle => element.OnMouseMiddleUp,
//            _ => null,
//        };
//    }

//    private static Action<MyElement.UIMouseEvent> GetMouseClickEvent(MouseButtonType mouseButtonType, UIView element)
//    {
//        if (element is null) return null;

//        return mouseButtonType switch
//        {
//            MouseButtonType.Left => element.OnMouseLeftClick,
//            MouseButtonType.Right => element.OnMouseRightClick,
//            MouseButtonType.Middle => element.OnMouseMiddleClick,
//            _ => null,
//        };
//    }

//    #endregion

//    private void HandleMouseEvent(MouseEventType eventType, MouseButtonType mouseButton)
//    {
//        var mouseEvent = new MyElement.UIMouseEvent(MouseHoverTarget, MousePosition);

//        switch (eventType)
//        {
//            default:
//            case MouseEventType.Down:
//                GetMouseDownEvent(mouseButton, MouseHoverTarget)?.Invoke(mouseEvent);
//                break;
//            case MouseEventType.Up:
//                GetMouseUpEvent(mouseButton, LastMouseTargets[mouseButton])?.Invoke(mouseEvent);

//                if (LastMouseTargets[mouseButton] == MouseHoverTarget)
//                {
//                    mouseEvent = new MyElement.UIMouseEvent(MouseHoverTarget, MousePosition);
//                    GetMouseClickEvent(mouseButton, LastMouseTargets[mouseButton])?.Invoke(mouseEvent);
//                }
//                break;
//        }

//        LastMouseTargets[mouseButton] = MouseHoverTarget;
//    }

//    #endregion

//    private uint _lastCandidateCount;
//    public void Draw()
//    {
//        if (BasicBody is not { Enabled: true }) return;
//        BasicBody.Draw(Main.spriteBatch);

//        // 鼠标焦点程序
//        if (MouseFocusTarget is not UIView { OccupyPlayerInput: true } inputElement) return;

//        Main.spriteBatch.End();
//        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
//            DepthStencilState.None, UIView.OverflowHiddenRasterizerState, null, inputElement.FinalMatrix);

//        PlayerInput.WritingText = true;
//        Main.instance.HandleIME();
//        Main.instance.DrawWindowsIMEPanel(inputElement.InputMethodPosition);

//        inputElement.HandlePlayerInput(_lastCandidateCount > 0 || Platform.Get<IImeService>().CandidateCount > 0);
//        _lastCandidateCount = Platform.Get<IImeService>().CandidateCount;

//        Main.spriteBatch.End();
//        Main.spriteBatch.Begin();
//    }
//}

//public class MouseTarget
//{
//    private UIView _leftButton;
//    private UIView _middleButton;
//    private UIView _rightButton;

//    public UIView this[MouseButtonType button]
//    {
//        get => button switch
//        {
//            MouseButtonType.Left => _leftButton,
//            MouseButtonType.Middle => _middleButton,
//            MouseButtonType.Right => _rightButton,
//            _ => _leftButton
//        };
//        set
//        {
//            switch (button)
//            {
//                default:
//                case MouseButtonType.Left:
//                    _leftButton = value;
//                    break;
//                case MouseButtonType.Right:
//                    _rightButton = value;
//                    break;
//                case MouseButtonType.Middle:
//                    _middleButton = value;
//                    break;
//            }
//        }
//    }
//}
