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
    private bool
        _leftButton,
        _middleButton,
        _rightButton;

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
    private UIElement _leftButton;
    private UIElement _middleButton;
    private UIElement _rightButton;

    public UIElement this[MouseButtonType button]
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
    public UIElement MouseHoverTarget { get; private set; }
    public UIElement MouseFocusTarget { get; private set; }
    public UIElement LastHoverTarget { get; private set; }
    public MouseTarget LastMouseTargets { get; } = new();

    private readonly MouseStatus _mouseStatus = new();
    private readonly MouseStatus _lastMouseStatus = new();

    public void SetFocus(UIElement element)
    {
        var target = MouseFocusTarget;
        MouseFocusTarget = element;
        (target as View)?.OnLostFocus(new UIMouseEvent(MouseFocusTarget, MousePosition));
        (MouseFocusTarget as View)?.OnGotFocus(new UIMouseEvent(MouseFocusTarget, MousePosition));
    }

    public void SetBasicBody(BasicBody basicBody = null)
    {
        if (BasicBody == basicBody) return;
        BasicBody = basicBody;

        if (BasicBody == null) return;
        BasicBody.SilkyUI = this;
        BasicBody.Activate();
        BasicBody.Recalculate();
    }

    public bool Update(GameTime gameTime, bool hasHoverTarget, bool hasFocusTarget)
    {
        if (hasFocusTarget || Keys.Escape.JustPressed())
        {
            SetFocus(null);
        }

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
                MouseHoverTarget = BasicBody.GetElementAtFromView(Vector2.Transform(MousePosition, TransformMatrix));
            }
            else
            {
                MouseHoverTarget = null;
            }

            // 当切换悬浮目标
            if (MouseHoverTarget != LastHoverTarget)
            {
                LastHoverTarget?.MouseOut(new UIMouseEvent(LastHoverTarget, MousePosition));
                MouseHoverTarget?.MouseOver(new UIMouseEvent(MouseHoverTarget, MousePosition));
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
                MouseHoverTarget?.ScrollWheel(new UIScrollWheelEvent(MouseHoverTarget, MousePosition,
                    PlayerInput.ScrollWheelDeltaForUI));
            }

            BasicBody.Update(gameTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return true;
    }

    #region HandleMouseEvent

    #region Get Event

    private static Action<UIMouseEvent> GetMouseDownEvent(MouseButtonType mouseButtonType, UIElement element)
    {
        if (element is null) return null;
        return mouseButtonType switch
        {
            MouseButtonType.Left => element.LeftMouseDown,
            MouseButtonType.Right => element.RightMouseDown,
            MouseButtonType.Middle => element.MiddleMouseDown,
            _ => element.LeftMouseDown,
        };
    }

    private static Action<UIMouseEvent> GetMouseUpEvent(MouseButtonType mouseButtonType, UIElement element)
    {
        if (element is null) return null;
        return mouseButtonType switch
        {
            MouseButtonType.Left => element.LeftMouseUp,
            MouseButtonType.Right => element.RightMouseUp,
            MouseButtonType.Middle => element.MiddleMouseUp,
            _ => element.LeftMouseUp,
        };
    }

    private static Action<UIMouseEvent> GetMouseClickEvent(MouseButtonType mouseButtonType, UIElement element)
    {
        if (element is null) return null;
        return mouseButtonType switch
        {
            MouseButtonType.Left => element.LeftClick,
            MouseButtonType.Right => element.RightClick,
            MouseButtonType.Middle => element.MiddleClick,
            _ => element.LeftClick,
        };
    }

    #endregion

    private void HandleMouseEvent(MouseEventType eventType, MouseButtonType mouseButton)
    {
        var mouseEvent = new UIMouseEvent(MouseHoverTarget, MousePosition);

        switch (eventType)
        {
            default:
            case MouseEventType.Down:
                GetMouseDownEvent(mouseButton, MouseHoverTarget)?.Invoke(mouseEvent);
                break;
            case MouseEventType.Up:
                GetMouseUpEvent(mouseButton, LastMouseTargets[mouseButton])?.Invoke(mouseEvent);

                if (LastMouseTargets[mouseButton] == MouseHoverTarget)
                {
                    mouseEvent = new UIMouseEvent(MouseHoverTarget, MousePosition);
                    GetMouseClickEvent(mouseButton, LastMouseTargets[mouseButton])?.Invoke(mouseEvent);
                }
                break;
        }

        LastMouseTargets[mouseButton] = MouseHoverTarget;
    }

    #endregion

    private uint _lastCandidateCount;
    public void Draw()
    {
        if (BasicBody is not { Enabled: true }) return;
        BasicBody.Draw(Main.spriteBatch);

        // 鼠标焦点程序
        if (MouseFocusTarget is not View { OccupyPlayerInput: true } inputElement) return;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
            DepthStencilState.None, UIElement.OverflowHiddenRasterizerState, null, inputElement.FinalMatrix);

        PlayerInput.WritingText = true;
        Main.instance.HandleIME();
        Main.instance.DrawWindowsIMEPanel(inputElement.InputMethodPosition);

        inputElement.HandlePlayerInput(_lastCandidateCount > 0 || Platform.Get<IImeService>().CandidateCount > 0);
        _lastCandidateCount = Platform.Get<IImeService>().CandidateCount;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
    }
}