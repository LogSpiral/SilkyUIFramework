namespace SilkyUIFramework;

public class SilkyUIManager
{
    public static SilkyUIManager Instance { get; } = new();

    private SilkyUIManager() { }

    #region Fields and Propertices

    /// <summary>
    /// 当前的 <see cref="UserInterface"/>
    /// </summary>
    public SilkyUI CurrentSilkyUI { get; private set; }

    /// <summary>
    /// 当前的 <see cref="UserInterface"/> 所在 List
    /// </summary>
    public List<SilkyUI> CurrentSilkyUIs { get; private set; }

    /// <summary>
    /// 鼠标悬浮元素
    /// </summary>
    protected UIElement MouseHoverTarget { get; set; }

    /// <summary>
    /// <see cref="UserInterface"/> 实例绑定的 <see cref="BasicBody"/> <see cref="Type"/>
    /// </summary>
    public Dictionary<SilkyUI, Type> BasicBodyMappingTable { get; } = [];

    /// <summary>
    /// <see cref="UserInterface"/> 实例绑定的 <see cref="BasicBody"/> <see cref="RegisterUIAttribute"/>
    /// </summary>
    public Dictionary<SilkyUI, RegisterUIAttribute> RegisterUIMappingTable { get; } = [];

    /// <summary>
    /// 界面层顺序
    /// </summary>
    private readonly List<string> _interfaceLayerOrders = [];

    /// <summary>
    /// <see cref="string"/> 插入点<br/>
    /// <see cref="List{T}"/> 插入 UI <see cref="SilkyUI"/>
    /// </summary>
    public readonly Dictionary<string, List<SilkyUI>> SilkyUILayerNodes = [];

    #endregion

    public void RegisterUI(Type basicBodyType, RegisterUIAttribute registerUIAttribute)
    {
        var silkyUI = new SilkyUI();

        BasicBodyMappingTable[silkyUI] = basicBodyType;
        RegisterUIMappingTable[silkyUI] = registerUIAttribute;

        if (SilkyUILayerNodes.TryGetValue(registerUIAttribute.LayerNode, out var silkyUIs))
            silkyUIs.Add(silkyUI);
        else SilkyUILayerNodes[registerUIAttribute.LayerNode] = [silkyUI];
    }

    public void CurrentUserInterfaceMoveToTop()
    {
        if (CurrentSilkyUIs.Remove(CurrentSilkyUI))
        {
            CurrentSilkyUIs.Insert(0, CurrentSilkyUI);
        }
    }

    protected UIElement MouseFocusTarget { get; set; }

    /// <summary>
    /// 更新 UI
    /// </summary>
    public void UpdateUI(GameTime gameTime)
    {
        MouseHoverTarget = null;
        MouseFocusTarget = null;

        // 它是绘制顺序, 所以事件处理要倒序
        foreach (var layerNode in _interfaceLayerOrders.Where(SilkyUILayerNodes.ContainsKey).Reverse())
        {
            try
            {
                var silkyUIs = SilkyUILayerNodes[layerNode];
                CurrentSilkyUIs = silkyUIs;

                var order = silkyUIs.OrderBy(value =>
                    RegisterUIMappingTable[value].Priority).ToList();
                silkyUIs.Clear();
                silkyUIs.AddRange(order);

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < silkyUIs.Count; i++)
                {
                    var silkyUI = silkyUIs[i];
                    CurrentSilkyUI = silkyUI;
                    if (silkyUI is not null)
                    {
                        if (silkyUI.Update(gameTime, MouseHoverTarget != null, MouseFocusTarget != null))
                        {
                            if (silkyUI.MouseHoverTarget != null)
                            {
                                MouseHoverTarget = silkyUI.MouseHoverTarget;
                            }
                            if (silkyUI.MouseFocusTarget != null)
                            {
                                MouseFocusTarget = silkyUI.MouseFocusTarget;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        if (MouseFocusTarget is View { OccupyPlayerInput: true } inputElement)
            Main.CurrentInputTextTakerOverride = inputElement;

        CurrentSilkyUIs = null;
        CurrentSilkyUI = null;
    }

    /// <summary>
    /// 修改界面层级
    /// </summary>
    public void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var uiLayerCount = _interfaceLayerOrders.Count;

        #region InterfaceLayer.Name 顺序

        _interfaceLayerOrders.Clear();

        foreach (var layer in layers.Where(layer => !_interfaceLayerOrders.Contains(layer.Name)))
        {
            _interfaceLayerOrders.Add(layer.Name);
        }

        #endregion

        #region 向 InterfaceLayer 插入 UI

        if (uiLayerCount < 1) return;
        foreach (var (layerNode, silkyUIs) in SilkyUILayerNodes)
        {
            // 找到图层节点
            var index = layers.FindIndex(layer => layer.Name.Equals(layerNode));
            if (index <= -1) continue;

            // 遍历当前 UI 向节点插入
            foreach (var silkyUI in silkyUIs)
            {
                if (!RegisterUIMappingTable.TryGetValue(silkyUI, out var autoload)) continue;

                var silkyUILayer = new SilkyUILayer(
                    silkyUI, autoload.Name,
                    autoload.InterfaceScaleType,
                    () => CurrentSilkyUI = silkyUI,
                    () => CurrentSilkyUI = null);

                layers.Insert(index + 1, silkyUILayer);
            }
        }

        #endregion
    }
}