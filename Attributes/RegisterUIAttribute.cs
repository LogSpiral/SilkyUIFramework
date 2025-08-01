namespace SilkyUIFramework.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterUIAttribute(
    string layerNode,
    string name,
    int priority = 0,
    InterfaceScaleType interfaceScaleType = InterfaceScaleType.UI) : Attribute
{
    /// <summary> 图层节点 </summary>
    public string LayerNode { get; } = layerNode;

    /// <summary> 界面名 </summary>
    public string Name { get; } = name;

    /// <summary> 优先级 </summary>
    public int Priority { get; } = priority;

    public InterfaceScaleType InterfaceScaleType { get; } = interfaceScaleType;
}