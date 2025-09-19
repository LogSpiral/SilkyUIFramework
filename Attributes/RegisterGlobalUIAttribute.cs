namespace SilkyUIFramework.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterGlobalUIAttribute(string name, int priority) : Attribute
{
    public string Name { get; } = name;

    public int Priority { get; } = priority;
}
