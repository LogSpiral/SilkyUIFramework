using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME;
using ReLogic.OS;

namespace SilkyUIFramework;

#region enum and struct

public enum MouseButtonType
{
    Left,
    Middle,
    Right
}

public enum MouseEventType
{
    Down,
    Up
}

public class MouseStatus
{
    private bool _leftButton, _middleButton, _rightButton;

    public void SetState(MouseStatus status)
    {
        _leftButton = status._leftButton;
        _middleButton = status._middleButton;
        _rightButton = status._rightButton;
    }

    public void SetState(bool leftButton, bool middleButton, bool rightButton)
    {
        _leftButton = leftButton;
        _middleButton = middleButton;
        _rightButton = rightButton;
    }

    public bool this[MouseButtonType button]
    {
        get
        {
            return button switch
            {
                MouseButtonType.Left => _leftButton,
                MouseButtonType.Middle => _middleButton,
                MouseButtonType.Right => _rightButton,
                _ => _leftButton,
            };
        }
        set
        {
            switch (button)
            {
                default:
                case MouseButtonType.Left:
                    _leftButton = value;
                    break;
                case MouseButtonType.Right:
                    _rightButton = value;
                    break;
                case MouseButtonType.Middle:
                    _middleButton = value;
                    break;
            }
        }
    }
}

public class MouseTarget
{
    private UIView _leftButton;
    private UIView _middleButton;
    private UIView _rightButton;

    public UIView this[MouseButtonType button]
    {
        get => button switch
        {
            MouseButtonType.Left => _leftButton,
            MouseButtonType.Middle => _middleButton,
            MouseButtonType.Right => _rightButton,
            _ => _leftButton
        };
        set
        {
            switch (button)
            {
                default:
                case MouseButtonType.Left:
                    _leftButton = value;
                    break;
                case MouseButtonType.Right:
                    _rightButton = value;
                    break;
                case MouseButtonType.Middle:
                    _middleButton = value;
                    break;
            }
        }
    }
}

#endregion

/// <summary>
/// 处理交互逻辑
/// </summary>
public class SilkyUI
{
    public static SilkyUIManager Manager => SilkyUIManager.Instance;

    public BasicBody BasicBody { get; private set; }
    public Matrix TransformMatrix { get; set; }
    public Vector2 MousePosition { get; private set; }
    public UIView MouseHoverTarget { get; private set; }
    public UIView MouseFocusTarget { get; private set; }
    public UIView LastHoverTarget { get; private set; }
    public MouseTarget LastMouseTargets { get; } = new();

    private readonly MouseStatus _mouseStatus = new();
    private readonly MouseStatus _lastMouseStatus = new();

    public void SetFocus(UIView focusTarget)
    {
        var lastFocusTarget = MouseFocusTarget;
        MouseFocusTarget = focusTarget;
        lastFocusTarget?.OnLostFocus(new UIMouseEvent(MouseFocusTarget, MousePosition));
        MouseFocusTarget?.OnGotFocus(new UIMouseEvent(MouseFocusTarget, MousePosition));
    }

    public void SetBody(BasicBody basicBody = null)
    {
        if (BasicBody != null && BasicBody.SilkyUI != null) return;

        if (BasicBody == basicBody) return;

        var lastBasicBody = basicBody;
        BasicBody = basicBody;

        lastBasicBody?.HandleUnmounted();
        BasicBody?.Initialize();
        BasicBody?.HandleMounted(this);
    }

    public bool Update(GameTime gameTime, bool hasHoverTarget, bool hasFocusTarget)
    {
        if (hasFocusTarget || Keys.Escape.JustPressed()) { SetFocus(null); }

        if (BasicBody is null or { Enabled: false }) return false;

        MousePosition = new Vector2(Main.mouseX, Main.mouseY);

        _lastMouseStatus.SetState(_mouseStatus);
        _mouseStatus.SetState(Main.mouseLeft, Main.mouseMiddle, Main.mouseRight);

        try
        {
            // Update HoverTarget
            LastHoverTarget = MouseHoverTarget;
            if (!hasHoverTarget && BasicBody.IsInteractable)
            {
                MouseHoverTarget = BasicBody.GetElementAt(MousePosition);
            }
            else
            {
                MouseHoverTarget = null;
            }

            // 当切换悬浮目标
            if (MouseHoverTarget != LastHoverTarget)
            {
                LastHoverTarget?.OnMouseLeave(new UIMouseEvent(LastHoverTarget, MousePosition));
                MouseHoverTarget?.OnMouseEnter(new UIMouseEvent(MouseHoverTarget, MousePosition));
            }

            // 遍历三种鼠标按键：左键、右键和中键
            foreach (MouseButtonType mouseButton in Enum.GetValues(typeof(MouseButtonType)))
            {
                if (_mouseStatus[mouseButton])
                {
                    if (!_lastMouseStatus[mouseButton])
                    {
                        // 如果目标元素存在且可以被优先处理，则将视图置于顶层
                        if (MouseHoverTarget is not null)
                        {
                            Manager.CurrentUserInterfaceMoveToTop();
                        }

                        // 设置焦点元素
                        if (!hasFocusTarget) SetFocus(MouseHoverTarget);
                        HandleMouseEvent(MouseEventType.Down, mouseButton);
                    }
                }
                else
                {
                    if (_lastMouseStatus[mouseButton])
                    {
                        HandleMouseEvent(MouseEventType.Up, mouseButton);
                    }
                }
            }

            // 滚动
            if (PlayerInput.ScrollWheelDeltaForUI != 0)
            {
                MouseHoverTarget?.OnMouseWheel(new UIScrollWheelEvent(MouseHoverTarget, MousePosition,
                    PlayerInput.ScrollWheelDeltaForUI));
            }
            // UI 中的普通更新
            BasicBody.HandleUpdate(gameTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return true;
    }

    #region HandleMouseEvent

    #region Switch Mouse Events

    private static Action<UIMouseEvent> GetMouseDownEvent(MouseButtonType mouseButtonType, UIView element)
    {
        if (element is null) return null;
        return mouseButtonType switch
        {
            MouseButtonType.Left => element.OnLeftMouseDown,
            MouseButtonType.Right => element.OnRightMouseDown,
            MouseButtonType.Middle => element.OnMiddleMouseDown,
            _ => null,
        };
    }

    private static Action<UIMouseEvent> GetMouseUpEvent(MouseButtonType mouseButtonType, UIView element)
    {
        if (element is null) return null;
        return mouseButtonType switch
        {
            MouseButtonType.Left => element.OnLeftMouseUp,
            MouseButtonType.Right => element.OnRightMouseUp,
            MouseButtonType.Middle => element.OnMiddleMouseUp,
            _ => null,
        };
    }

    private static Action<UIMouseEvent> GetMouseClickEvent(MouseButtonType mouseButtonType, UIView element)
    {
        if (element is null) return null;
        return mouseButtonType switch
        {
            MouseButtonType.Left => element.OnLeftMouseClick,
            MouseButtonType.Right => element.OnRightMouseClick,
            MouseButtonType.Middle => element.OnMiddleMouseClick,
            _ => null,
        };
    }

    #endregion

    private void HandleMouseEvent(MouseEventType eventType, MouseButtonType mouseButton)
    {

        switch (eventType)
        {
            default:
            case MouseEventType.Down:
                GetMouseDownEvent(mouseButton, MouseHoverTarget)?.Invoke(new UIMouseEvent(MouseHoverTarget, MousePosition));
                break;
            case MouseEventType.Up:
                GetMouseUpEvent(mouseButton, LastMouseTargets[mouseButton])?.Invoke(new UIMouseEvent(LastMouseTargets[mouseButton], MousePosition));

                if (LastMouseTargets[mouseButton] == MouseHoverTarget)
                {
                    GetMouseClickEvent(mouseButton, MouseHoverTarget)?.Invoke(new UIMouseEvent(MouseHoverTarget, MousePosition));
                }
                break;
        }

        LastMouseTargets[mouseButton] = MouseHoverTarget;
    }

    #endregion

    private uint _lastCandidateCount;
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (BasicBody is not { Enabled: true }) return;

        // 更新 UI 的各种状态，比如动画
        BasicBody.HandleUpdateStatus(gameTime);
        // Bounds and Layout
        BasicBody.RefreshLayout();
        // 位置脏标记检测
        BasicBody.UpdatePosition();

        BasicBody.HandleDraw(gameTime, spriteBatch);

        // 鼠标焦点程序
        if (MouseFocusTarget is not { OccupyPlayerInput: true }) return;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
            DepthStencilState.None, RasterizerStateForOverflowHidden, null, TransformMatrix);

        PlayerInput.WritingText = true;
        Main.instance.HandleIME();
        Main.instance.DrawWindowsIMEPanel(MouseFocusTarget.InputMethodPosition);

        MouseFocusTarget.HandlePlayerInput(_lastCandidateCount > 0 || Platform.Get<IImeService>().CandidateCount > 0);
        _lastCandidateCount = Platform.Get<IImeService>().CandidateCount;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
    }

    public static RasterizerState RasterizerStateForOverflowHidden
    {
        get;
        set;
    } = new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true, };
}