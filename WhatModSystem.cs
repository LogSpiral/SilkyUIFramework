//using System.Data;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;

//namespace SilkyUIFramework;

//public static class WhatModSystem
//{
//    internal static readonly List<ExtensibleProperty> Properties = [];

//    public static ExtensibleProperty RegisterExtensibleProperty<T>(string name, Type type, T defaultValue)
//    {
//        ArgumentNullException.ThrowIfNull(defaultValue);

//        if (type != typeof(T))
//        {
//            throw new ArgumentException($"The type of the property ({type}) does not match the generic type parameter ({typeof(T)}).", nameof(type));
//        }

//        var count = Properties.Count;

//        var property = new ExtensibleProperty(name, count, type, defaultValue);

//        if (Properties.Contains(property))
//        {
//            throw new ArgumentException($"The property {name} has already been registered.", nameof(name));
//        }

//        Properties.Add(property);

//        return property;
//    }
//}

//public class ExtensibleProperties
//{
//    private readonly object[] _properties;

//    public ExtensibleProperties()
//    {
//        var properties = WhatModSystem.Properties;
//        _properties = new object[properties.Count];

//        for (int i = 0; i < _properties.Length; i++)
//        {
//            _properties[i] = properties[i].DefaultValue;
//        }
//    }

//    public T Get<T>(ExtensibleProperty property)
//    {
//        var value = _properties[property.Index];
//        if (value is null) return (T)property.DefaultValue;
//        return (T)_properties[property.Index];
//    }

//    public void Set<T>(ExtensibleProperty property, T value)
//    {
//        _properties[property.Index] = value;
//    }
//}

//public class ExtensibleProperty : IEquatable<ExtensibleProperty>
//{
//    public readonly string Name;
//    public readonly int Index;
//    public readonly Type Type;
//    internal readonly object DefaultValue;

//    internal ExtensibleProperty(string name, int index, Type type, object defaultValue)
//    {
//        ArgumentNullException.ThrowIfNull(defaultValue);

//        if (type != defaultValue.GetType())
//        {
//            throw new ArgumentException($"The type of the property ({type}) does not match the generic type parameter ({defaultValue.GetType()}).", nameof(type));
//        }

//        Name = name;
//        Index = index;
//        Type = type;
//        DefaultValue = defaultValue;
//    }

//    public override bool Equals([NotNullWhen(true)] object obj) => obj is ExtensibleProperty other && Equals(other);
//    public bool Equals(ExtensibleProperty other) => Name.Equals(other.Name);
//    public override int GetHashCode() => Name.GetHashCode();
//}

//public static class ExampleProperties
//{
//    private static readonly ExtensibleProperty _width = WhatModSystem.RegisterExtensibleProperty("Width", typeof(float), 0f);

//    public static float Width(this ExtensibleProperties ep)
//    {
//        return ep.Get<float>(_width);
//    }
//}