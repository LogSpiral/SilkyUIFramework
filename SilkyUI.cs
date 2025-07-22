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

public class MouseStatus
{
    private readonly bool[] _buttons = new bool[3];

    public bool this[MouseButtonType button]
    {
        get => _buttons[(int)button];
        set => _buttons[(int)button] = value;
    }

    public void SetState(MouseStatus status)
    {
        Array.Copy(status._buttons, _buttons, _buttons.Length);
    }

    public void SetState(bool left, bool middle, bool right)
    {
        _buttons[0] = left;
        _buttons[1] = middle;
        _buttons[2] = right;
    }
}

public class MouseTarget
{
    private readonly UIView[] _targets = new UIView[3];

    public UIView this[MouseButtonType button]
    {
        get => _targets[(int)button];
        set => _targets[(int)button] = value;
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
        if (BasicBody == basicBody) return;

        if (basicBody != null && basicBody.SilkyUI != null) return;

        var lastBasicBody = BasicBody;
        BasicBody = basicBody;

        lastBasicBody?.SetSilkyUI(null);
        BasicBody?.SetSilkyUI(this);

        lastBasicBody?.HandleExitTree();
        BasicBody?.HandleEnterTree();
    }

    /// <summary>
    /// 用于持续检测元素初始化状态以及初始化
    /// </summary>
    public void PreUpdate()
    {
        BasicBody?.Initialize();
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

            var mouseButtons = Enum.GetValues(typeof(MouseButtonType)).Cast<MouseButtonType>().ToArray();

            // 遍历三种鼠标按键：左键、右键和中键
            foreach (MouseButtonType mouseButton in mouseButtons)
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
                        HandleMousePressEvent(mouseButton);
                        LastMouseTargets[mouseButton] = MouseHoverTarget;
                    }
                }
                else
                {
                    if (_lastMouseStatus[mouseButton])
                    {
                        HandleMouseReleaseEvent(mouseButton);
                        LastMouseTargets[mouseButton] = MouseHoverTarget;
                    }
                }
            }

            // 滚动
            if (PlayerInput.ScrollWheelDeltaForUI != 0)
            {
                MouseHoverTarget?.OnMouseWheel(new UIScrollWheelEvent(MouseHoverTarget, MousePosition,
                    PlayerInput.ScrollWheelDeltaForUI));
            }

            BasicBody.HandleUpdate(gameTime);
        }
        catch (Exception ex)
        {
            SilkyUIFramework.Instance.Logger.Error("SilkyUI Update Error", ex);
        }

        return true;
    }

    #region Handle Mouse Event

    private void HandleMousePressEvent(MouseButtonType mouseButton)
    {
        if (MouseHoverTarget is null) return;

        switch (mouseButton)
        {
            default:
                break;
            case MouseButtonType.Left:
                MouseHoverTarget.OnLeftMouseDown(new UIMouseEvent(MouseHoverTarget, MousePosition));
                break;
            case MouseButtonType.Middle:
                MouseHoverTarget.OnMiddleMouseDown(new UIMouseEvent(MouseHoverTarget, MousePosition));
                break;
            case MouseButtonType.Right:
                MouseHoverTarget.OnRightMouseDown(new UIMouseEvent(MouseHoverTarget, MousePosition));
                break;
        }
    }

    private void HandleMouseReleaseEvent(MouseButtonType mouseButton)
    {
        if (MouseHoverTarget is null) return;

        switch (mouseButton)
        {
            default:
                break;
            case MouseButtonType.Left:
                MouseHoverTarget.OnLeftMouseUp(new UIMouseEvent(MouseHoverTarget, MousePosition));
                break;
            case MouseButtonType.Middle:
                MouseHoverTarget.OnMiddleMouseUp(new UIMouseEvent(MouseHoverTarget, MousePosition));
                break;
            case MouseButtonType.Right:
                MouseHoverTarget.OnRightMouseUp(new UIMouseEvent(MouseHoverTarget, MousePosition));
                break;
        }

        // 抬起时如果鼠标仍然在目标元素上，则触发点击事件
        if (LastMouseTargets[mouseButton] == MouseHoverTarget)
        {
            switch (mouseButton)
            {
                default:
                    break;
                case MouseButtonType.Left:
                    MouseHoverTarget.OnLeftMouseClick(new UIMouseEvent(MouseHoverTarget, MousePosition));
                    break;
                case MouseButtonType.Middle:
                    MouseHoverTarget.OnMiddleMouseClick(new UIMouseEvent(MouseHoverTarget, MousePosition));
                    break;
                case MouseButtonType.Right:
                    MouseHoverTarget.OnRightMouseClick(new UIMouseEvent(MouseHoverTarget, MousePosition));
                    break;
            }
        }
    }

    #endregion

    private uint _lastCandidateCount;
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (BasicBody is not { Enabled: true }) return;

        // 更新 UI 的各种状态，比如动画
        BasicBody.HandleUpdateStatus(gameTime);

        if (BasicBody is not { Enabled: true }) return;

        BasicBody.RefreshLayout();
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