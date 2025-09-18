using log4net;
using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME;
using ReLogic.OS;
using SilkyUIFramework.Helper;

namespace SilkyUIFramework;

/// <summary>
/// 处理交互逻辑
/// </summary>
[Service(ServiceLifetime.Transient)]
public class SilkyUI(SilkyUIManager manager, ILog logger)
{
    private ILog Logger { get; } = logger;
    private SilkyUIManager Manager { get; } = manager;

    public int Priority { get; set; }

    public BasicBody BasicBody { get; private set; }

    public Matrix TransformMatrix { get; set; }
    public Vector2 MousePosition { get; private set; }

    public bool HasHoverElement => MouseHoverElement != null;
    public UIView MouseHoverElement { get; private set; } = null;
    public UIView LastHoverElement { get; private set; } = null;

    public void SetHoverElement(UIView hoverElement)
    {
        LastHoverElement = MouseHoverElement;

        if (hoverElement == MouseHoverElement) return;

        MouseHoverElement = hoverElement;

        try
        {
            LastHoverElement?.OnMouseLeave(new UIMouseEvent(LastHoverElement, MousePosition));
        }
        catch { throw; }
        finally
        {
            MouseHoverElement?.OnMouseEnter(new UIMouseEvent(MouseHoverElement, MousePosition));
        }
    }

    public UIView MouseFocusElement { get; private set; }
    public bool HasFocusElement => MouseFocusElement != null;

    public Dictionary<MouseButtonType, UIView> MouseElement { get; } = [];

    private readonly MouseStatus _mouseStatus = new();
    private readonly MouseStatus _lastMouseStatus = new();

    public void SetFocus(UIView focusTarget)
    {
        var lastFocusTarget = MouseFocusElement;
        MouseFocusElement = focusTarget;

        try
        {
            lastFocusTarget?.OnLostFocus(new UIMouseEvent(MouseFocusElement, MousePosition));
        }
        catch { throw; }
        finally
        {
            MouseFocusElement?.OnGotFocus(new UIMouseEvent(MouseFocusElement, MousePosition));
        }
    }

    public SilkyUI SetBody(BasicBody basicBody = null)
    {
        if (BasicBody == basicBody) return this;

        if (basicBody != null && basicBody.SilkyUI != null) return this;

        var lastBasicBody = BasicBody;
        BasicBody = basicBody;

        RuntimeHelper.ErrorCapture(() => lastBasicBody?.HandleExitTree());
        RuntimeHelper.ErrorCapture(() => BasicBody?.HandleEnterTree(this));

        return this;
    }

    /// <summary>
    /// 用于持续检测元素初始化状态以及初始化
    /// </summary>
    public void PreUpdate()
    {
        BasicBody?.Initialize();
    }

    /// <summary> 更新状态 </summary>
    private void UpdateMouseStates()
    {
        MousePosition = new Vector2(Main.mouseX, Main.mouseY);

        _lastMouseStatus.SetState(_mouseStatus);
        _mouseStatus.SetState(Main.mouseLeft, Main.mouseMiddle, Main.mouseRight);
    }

    public bool Update(GameTime gameTime)
    {
        // 按下 Escape 键时移除焦点 (或已有焦点时)
        if (Manager.HasFocusGroup || Keys.Escape.JustPressed()) SetFocus(null);

        if (BasicBody is not { Enabled: true }) return false;

        UpdateMouseStates();

        try
        {
            if (!Manager.HasHoverGroup && BasicBody.IsInteractable)
            {
                SetHoverElement(BasicBody.GetElementAt(MousePosition));
            }
            else SetHoverElement(null);

            var buttonTypes = Enum.GetValues(typeof(MouseButtonType)).Cast<MouseButtonType>().ToArray();

            // 遍历三种鼠标按键：左键、右键和中键
            foreach (MouseButtonType buttonType in buttonTypes)
            {
                if (_mouseStatus[buttonType])
                {
                    // 刚刚按下
                    if (_lastMouseStatus[buttonType]) continue;

                    // 如果目标元素存在且可以被优先处理，则将视图置于顶层
                    if (MouseHoverElement is not null)
                    {
                        Manager.CurrentSilkyUIGroup.MoveToTop(this);
                    }

                    // 设置焦点元素
                    if (!Manager.HasFocusGroup)
                    {
                        SetFocus(MouseHoverElement);
                    }

                    MouseElement[buttonType] = MouseHoverElement;
                    HandleMousePress(buttonType);
                }
                else
                {
                    // 刚刚松开
                    if (!_lastMouseStatus[buttonType]) continue;

                    HandleMouseRelease(buttonType);
                    MouseElement[buttonType] = null;
                }
            }

            // 滚动
            if (PlayerInput.ScrollWheelDeltaForUI != 0)
            {
                MouseHoverElement?.OnMouseWheel(new UIScrollWheelEvent(MouseHoverElement, MousePosition,
                    PlayerInput.ScrollWheelDeltaForUI));
            }

            BasicBody.HandleUpdate(gameTime);
        }
        catch (Exception ex)
        {
            Logger.Error("SilkyUI Update Error", ex);
        }

        return true;
    }

    #region Handle Mouse Event

    private void HandleMousePress(MouseButtonType buttonType)
    {
        var mouseElement = MouseElement[buttonType];

        // 按下元素事件
        switch (buttonType)
        {
            default:
                break;
            case MouseButtonType.Left:
                mouseElement?.OnLeftMouseDown(new UIMouseEvent(mouseElement, MousePosition));
                break;
            case MouseButtonType.Middle:
                mouseElement?.OnMiddleMouseDown(new UIMouseEvent(mouseElement, MousePosition));
                break;
            case MouseButtonType.Right:
                mouseElement?.OnRightMouseDown(new UIMouseEvent(mouseElement, MousePosition));
                break;
        }
    }

    /// <summary> 处理鼠标释放 </summary>
    private void HandleMouseRelease(MouseButtonType buttonType)
    {
        var mouseElement = MouseElement[buttonType];

        // 松开元素事件
        switch (buttonType)
        {
            default:
                break;
            case MouseButtonType.Left:
                mouseElement?.OnLeftMouseUp(new UIMouseEvent(mouseElement, MousePosition));
                break;
            case MouseButtonType.Middle:
                mouseElement?.OnMiddleMouseUp(new UIMouseEvent(mouseElement, MousePosition));
                break;
            case MouseButtonType.Right:
                mouseElement?.OnRightMouseUp(new UIMouseEvent(mouseElement, MousePosition));
                break;
        }

        // 点击元素事件，要求鼠标松开时指针必须在元素上
        if (mouseElement == MouseHoverElement)
        {
            switch (buttonType)
            {
                default:
                    break;
                case MouseButtonType.Left:
                    mouseElement?.OnLeftMouseClick(new UIMouseEvent(mouseElement, MousePosition));
                    break;
                case MouseButtonType.Middle:
                    mouseElement?.OnMiddleMouseClick(new UIMouseEvent(mouseElement, MousePosition));
                    break;
                case MouseButtonType.Right:
                    mouseElement?.OnRightMouseClick(new UIMouseEvent(mouseElement, MousePosition));
                    break;
            }
        }
    }

    #endregion

    private uint _lastCandidateCount;
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (BasicBody is not { Enabled: true }) return;

        BasicBody.Initialize();

        BasicBody.UpdateLayout();
        BasicBody.UpdatePosition();
        BasicBody.UpdateElementsOrder();

        // 更新 UI 的各种状态，比如动画
        BasicBody.HandleUpdateStatus(gameTime);

        if (BasicBody is not { Enabled: true }) return;

        BasicBody.UpdateLayout();
        BasicBody.UpdatePosition();
        BasicBody.UpdateElementsOrder();

        BasicBody.HandleDraw(gameTime, spriteBatch);

        // 鼠标焦点程序
        if (MouseFocusElement is not { OccupyPlayerInput: true }) return;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
            DepthStencilState.None, RasterizerStateForOverflowHidden, null, TransformMatrix);

        PlayerInput.WritingText = true;
        Main.instance.HandleIME();
        Main.instance.DrawWindowsIMEPanel(MouseFocusElement.InputMethodPosition);

        MouseFocusElement.HandlePlayerInput(_lastCandidateCount > 0 || Platform.Get<IImeService>().CandidateCount > 0);
        _lastCandidateCount = Platform.Get<IImeService>().CandidateCount;

        Main.spriteBatch.End();
        Main.spriteBatch.Begin();
    }

    /// <summary>
    /// 光栅化：无正反，启用裁切
    /// </summary>
    public static RasterizerState RasterizerStateForOverflowHidden
    {
        get;
        set;
    } = new RasterizerState { CullMode = CullMode.None, ScissorTestEnable = true, };
}