using log4net;

namespace SilkyUIFramework;

[Service(ServiceLifetime.Singleton)]
public partial class SilkyUIManager
{
    private ILog Logger { get; }
    private IServiceProvider ServiceProvider { get; }

    public SilkyUIManager(IServiceProvider serviceProvider, ILog logger)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }


    #region Fields and Propertices

    /// <summary>
    /// 当前的 <see cref="UserInterface"/> 所在 List
    /// </summary>
    public SilkyUIGroup CurrentSilkyUIGroup { get; private set; }

    public SilkyUIGroup MouseHoverGroup { get; internal set; }
    public SilkyUIGroup MouseFocusGroup { get; internal set; }

    public bool HasHoverGroup => MouseHoverGroup != null;
    public bool HasFocusGroup => MouseFocusGroup != null;

    /// <summary> 界面层顺序 </summary>
    private readonly List<string> _layerOrders = [];

    /// <summary> 插入位置 </summary>
    public Dictionary<string, SilkyUIGroup> SilkyUILayerNodes { get; } = [];
    public Dictionary<string, List<Type>> SilkyUIType { get; } = [];

    #endregion

    /// <summary>
    /// 注册游戏内 UI
    /// </summary>
    public void RegisterUI(Type bodyType, string layerNode)
    {
        Logger.Info($"Register Game UI: \"{bodyType.Name}\", \"{layerNode}\"");

        if (!SilkyUIType.TryGetValue(layerNode, out var types))
        {
            types = [];
            SilkyUIType[layerNode] = types;
        }

        types.Add(bodyType);

        if (!SilkyUILayerNodes.TryGetValue(layerNode, out var group))
        {
            group = SilkyUISystem.ServiceProvider.GetRequiredService<SilkyUIGroup>();
            SilkyUILayerNodes[layerNode] = group;
        }
    }

    /// <summary>
    /// 更新 UI
    /// </summary>
    public void UpdateUI(GameTime gameTime)
    {
        MouseHoverGroup = null;
        MouseFocusGroup = null;

        CurrentSilkyUIGroup = null;

        // 它是绘制顺序, 所以事件处理要倒序
        foreach (var layerNode in _layerOrders.Where(SilkyUILayerNodes.ContainsKey).Reverse())
        {
            CurrentSilkyUIGroup = SilkyUILayerNodes[layerNode];
            CurrentSilkyUIGroup.Order();
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

        var index = 0;

        foreach (var (layerNode, silkyUIGroup) in SilkyUILayerNodes)
        {
            silkyUIGroup.Order();

            // 找到图层节点
            index = layers.FindIndex(layer => layer.Name.Equals(layerNode));
            if (index <= -1) return;

            silkyUIGroup.ModifyInterfaceLayers(layers, index);
        }

        // 游戏内全局 UI
        index = layers.FindIndex(layers => layers.Name.Equals("Vanilla: Radial Hotbars"));
        if (index >= 0)
        {
            var silkyUILayer = new LegacyGameInterfaceLayer("SilkyUI: GlobalUI", delegate
            {
                DrawGlobalUI(Main.gameTimeCache);
                return true;
            }, InterfaceScaleType.UI);

            layers.Insert(index, silkyUILayer);
        }
    }
}