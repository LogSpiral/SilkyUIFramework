using SilkyUIFramework.BasicElements;

namespace SilkyUIFramework;

public class SilkyUIManager
{
    public static SilkyUIManager Instance { get; } = new();

    private SilkyUIManager() { }

    /// <summary>
    /// 鼠标位置
    /// </summary>
    public static Vector2 MouseScreen => new(Main.mouseX, Main.mouseY);

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
    /// 鼠标焦点元素
    /// </summary>
    public UIElement MouseHoverTarget { get; set; }

    private UIElement _mouseFocusTarget;

    public UIElement MouseFocusTarget
    {
        get => _mouseFocusTarget;
        set => _mouseFocusTarget = value;
    }

    /// <summary>
    /// 当前鼠标焦点下是否有元素
    /// </summary>
    public bool HasMouseHoverElement => MouseHoverTarget is not null;

    /// <summary>
    /// <see cref="UserInterface"/> 实例绑定的 <see cref="BasicBody"/> <see cref="Type"/>
    /// </summary>
    public Dictionary<SilkyUI, Type> BasicBodyTypes { get; } = [];

    /// <summary>
    /// <see cref="UserInterface"/> 实例绑定的 <see cref="BasicBody"/> <see cref="AutoloadUIAttribute"/>
    /// </summary>
    public Dictionary<SilkyUI, AutoloadUIAttribute> BasicBodyTypesAutoloadInfo { get; } = [];

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

    public void RegisterUI(Type basicBodyType, AutoloadUIAttribute autoloadUIAttribute)
    {
        var silkyUI = new SilkyUI();

        BasicBodyTypes[silkyUI] = basicBodyType;
        BasicBodyTypesAutoloadInfo[silkyUI] = autoloadUIAttribute;

        if (SilkyUILayerNodes.TryGetValue(autoloadUIAttribute.LayerNode, out var silkyUIs))
            silkyUIs.Add(silkyUI);
        else SilkyUILayerNodes[autoloadUIAttribute.LayerNode] = [silkyUI];
    }

    /// <summary>
    /// 移动当前 UserInterface 到顶层
    /// </summary>
    public void CurrentUserInterfaceMoveToTop()
    {
        if (CurrentSilkyUIs.Remove(CurrentSilkyUI))
        {
            CurrentSilkyUIs.Insert(0, CurrentSilkyUI);
        }
    }

    /// <summary>
    /// 更新 UI
    /// </summary>
    public void UpdateUI(GameTime gameTime)
    {
        MouseHoverTarget = null;

        // 它是绘制顺序, 所以事件处理要倒序
        foreach (var layerNode in _interfaceLayerOrders.Where(SilkyUILayerNodes.ContainsKey).Reverse())
        {
            try
            {
                var silkyUIs = SilkyUILayerNodes[layerNode];
                CurrentSilkyUIs = silkyUIs;

                var order = silkyUIs.OrderBy(value =>
                    BasicBodyTypesAutoloadInfo[value].Priority).ToList();
                silkyUIs.Clear();
                silkyUIs.AddRange(order);

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < silkyUIs.Count; i++)
                {
                    var silkyUI = silkyUIs[i];
                    CurrentSilkyUI = silkyUI;
                    silkyUI?.Update(gameTime);
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
                if (!BasicBodyTypesAutoloadInfo.TryGetValue(silkyUI, out var autoload)) continue;

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