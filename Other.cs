namespace SilkyUIFramework;

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public partial class Other
{
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
        IsPositionDirty = true;
    }

    public virtual void AppendChildren(params Other[] children)
    {
        foreach (var child in children)
        {
            child.Remove();
            _children.Add(child);
            child.Parent = this;
        }

        BubbleMarkerDirty();
        IsPositionDirty = true;
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
        IsPositionDirty = true;
    }

    public virtual void RemoveChildren()
    {
        foreach (var child in _children.ToArray())
        {
            _children.Remove(child);
            child.Parent = null;
        }
        BubbleMarkerDirty();
        IsPositionDirty = true;
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

    #region HandleUpdate

    public event Action<GameTime> OnUpdate;

    public void HandleUpdate(GameTime gameTime)
    {
        OnUpdate?.Invoke(gameTime);
        Update(gameTime);
        HandleChildrenUpdate(gameTime);
    }

    public virtual void Update(GameTime gameTime)
    {

    }

    protected virtual void HandleChildrenUpdate(GameTime gameTime)
    {
        foreach (var child in _children.ToArray())
        {
            if (_invalidate) continue;
            child.HandleUpdate(gameTime);
        }
    }

    #endregion

    /// <summary>
    /// 无效化元素: 无法触发事件, 无法更新, 无法绘制, 无法参与布局
    /// </summary>
    public bool Invalidate
    {
        get => _invalidate;
        set
        {
            if (_invalidate == value) return;
            _invalidate = value;
            BubbleMarkerDirty();
        }
    }
    private bool _invalidate;

    /// <summary>
    /// 自由元素, 不受布局约束
    /// </summary>
    public readonly List<Other> FreeElements = [];

    /// <summary>
    /// 布局元素, 受布局约束
    /// </summary>
    public readonly List<Other> LayoutElements = [];

    /// <summary>
    /// 元素分类
    /// </summary>
    public void Classify()
    {
        FreeElements.Clear();
        LayoutElements.Clear();

        foreach (var child in _children)
        {
            if (_invalidate) continue;

            if (Positioning is Core.Positioning.Absolute or Core.Positioning.Fixed)
            {
                FreeElements.Add(child);
            }
            else
            {
                LayoutElements.Add(child);
            }
        }
    }

    public bool OverflowHidden { get; set; }

    public Other GetElementAt(Vector2 position)
    {
        if (_invalidate) return null;

        if (OverflowHidden)
        {
            if (!ContainsPoint(position)) return null;
            foreach (var child in _children)
            {
                if (child.GetElementAt(position) is { } target) return target;
            }
            return this;
        }
        else
        {
            foreach (var child in _children)
            {
                if (child.GetElementAt(position) is { } target) return target;
            }
            return ContainsPoint(position) ? this : null;
        }
    }

    public bool ContainsPoint(Vector2 point) => _bounds.Contains(point);
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