namespace SilkyUIFramework.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class XmlElementMappingAttribute(string elementName) : Attribute
{
    public string ElementName { get; } = elementName;
}