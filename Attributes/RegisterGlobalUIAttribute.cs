namespace SilkyUIFramework.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterGlobalUIAttribute(string name, int layer) : Attribute
{
    public string Name { get; } = name;

    public int Priority { get; } = layer;
}
