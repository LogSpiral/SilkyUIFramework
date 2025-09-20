namespace SilkyUIFramework;

[Service(ServiceLifetime.Transient)]
public class SilkyUIGroup(SilkyUIManager silkyUIManager)
{
    private SilkyUIManager SilkyUIManager { get; } = silkyUIManager;

    private readonly List<SilkyUI> _originalSilkyUIs = [];

    private readonly List<SilkyUI> _silkyUIs = [];

    public IReadOnlyList<SilkyUI> SilkyUIs => _silkyUIs;

    private SilkyUI MouseHoverUI { get; set; }
    private SilkyUI MouseFocusUI { get; set; }

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
        var order = _originalSilkyUIs.OrderByDescending(value => value.Priority).ToArray();

        _originalSilkyUIs.Clear();
        _silkyUIs.Clear();

        _originalSilkyUIs.AddRange(order);
        _silkyUIs.AddRange(order);
    }

    public void UpdateUI(GameTime gameTime)
    {
        MouseHoverUI = null;
        MouseFocusUI = null;

        foreach (var ui in _silkyUIs.Where(ui => (ui) is not null))
        {
            ui.PreUpdate();

            if (!ui.Update(gameTime)) continue;

            if (ui.HasHoverElement)
            {
                MouseHoverUI = ui;
                SilkyUIManager.MouseHoverGroup = this;
            }

            if (!ui.HasFocusElement) continue;

            MouseFocusUI = ui;
            SilkyUIManager.MouseFocusGroup = this;
        }

        if (MouseFocusUI?.MouseFocusElement is { OccupyPlayerInput: true } inputElement)
            Main.CurrentInputTextTakerOverride = inputElement;
    }

    public void ModifyInterfaceLayers(List<GameInterfaceLayer> layers, int index)
    {
        foreach (var silkyUI in _silkyUIs)
        {
            if (silkyUI.BaseBody.GetRegisterUI() is not { } registerUI) continue;

            var silkyUILayer = new SilkyUILayer(silkyUI, registerUI.Name, registerUI.InterfaceScaleType);

            layers.Insert(index + 1, silkyUILayer);
        }
    }

    // 绘制 UI
    public void Draw(GameTime gameTime)
    {
        var reversedList = new List<SilkyUI>(_silkyUIs);
        reversedList.Reverse();

        foreach (var silkyUI in reversedList.Where(silkyUI => silkyUI.BaseBody.GetRegisterGlobalUI() is not null))
        {
            silkyUI.TransformMatrix = Main.UIScaleMatrix;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, silkyUI.TransformMatrix);

            silkyUI.Draw(gameTime, Main.spriteBatch);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred,
                null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, silkyUI.TransformMatrix);
        }
    }
}