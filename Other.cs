using SilkyUIFramework.Core;

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

    public bool WidthIsAuto { get; set; }
    public bool HeightIsAuto { get; set; }

    public Unit WidthUnit;
    public Unit HeightUnit;

    public float WidthValue { get; protected set; }
    public float HeightValue { get; protected set; }

    public Unit MinWidth, MaxWidth;
    public Unit MinHeight, MaxHeight;

    protected Constraints _constraints;
    public Constraints Constraints => _constraints;

    public float Left { get; protected set; }
    public float Top { get; protected set; }

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

    public event Action<GameTime, SpriteBatch> OnDraw;

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        OnDraw?.Invoke(gameTime, spriteBatch);
        DrawSelf(gameTime, spriteBatch);
        DrawChildren(gameTime, spriteBatch);
    }


    /// <summary>
    /// 先看效果，随便画画
    /// </summary>
    public virtual void DrawSelf(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var texture2D = TextureAssets.MagicPixel.Value;
        var sourceRectangle = new Rectangle(0, 0, 1, 1);
        var color = Color.White * 0.5f;
        spriteBatch.Draw(texture2D, _bounds.Position, sourceRectangle, color, 0f, Vector2.Zero, _bounds.Size, 0, 0f);
    }

    public virtual void DrawChildren(GameTime gameTime, SpriteBatch spriteBatch)
    {
        foreach (var child in _children.ToArray())
        {
            child.Draw(gameTime, spriteBatch);
        }
    }

    public Vector2 Gap;
    public LayoutDirection LayoutDirection { get; set; }

    public BoxSizing BoxSizing { get; set; }
    public Margin Margin { get; } = new();
    public Margin Padding { get; } = new();

    public void CalculateBoxSize(Size size, out Size box, out Size outerBox, out Size innerBox)
    {
        switch (BoxSizing)
        {
            default:
            case BoxSizing.BorderBox:
            {
                box = size;
                outerBox = size + Margin;
                innerBox = size - Padding;
                break;
            }
            case BoxSizing.ContentBox:
            {
                innerBox = size;
                box = innerBox + Padding;
                outerBox = box + Margin;
                break;
            }
        }

        box = Size.Max(Size.Zero, box);
        outerBox = Size.Max(Size.Zero, outerBox);
        innerBox = Size.Max(Size.Zero, innerBox);
    }

    /// <summary>
    /// 修剪
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="forcedWidth">父元素强制为子元素设定的宽度 (是否接受取决于子元素)</param>
    /// <param name="forcedHeight">父元素强制为子元素设定的高度 (是否接受取决于子元素)</param>
    /// <returns></returns>
    protected virtual Size Trim(float width, float height, float? forcedWidth = null, float? forcedHeight = null)
    {
        if (forcedWidth.HasValue)
            WidthValue = forcedWidth.Value;
        else WidthValue = WidthIsAuto ? 0f : WidthUnit.GetValue(width);

        // 如果我现在要实现 Flexbox 的根据最大的子元素拉伸其余子元素的行为是不是就可以了

        switch (LayoutDirection)
        {
            // 横向排列
            case LayoutDirection.Row:
            {
                // 纵向要拉伸子元素
                for (int i = 0; i < _children.Count; i++)
                {
                    var child = _children[i];
                    child.Trim(WidthValue, HeightValue, forcedHeight: HeightValue);
                }
                break;
            }
            default:
            case LayoutDirection.Column:
            {
                break;
            }
        }

        return new Size();
    }

    /// <summary>
    /// 临时使用
    /// </summary>
    public bool IsFlow;

    /// <summary>
    /// 测量, 测量期间不会对元素进行任何限制
    /// 测量的大小仅反应元素自身的预期, 并非最终大小
    /// </summary>
    protected virtual Size Measure(float width, float height)
    {
        WidthValue = WidthIsAuto ? 0f : WidthUnit.GetValue(width);
        HeightValue = HeightIsAuto ? 0f : HeightUnit.GetValue(height);

        if (WidthIsAuto && HeightIsAuto)
        {
            _bounds.Size = Size.Zero;
            _outerBounds.Size = Size.Zero;
            _innerBounds.Size = Size.Zero;
        }
        else
        {
            CalculateBoxSize(new Size(WidthValue, HeightValue), out var box, out var outerBox, out var innerBox);
            _bounds.Size = box;
            _outerBounds.Size = outerBox;
            _innerBounds.Size = innerBox;
        }

        if (_children.Count < 1) return _outerBounds.Size;

        foreach (var child in _children)
        {
            child.Measure(_innerBounds.Width, _innerBounds.Height);
        }

        if (IsFlow)
        {
            switch (LayoutDirection)
            {
                case LayoutDirection.Row:
                {
                    WidthValue = _children.Sum(child => child.OuterBounds.Width) + (_children.Count - 1) * Gap.X;
                    HeightValue = _children.Max(child => child.OuterBounds.Height);
                    break;
                }
                default:
                case LayoutDirection.Column:
                {
                    WidthValue = _children.Max(child => child.OuterBounds.Width);
                    HeightValue = _children.Sum(child => child.OuterBounds.Height) + (_children.Count - 1) * Gap.Y;
                    break;
                }
            }

            CalculateBoxSize(new Size(WidthValue, HeightValue) + Padding, out var box, out var outerBox, out var innerBox);

            _bounds.Size = box;
            _outerBounds.Size = outerBox;
            _innerBounds.Size = innerBox;
        }

        return _outerBounds.Size;
    }

    /// <summary>
    /// 元素定位模式
    /// </summary>
    public Positioning Positioning { get; set; }

    protected Vector2 Position;
    protected Vector2 LayoutPosition;
    protected Vector2 Offset;

    protected void Arrange(float width, float height)
    {
        switch (LayoutDirection)
        {
            case LayoutDirection.Row:
            {
                var offset = Vector2.Zero;
                for (int i = 0; i < _children.Count; i++)
                {
                    _children[i].Position = _innerBounds.Position + offset;

                    var childSize = _children[i].Measure(WidthValue, HeightValue);
                    if (i > 0) WidthValue += Gap.X;
                    WidthValue += childSize.Width;
                    HeightValue = Math.Max(HeightValue, childSize.Height);
                }
                break;
            }
            case LayoutDirection.Column:
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    var childSize = _children[i].Measure(WidthValue, HeightValue);
                    if (i > 0) HeightValue += Gap.Y;
                    HeightValue += childSize.Height;
                    WidthValue = Math.Max(WidthValue, childSize.Width);
                }
                break;
            }
        }
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