namespace SilkyUIFramework;

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public partial class Other
{
    public bool Leaf { get; }
    public Other Parent { get; protected set; }
    protected readonly List<Other> _children = [];
    public IReadOnlyList<Other> Children => _children;

    #region Append Remove RemoveChild

    public virtual void AppendChild(Other child)
    {
        child.Remove();
        _children.Add(child);
        child.Parent = this;
        BubbleMarkerDirty();
    }

    public virtual void AppendChildren(params Other[] children)
    {
        foreach (var child in children)
        {
            AppendChild(child);
        }
    }

    public virtual void Remove()
    {
        Parent?.RemoveChild(this);
    }

    public virtual void RemoveChild(Other child)
    {
        _children.Remove(child);
        child.Parent = null;
        BubbleMarkerDirty();
    }

    public virtual void RemoveChildren()
    {
        foreach (var child in _children.ToArray())
        {
            child.Remove();
        }
    }

    #endregion

    #region bounds

    protected Bounds _bounds;
    public Bounds Bounds => _bounds;

    protected Bounds _innerBounds;
    public Bounds InnerBounds => _innerBounds;

    protected Bounds _outerBounds;
    public Bounds OuterBounds => _outerBounds;

    #endregion

    public event Action<GameTime> OnUpdate;

    public void Update(GameTime gameTime)
    {
        OnUpdate?.Invoke(gameTime);
        UpdateChildren(gameTime);
    }

    protected virtual void UpdateChildren(GameTime gameTime)
    {
        foreach (var child in _children.ToArray())
        {
            child.Update(gameTime);
        }
    }

    public bool Invalidate { get; set; }
    private bool _invalidate;

    // public Other GetElementAt(Vector2 position)
    // {
    //     if (Invalidate) return null;

    //     var children =
    //         GetChildrenByZIndexSort().OfType<View>().Where(el => !el.IgnoresMouseInteraction).Reverse().ToArray();

    //     if (OverflowHidden && !ContainsPoint(point)) return null;

    //     foreach (var child in children)
    //     {
    //         if (child.GetElementAt(point) is { } target) return target;
    //     }

    //     if (IgnoresMouseInteraction) return null;

    //     return ContainsPoint(point) ? this : null;
    // }
}

public enum TrackDirection { Horizontal, Vertical }

/// 布局轨道
public class LayoutTrack
{
    private readonly List<Other> _elements;
    public IReadOnlyList<Other> Elements => _elements;
    public int Count => _elements.Count;
    public TrackDirection Direction { get; set; }

    public float Left;
    public float Top;
    public float Width;
    public float Height;
}