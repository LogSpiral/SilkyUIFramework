namespace SilkyUIFramework;

public partial class Other : IView
{
    /// <summary>
    /// 无效化: 交互事件, 更新, 绘制, 布局
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

    private Bounds _bounds;
    public Bounds Bounds => _bounds;

    private Bounds _innerBounds;
    public Bounds InnerBounds => _innerBounds;

    private Bounds _outerBounds;
    public Bounds OuterBounds => _outerBounds;

    public virtual void Update(GameTime gameTime)
    {

    }
}