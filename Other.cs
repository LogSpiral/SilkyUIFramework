namespace SilkyUIFramework;

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public class Other
{
    public Other Parent { get; protected set; }
    protected readonly List<Other> _children = [];
    public IReadOnlyList<Other> Children => _children;

    #region Append Remove RemoveChild

    public virtual void Append(Other child)
    {
        child.Remove();
        _children.Add(child);
        child.Parent = this;
    }

    public virtual void Remove()
    {
        Parent?.RemoveChild(this);
    }

    public virtual void RemoveChild(Other child)
    {
        _children.Remove(child);
        child.Parent = null;
    }

    #endregion

    public Unit Width;
    public Unit Height;

    public Unit MinWidth, MaxWidth;
    public Unit MinHeight, MaxHeight;

    public bool WidthIsAuto { get; set; }
    public bool HeightIsAuto { get; set; }

    public float WidthValue { get; protected set; }
    public float HeightValue { get; protected set; }

    protected Constraints _constraints;
    public Constraints Constraints => _constraints;

    public float Left { get; protected set; }
    public float Top { get; protected set; }

    protected Bounds _bounds;
    public Bounds Bounds => _bounds;

    protected Bounds _innerBounds;
    public Bounds InnerBounds => _innerBounds;

    protected Bounds _outerBounds;
    public Bounds OuterBounds => _outerBounds;

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

    public event Action<GameTime, SpriteBatch> OnDraw;

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        OnDraw?.Invoke(gameTime, spriteBatch);
        DrawChildren(gameTime, spriteBatch);
    }

    public void DrawChildren(GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach (var child in _children.ToArray())
        {
            child.Draw(gameTime, spriteBatch);
        }
    }

    protected bool IsMeasureDirty = true;
    protected bool IsArrangeDirty = true;

    protected Vector2 Measure(float? width, float? height, Constraints constraints)
    {
        WidthValue = WidthIsAuto ? 0f : Width.GetValue(width ?? 0f);
        HeightValue = HeightIsAuto ? 0f : Height.GetValue(height ?? 0f);

        _constraints.MinWidth = MinWidth.GetValue(width ?? constraints.MinWidth);
        _constraints.MaxWidth = MaxWidth.GetValue(height ?? constraints.MaxWidth);
        _constraints.MinHeight = MinHeight.GetValue(width ?? constraints.MinHeight);
        _constraints.MaxHeight = MaxHeight.GetValue(height ?? constraints.MaxHeight);

        foreach (var child in _children)
        {
            child.Measure(WidthValue, HeightValue, _constraints);
        }

        IsMeasureDirty = false;
        return new Vector2(WidthValue, HeightValue);
    }

    protected void Arrange()
    {
        IsArrangeDirty = false;
    }
}

public enum TrackDirection
{
    Horizontal,
    Vertical
}

/// <summary>
/// 布局轨道
/// </summary>
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