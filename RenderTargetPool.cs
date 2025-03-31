namespace SilkyUIFramework;

/// <summary>
/// 渲染目标对象池，用于管理和复用 RenderTarget2D 实例
/// </summary>
public sealed class RenderTargetPool : IDisposable
{
    /// <summary>
    /// 获取 RenderTargetPool 的单例实例
    /// </summary>
    public static RenderTargetPool Instance { get; } = new();

    // 图形设备引用
    private readonly GraphicsDevice _graphicsDevice = Main.graphics.GraphicsDevice;

    // 可用渲染目标字典，按尺寸分组存储
    private readonly Dictionary<(int Width, int Height), Stack<RenderTarget2D>> _available = [];

    // 已占用渲染目标字典，按尺寸分组存储
    private readonly Dictionary<(int Width, int Height), Stack<RenderTarget2D>> _occupied = [];

    /// <summary>
    /// 租借指定尺寸的渲染目标
    /// </summary>
    /// <param name="width">渲染目标宽度</param>
    /// <param name="height">渲染目标高度</param>
    /// <returns>可用的 RenderTarget2D 实例</returns>
    public RenderTarget2D Rent(int width, int height)
    {
        var size = (width, height);

        // 确保 _available 和 _occupied 都有对应的 Stack
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

        // 如果有可用的渲染目标，则直接返回
        if (available.Count > 0)
        {
            var target = available.Pop();
            occupied.Push(target);
            return target;
        }

        // 没有可用的则创建新的
        var newTarget = CreateRenderTarget(_graphicsDevice, width, height);
        occupied.Push(newTarget);
        return newTarget;
    }

    /// <summary>
    /// 归还渲染目标到对象池
    /// </summary>
    /// <param name="renderTarget">要归还的 RenderTarget2D 实例</param>
    /// <exception cref="InvalidOperationException">当尝试归还未从此池租借的渲染目标时抛出</exception>
    public void Return(RenderTarget2D renderTarget)
    {
        var size = (renderTarget.Width, renderTarget.Height);

        // 检查渲染目标是否确实是从此池租借的
        if (!_occupied.TryGetValue(size, out var occupied) || occupied.Count == 0)
        {
            throw new InvalidOperationException("RenderTarget was not rented from this pool");
        }

        // 从占用栈移除并添加到可用栈
        occupied.Pop();
        _available[size].Push(renderTarget);
    }

    /// <summary>
    /// 创建新的渲染目标
    /// </summary>
    /// <param name="graphicsDevice">图形设备</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>新创建的 RenderTarget2D 实例</returns>
    private static RenderTarget2D CreateRenderTarget(GraphicsDevice graphicsDevice, int width, int height)
    {
        return new RenderTarget2D(
            graphicsDevice,
            width,
            height,
            false,
            graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.None,
            0,
            RenderTargetUsage.PreserveContents);
    }

    /// <summary>
    /// 释放对象池管理的所有资源
    /// </summary>
    /// <exception cref="InvalidOperationException">当还有未归还的渲染目标时抛出</exception>
    public void Dispose()
    {
        // 检查是否所有渲染目标都已归还
        foreach (var stack in _occupied.Values)
        {
            if (stack?.Count > 0)
                throw new InvalidOperationException("Dispose 前请确保所有 RenderTarget2D 已归还");
        }

        // 释放所有可用渲染目标
        foreach (var stack in _available.Values)
        {
            while (stack?.Count > 0)
            {
                stack.Pop().Dispose();
            }
        }

        // 清空字典
        _occupied.Clear();
        _available.Clear();
    }
}