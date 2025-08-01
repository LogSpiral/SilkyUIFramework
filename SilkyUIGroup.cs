namespace SilkyUIFramework;

[Service(ServiceLifetime.Transient)]
public class SilkyUIGroup(SilkyUIManager silkyUIManager)
{
    private SilkyUIManager SilkyUIManager { get; } = silkyUIManager;

    private readonly List<SilkyUI> _originalSilkyUIs = [];

    private readonly List<SilkyUI> _silkyUIs = [];

    public IReadOnlyList<SilkyUI> SilkyUIs => _silkyUIs;

    public SilkyUI CurrentUI { get; internal set; }

    public SilkyUI MouseHoverUI { get; private set; }
    public SilkyUI MouseFocusUI { get; private set; }

    public bool HasHoverUI => MouseHoverUI != null;
    public bool HasFocusUI => MouseFocusUI != null;

    public SilkyUIGroup Add(SilkyUI ui)
    {
        _originalSilkyUIs.Add(ui);
        return this;
    }

    public bool Remove(SilkyUI ui)
    {
        return _originalSilkyUIs.Remove(ui);
    }

    public SilkyUIGroup Clear()
    {
        _originalSilkyUIs.Clear();
        return this;
    }

    public void MoveToTop(SilkyUI ui)
    {
        if (_originalSilkyUIs.Remove(ui))
        {
            _originalSilkyUIs.Insert(0, ui);
        }
    }

    public void Order()
    {
        var order = _originalSilkyUIs.OrderByDescending(value => value.Priority).ToList();

        _originalSilkyUIs.Clear();
        _silkyUIs.Clear();

        _originalSilkyUIs.AddRange(order);
        _silkyUIs.AddRange(order);
    }

    public void UpdateUI(GameTime gameTime)
    {
        CurrentUI = null;
        MouseHoverUI = null;
        MouseFocusUI = null;

        foreach (var ui in _silkyUIs)
        {
            if ((CurrentUI = ui) is null) continue;

            ui.PreUpdate();

            if (!ui.Update(gameTime)) continue;

            if (ui.HasHoverElement)
            {
                MouseHoverUI = ui;
                SilkyUIManager.MouseHoverGroup = this;
            }

            if (ui.HasFocusElement)
            {
                MouseFocusUI = ui;
                SilkyUIManager.MouseFocusGroup = this;
            }
        }

        if (MouseFocusUI?.MouseFocusElement is { OccupyPlayerInput: true } inputElement)
            Main.CurrentInputTextTakerOverride = inputElement;

        CurrentUI = null;
    }

    public void ModifyInterfaceLayers(List<GameInterfaceLayer> layers, int index)
    {
        foreach (var silkyUI in _silkyUIs)
        {
            if (silkyUI.BasicBody.GetType().GetCustomAttribute<RegisterUIAttribute>() is not { } registerUI) continue;

            var silkyUILayer = new SilkyUILayer(this, silkyUI, registerUI.Name, registerUI.InterfaceScaleType);

            layers.Insert(index + 1, silkyUILayer);
        }
    }

    // 绘制 UI
    public void Draw(GameTime gameTime)
    {
        var reversedList = new List<SilkyUI>(_silkyUIs);
        reversedList.Reverse();

        foreach (var silkyUI in reversedList)
        {
            if (silkyUI.BasicBody.GetType().GetCustomAttribute<RegisterGlobalUIAttribute>() is not { } globalUI) continue;

            silkyUI.TransformMatrix = Main.UIScaleMatrix;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, silkyUI.TransformMatrix);

            CurrentUI = silkyUI;
            silkyUI.Draw(gameTime, Main.spriteBatch);
            CurrentUI = null;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, silkyUI.TransformMatrix);
        }
    }
}
