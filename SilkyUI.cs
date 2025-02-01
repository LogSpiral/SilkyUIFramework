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
    public UIElement CurrentHoverTarget { get; private set; }
    public UIElement CurrentFocusTarget { get; private set; }
    public UIElement LastHoverTarget { get; private set; }
    public MouseTarget LastMouseTargets { get; } = new();

    private readonly MouseStatus _mouseStatus = new();
    private readonly MouseStatus _lastMouseStatus = new();

    public void SetBasicBody(BasicBody basicBody = null)
    {
        if (BasicBody == basicBody) return;
        BasicBody = basicBody;

        if (BasicBody == null) return;
        BasicBody.SilkyUI = this;
        BasicBody.Activate();
        BasicBody.Recalculate();
    }

    public void SetFocusTarget(View element)
    {
        CurrentFocusTarget = element;
        Manager.MouseFocusTarget = element;
        element?.LeftMouseDown(new UIMouseEvent(element, MousePosition));
    }

    public void Update(GameTime gameTime)
    {
        if (BasicBody is null or { Enabled: false }) return;

        MousePosition = SilkyUIManager.MouseScreen;

        _lastMouseStatus.SetState(_mouseStatus);
        _mouseStatus.SetState(Main.mouseLeft, Main.mouseMiddle, Main.mouseRight);

        try
        {
            // Update HoverTarget
            LastHoverTarget = CurrentHoverTarget;
            CurrentHoverTarget =
                Manager.HasMouseHoverElement || BasicBody.UnableToSelect
                    ? null
                    : BasicBody.GetElementAt(Vector2.Transform(MousePosition, TransformMatrix));

            // 悬浮元素能否交互, 可交互将 Manager 悬浮目标设为它, 设置后会阻止下层元素交互
            if (BasicBody.AreHoverTargetInteractive(CurrentHoverTarget))
                Manager.MouseHoverTarget = CurrentHoverTarget;

            // 当切换悬浮目标
            if (CurrentHoverTarget != LastHoverTarget)
            {
                LastHoverTarget?.MouseOut(new UIMouseEvent(LastHoverTarget, MousePosition));
                CurrentHoverTarget?.MouseOver(new UIMouseEvent(CurrentHoverTarget, MousePosition));
            }

            // 遍历三种鼠标按键：左键、右键和中键
            foreach (MouseButtonType mouseButton in Enum.GetValues(typeof(MouseButtonType)))
            {
                switch (_mouseStatus[mouseButton])
                {
                    // 判断当前按键是否被按下
                    case true when !_lastMouseStatus[mouseButton]:
                    {
                        // 如果目标元素存在且可以被优先处理，则将视图置于顶层
                        if (CurrentHoverTarget is not null && Manager.MouseHoverTarget == CurrentHoverTarget)
                        {
                            Manager.CurrentUserInterfaceMoveToTop();
                        }

                        HandleMouseEvent(MouseEventType.Down, mouseButton);
                        break;
                    }
                    case false when _lastMouseStatus[mouseButton]:
                    {
                        HandleMouseEvent(MouseEventType.Up, mouseButton);
                        break;
                    }
                }
            }

            // 滚动
            if (PlayerInput.ScrollWheelDeltaForUI != 0)
            {
                CurrentHoverTarget?.ScrollWheel(new UIScrollWheelEvent(CurrentHoverTarget, MousePosition,
                    PlayerInput.ScrollWheelDeltaForUI));
            }

            BasicBody.Update(gameTime);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    #region HandleMouseEvent

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

    private static Action<UIMouseEvent> GetClickEvent(MouseButtonType mouseButtonType, UIElement element)
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

    private void HandleMouseEvent(MouseEventType eventType, MouseButtonType mouseButton)
    {
        var mouseEvent = new UIMouseEvent(CurrentHoverTarget, MousePosition);

        switch (eventType)
        {
            default:
            case MouseEventType.Down:
                // 设置焦点元素
                if (CurrentHoverTarget is View inputElement)
                {
                    if (Manager.MouseFocusTarget is not SUIEditText { IsEditing: true })
                    {
                        CurrentFocusTarget = inputElement;
                        Manager.MouseFocusTarget = inputElement;
                    }
                }

                GetMouseDownEvent(mouseButton, CurrentHoverTarget)?.Invoke(mouseEvent);
                break;
            case MouseEventType.Up:
                GetMouseUpEvent(mouseButton, LastMouseTargets[mouseButton])?.Invoke(mouseEvent);

                if (LastMouseTargets[mouseButton] == CurrentHoverTarget)
                {
                    mouseEvent = new UIMouseEvent(CurrentHoverTarget, MousePosition);
                    GetClickEvent(mouseButton, LastMouseTargets[mouseButton])?.Invoke(mouseEvent);
                }

                break;
        }

        LastMouseTargets[mouseButton] = CurrentHoverTarget;
    }

    #endregion

    public void DrawUI()
    {
        // UI 未启用
        if (BasicBody is not { Enabled: true }) return;
        BasicBody.Draw(Main.spriteBatch);

        // 鼠标焦点程序
        if (CurrentFocusTarget != Manager.MouseFocusTarget ||
            Manager.MouseFocusTarget is not View { OccupyPlayerInput: true } inputElement) return;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
            DepthStencilState.None, UIElement.OverflowHiddenRasterizerState, null, inputElement.FinalMatrix);

        PlayerInput.WritingText = true;
        Main.instance.HandleIME();
        Main.instance.DrawWindowsIMEPanel(inputElement.IMEPosition);

        inputElement.HandlePlayerInput();

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
    }
}