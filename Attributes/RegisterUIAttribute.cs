namespace SilkyUIFramework.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterUIAttribute(
    string layerNode,
    string name,
    int priority = 0,
    InterfaceScaleType interfaceScaleType = InterfaceScaleType.UI) : Attribute
{
    /// <summary>
    /// 界面尺度类型
    /// </summary>
    public InterfaceScaleType InterfaceScaleType = interfaceScaleType;

    /// <summary>
    /// 图层节点
    /// </summary>
    public string LayerNode = layerNode;

    /// <summary>
    /// 界面名
    /// </summary>
    public string Name = name;

    /// <summary>
    /// 优先级
    /// </summary>
    public int Priority = priority;
}