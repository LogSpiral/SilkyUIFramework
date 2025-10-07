namespace SilkyUIFramework;

[Service(ServiceLifetime.Singleton)]
public partial class SilkyUIManager(IServiceProvider serviceProvider)
{
    private IServiceProvider ServiceProvider { get; } = serviceProvider;

    #region Fields and Propertices

    public SilkyUIGroup CurrentSilkyUIGroup { get; private set; }

    public SilkyUIGroup MouseHoverGroup { get; internal set; }
    public SilkyUIGroup MouseFocusGroup { get; internal set; }

    public bool HasHoverGroup => MouseHoverGroup != null;
    public bool HasFocusGroup => MouseFocusGroup != null;

    /// <summary> 界面层顺序 </summary>
    private readonly List<string> _layerOrders = [];

    /// <summary> 插入位置 </summary>
    public Dictionary<string, SilkyUIGroup> GameUILayerGroups { get; } = [];

    /// <summary>
    /// string 是 LayerNode
    /// </summary>
    public Dictionary<string, List<Type>> GameUILayerBodyTypesRegistry { get; } = [];

    #endregion

    private bool _isRegistrationCompleted = false;

    /// <summary> 注册游戏内 UI </summary>
    public void RegisterUI(Type bodyType, string layerNode)
    {
        if (_isRegistrationCompleted) return;

        var list = GameUILayerBodyTypesRegistry.TryGetValue(layerNode, out var types) ? types : (GameUILayerBodyTypesRegistry[layerNode] = []);
        list.Add(bodyType);

        if (!GameUILayerGroups.ContainsKey(layerNode))
        {
            GameUILayerGroups[layerNode] = SilkyUISystem.ServiceProvider.GetRequiredService<SilkyUIGroup>();
        }
    }

    /// <summary>
    /// 获取游戏内 UI 实例
    /// </summary>
    public bool TryGetInstance<TBody>(out TBody body) where TBody : BaseBody
    {
        foreach (var (_, value) in GameUILayerGroups)
        {
            foreach (var silkyUI in value.SilkyUIs)
            {
                if (silkyUI.BaseBody is not TBody tBody)
                    continue;

                body = tBody;
                return true;
            }
        }

        body = null;
        return false;
    }

    public void UpdateUI(GameTime gameTime)
    {
        // 它是绘制顺序, 所以事件处理要倒序
        foreach (var layerNode in _layerOrders.Where(GameUILayerGroups.ContainsKey).Reverse())
        {
            CurrentSilkyUIGroup = GameUILayerGroups[layerNode];
            CurrentSilkyUIGroup.UpdateUI(gameTime);
        }

        CurrentSilkyUIGroup = null;
    }

    /// <summary> 修改界面层级 </summary>
    public void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var uiLayerCount = _layerOrders.Count;

        _layerOrders.Clear();

        foreach (var layer in layers.Where(layer => !_layerOrders.Contains(layer.Name)))
        {
            _layerOrders.Add(layer.Name);
        }

        if (uiLayerCount == 0) return;

        int index;

        foreach (var (layerNode, silkyUIGroup) in GameUILayerGroups)
        {
            // 找到图层节点
            index = layers.FindIndex(layer => layer.Name.Equals(layerNode));
            if (index <= -1) continue;

            silkyUIGroup.ModifyInterfaceLayers(layers, index);
        }

        // 游戏内全局 UI
        index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        if (index < 0) return;
        var silkyUILayer = new LegacyGameInterfaceLayer("SilkyUI: GlobalUI", delegate
        {
            DrawGlobalUI(Main.gameTimeCache);
            return true;
        }, InterfaceScaleType.UI);

        layers.Insert(index, silkyUILayer);
    }
}