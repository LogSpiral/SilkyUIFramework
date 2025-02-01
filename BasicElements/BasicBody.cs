using System.Runtime.Serialization;

namespace SilkyUIFramework.BasicElements;

public abstract class BasicBody : View
{
    public SilkyUI SilkyUI { get; set; }

    protected BasicBody()
    {
        Border = 0;
        Width.Percent = Height.Percent = 1f;
        SpecifyWidth = SpecifyHeight = true;
    }

    public virtual bool Enabled { get; set; } = true;

    /// <summary>
    /// 无法选中
    /// </summary>
    public virtual bool UnableToSelect => false;

    /// <summary>
    /// 元素能否交互 (可交互将阻挡下层元素交互)
    /// </summary>
    public virtual bool AreHoverTargetInteractive(UIElement hoverTarget) => hoverTarget != null && hoverTarget != this;

    public override void Update(GameTime gameTime)
    {
        CheckScreenSizeChanges();
        base.Update(gameTime);
    }

    public event Action<Vector2, Vector2> ScreenSizeChanges;

    protected static Vector2 GetCurrentScreenSize() => PlayerInput.OriginalScreenSize / Main.UIScale;
    protected Vector2 LastScreenSize { get; set; } = GetCurrentScreenSize();

    /// <summary>
    /// 总是执行
    /// </summary>
    protected virtual void CheckScreenSizeChanges()
    {
        var currentScreenSize = GetCurrentScreenSize();
        if (currentScreenSize == LastScreenSize) return;
        OnScreenSizeChanges(currentScreenSize, LastScreenSize);
        LastScreenSize = currentScreenSize;
    }

    protected virtual void OnScreenSizeChanges(Vector2 newVector2, Vector2 oldVector2)
    {
        ScreenSizeChanges?.Invoke(newVector2, oldVector2);
        Recalculate();
    }

    public virtual bool UseRenderTarget { get; set; } = true;

    private float _opacity = 1f;

    /// <summary>
    /// 当前元素会创建一个独立的画布, 不建议频繁使用
    /// </summary>
    public virtual float Opacity
    {
        get => _opacity;
        set => _opacity = Math.Clamp(value, 0f, 1f);
    }

    protected float LastUIScale { get; set; } = Main.UIScale;
    protected bool UIScaleIsChanged => Math.Abs(LastUIScale - Main.UIScale) > float.Epsilon;

    protected virtual void OnUIScaleChanges()
    {
        try
        {
            UpdateMatrix();
        }
        finally
        {
            LastUIScale = Main.UIScale;
        }
    }

    protected static Point GetCanvasSize()
    {
        var viewport = Main.graphics.GraphicsDevice.Viewport;
        return new Point(viewport.Width, viewport.Height);
    }

    /// <summary>
    /// 记录原来 RenderTargetUsage, 并全部设为 PreserveContents 防止设置新的 RenderTarget 时候消失
    /// </summary>
    protected static Dictionary<RenderTargetBinding, RenderTargetUsage> RecordWhileSetUpUsages
        (RenderTargetBinding[] bindings)
    {
        if (bindings is null || bindings.Length == 0) return null;
        Dictionary<RenderTargetBinding, RenderTargetUsage> usages = [];
        foreach (var item in bindings)
        {
            if (item.renderTarget is not RenderTarget2D rt) continue;
            usages[item] = rt.RenderTargetUsage;
            rt.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        return usages;
    }

    protected static void RecoverUsages(Dictionary<RenderTargetBinding, RenderTargetUsage> usages)
    {
        if (usages is null) return;
        foreach (var kvp in usages)
        {
            if (kvp.Key.renderTarget is not RenderTarget2D rt) continue;
            rt.RenderTargetUsage = kvp.Value;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (UIScaleIsChanged) OnUIScaleChanges();

        var pool = RenderTargetPool.Instance;
        var device = Main.graphics.GraphicsDevice;

        // 不使用独立画布
        if (!UseRenderTarget)
        {
            base.Draw(spriteBatch);
            return;
        }

        var original = device.GetRenderTargets();
        var usages = RecordWhileSetUpUsages(original);
        var lastRenderTargetUsage = device.PresentationParameters.RenderTargetUsage;
        device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

        // 画布大小计算
        var canvasSize = GetCanvasSize();
        var rt2d = pool.Get(canvasSize.X, canvasSize.Y);
        try
        {
            device.SetRenderTarget(rt2d);
            device.Clear(Color.Transparent);
            base.Draw(spriteBatch);
            spriteBatch.End();

            // 恢复 RenderTargetUsage
            RecoverUsages(usages);
            device.SetRenderTargets(original);

            spriteBatch.Begin();
            spriteBatch.Draw(rt2d, Vector2.Zero, null,
                Color.White * Opacity, 0f, Vector2.Zero, Vector2.One, 0, 0);

            device.PresentationParameters.RenderTargetUsage = lastRenderTargetUsage;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            pool.Return(rt2d);
        }
    }
}