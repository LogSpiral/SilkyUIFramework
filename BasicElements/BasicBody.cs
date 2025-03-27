namespace SilkyUIFramework.BasicElements;

public abstract class BasicBody : UIElementGroup
{
    protected BasicBody()
    {
        SetSize(16f * 30f, 9f * 30f);
        SetPadding(10f);
        SetGap(10f);

        Positioning = Positioning.Fixed;
        Border = 2f;
        BorderColor = Color.Black;
        BackgroundColor = Color.White * 0.25f;

        LayoutType = LayoutType.Flexbox;
        FlexDirection = FlexDirection.Column;
        MainAlignment = MainAlignment.Start;
        FlexWrap = false;
        FinallyDrawBorder = true;
    }

    public override void RefreshLayout()
    {
        if (LayoutIsDirty)
        {
            var container = GetParentAvailableSpace();
            Prepare(container.Width, container.Height);
            ResizeChildrenWidth();
            CalculateHeight();
            ResizeChildrenHeight();
            ApplyLayout();

            CleanupDirtyMark();
        }

        foreach (var child in GetValidChildren())
        {
            child.RefreshLayout();
        }
    }

    public virtual bool Enabled { get; set; } = true;

    /// <summary> 可交互的 (default: true) </summary>
    public virtual bool IsInteractable => true;

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);
        WatchBackBufferSize();
    }

    protected Size LastBackBufferSize = GraphicsDeviceHelper.GetBackBufferSize();

    protected void WatchBackBufferSize()
    {
        var currentBackBufferSize = GraphicsDeviceHelper.GetBackBufferSize();
        if (LastBackBufferSize == currentBackBufferSize) return;

        OnBackBufferSizeChanged(currentBackBufferSize, LastBackBufferSize);
        LastBackBufferSize = currentBackBufferSize;
    }

    protected virtual void OnBackBufferSizeChanged(Size newVector2, Size oldVector2)
    {
        SetSize(newVector2.Width, newVector2.Height);
    }

    public override UIView GetElementAt(Vector2 mousePosition)
    {
        return base.GetElementAt(mousePosition);
        if (Invalid) return null;

        if (!ContainsPoint(mousePosition)) return null;

        foreach (var child in GetValidChildren())
        {
            var target = child.GetElementAt(mousePosition);
            if (target != null) return target;
        }

        // 所有子元素都不符合条件, 如果自身不忽略鼠标交互, 则返回自己
        return IgnoreMouseInteraction ? null : this;
    }

    public virtual bool UseRenderTarget { get; set; } = false;

    public virtual float Opacity
    {
        get => field;
        set => field = Math.Clamp(value, 0f, 1f);
    } = 1f;

    public override void HandleDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        var pool = RenderTargetPool.Instance;
        var device = Main.graphics.GraphicsDevice;

        // 不使用独立画布
        if (!UseRenderTarget)
        {
            base.HandleDraw(gameTime, spriteBatch);
            return;
        }

        var original = device.GetRenderTargets();
        var usageRecords = original.RecordUsage();
        var lastRenderTargetUsage = device.PresentationParameters.RenderTargetUsage;
        device.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

        // 画布大小计算
        var canvasSize = GraphicsDeviceHelper.GetBackBufferSize();
        var uiRenderTarget = pool.Get((int)canvasSize.Width, (int)canvasSize.Height);
        try
        {
            device.SetRenderTarget(uiRenderTarget);

            device.Clear(Color.Transparent);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred,
                null, null, null, SilkyUI.RasterizerStateForOverflowHidden, null, SilkyUI.TransformMatrix);

            base.HandleDraw(gameTime, spriteBatch);

            spriteBatch.End();

            device.SetRenderTargets(original);
            usageRecords.Restore();

            spriteBatch.Begin();
            spriteBatch.Draw(uiRenderTarget, Vector2.Zero, null,
                Color.White * Opacity, 0f, Vector2.Zero, Vector2.One, 0, 0);

            device.PresentationParameters.RenderTargetUsage = lastRenderTargetUsage;
        }
        catch (Exception e)
        {
            var mod = ModContent.GetInstance<SilkyUIFramework>();
            mod.Logger.Error($"BasicBody Draw error: {e}");
            throw;
        }
        finally
        {
            pool.Return(uiRenderTarget);
        }
    }
}