using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace SilkyUIFramework;

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public partial class Other
{
    public bool OverflowHidden { get; set; }

    #region Parent Children Properties

    /// <summary>
    /// 父元素
    /// </summary>
    public Other Parent { get; protected set; }

    /// <summary>
    /// 子元素
    /// </summary>
    protected readonly List<Other> _children = [];
    public IReadOnlyList<Other> Children => _children;

    #endregion

    #region Append Remove RemoveChild

    public virtual void AppendChild(Other child)
    {
        child.Remove();
        _children.Add(child);
        child.Parent = this;
        MarkLayoutDirty();
        PositionDirty = true;
    }

    public virtual void AppendChildren(params Other[] children)
    {
        foreach (var child in children)
        {
            child.Remove();
            _children.Add(child);
            child.Parent = this;
        }

        MarkLayoutDirty();
        PositionDirty = true;
    }

    public virtual void Remove()
    {
        Parent?.RemoveChild(this);
    }

    public virtual void RemoveChild(Other child)
    {
        _children.Remove(child);
        child.Parent = null;
        MarkLayoutDirty();
        PositionDirty = true;
    }

    public virtual void RemoveChildren()
    {
        foreach (var child in _children.ToArray())
        {
            _children.Remove(child);
            child.Parent = null;
        }
        MarkLayoutDirty();
        PositionDirty = true;
    }

    #endregion

    #region HandleUpdate
    public event Action<GameTime> OnUpdate;

    public void HandleUpdate(GameTime gameTime)
    {
        OnUpdate?.Invoke(gameTime);
        Update(gameTime);
        UpdateChildren(gameTime);
    }

    protected virtual void UpdateChildren(GameTime gameTime)
    {
        foreach (var child in _children.ToArray())
        {
            if (_invalidate) continue;
            child.HandleUpdate(gameTime);
        }
    }

    #endregion

    #region Classify

    /// <summary>
    /// 自由元素, 不受布局约束
    /// </summary>
    protected List<Other> FreeElements { get; private set; } = [];

    /// <summary>
    /// 布局元素, 受布局约束
    /// </summary>
    protected List<Other> LayoutElements { get; private set; } = [];

    /// <summary>
    /// 元素分类
    /// </summary>
    protected void Classify()
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

    #endregion

    public void HandleUpdateStatus(GameTime gameTime)
    {
        UpdateStatus(gameTime);
        UpdateChildrenStatus(gameTime);
    }

    /// <summary>
    /// 可以用作动画相关的更新 (最早调用)
    /// </summary>
    public virtual void UpdateStatus(GameTime gameTime)
    {

    }

    public virtual void UpdateChildrenStatus(GameTime gameTime)
    {
        foreach (var child in _children)
        {
            if (_invalidate) continue;
            child.HandleUpdateStatus(gameTime);
        }
    }

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