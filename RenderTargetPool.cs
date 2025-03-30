namespace SilkyUIFramework;

public class RenderTargetPool : IDisposable
{
    public static RenderTargetPool Instance { get; } = new();

    private RenderTargetPool()
    {
    }

    private readonly GraphicsDevice _graphicsDevice = Main.graphics.GraphicsDevice;

    /// <summary> 可用的 </summary>
    private readonly Dictionary<(int, int), Stack<RenderTarget2D>> _available = [];

    /// <summary> 被占用 </summary>
    private readonly Dictionary<(int, int), Stack<RenderTarget2D>> _occupied = [];

    /// <summary> 获取 RenderTarget2D (使用结束后需返还) </summary>
    public RenderTarget2D Get(int width, int height) => Get((width, height));

    /// <summary> 获取 RenderTarget2D (使用结束后需返还) </summary>
    public RenderTarget2D Get((int width, int height) size)
    {
        if (size.width <= 0 || size.height <= 0) throw new Exception("width or height must be greater than 0");

        #region 检查有无对应对象池 无则创建

        if (!_available.TryGetValue(size, out var available))
        {
            available = new Stack<RenderTarget2D>();
            _available[size] = available;
        }

        if (!_occupied.TryGetValue(size, out var occupied))
        {
            occupied = new Stack<RenderTarget2D>();
            _occupied[size] = occupied;
        }

        #endregion

        if (available.Count > 0)
        {
            available.TryPop(out var renderTarget2D);
            occupied.Push(renderTarget2D);
            return renderTarget2D;
        }
        else
        {
            var renderTarget2D = CreateObjectBySize(size.width, size.height);
            occupied.Push(renderTarget2D);
            return renderTarget2D;
        }
    }

    /// <summary>
    /// 归还 RenderTarget2D
    /// </summary>
    /// <param name="rt2d">要归还的 RenderTarget2D</param>
    public void Return(RenderTarget2D rt2d)
    {
        var size = (rt2d.Width, rt2d.Height);

        if (!_available.TryGetValue(size, out var available) ||
            !_occupied.TryGetValue(size, out var occupied) || !occupied.Contains(rt2d))
        {
            throw new Exception("对象不属于此处...");
        }

        if(occupied.TryPop(out _))
        {
            available.Push(rt2d);
        }
    }

    public void Dispose()
    {
        foreach (var (_, value) in _occupied)
            if (value?.Count > 0)
                throw new Exception("Please ensure that all uses are completed before Dispose");

        foreach (var (_, value) in _available)
        {
            if (value is null) continue;
            foreach (var renderTarget2D in value)
            {
                renderTarget2D?.Dispose();
            }
        }
    }

    /// <summary>
    /// 创建 RenderTarget2D
    /// </summary>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>object</returns>
    private RenderTarget2D CreateObjectBySize(int width, int height)
    {
        // Usage 设置为 PreserveContents 在每次替换的时候可以不清除内容
        return new RenderTarget2D(_graphicsDevice, width, height,
            false, _graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    }
}