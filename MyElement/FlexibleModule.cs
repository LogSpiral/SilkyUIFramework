namespace SilkyUIFramework;

#region enums

public enum FlexDirection
{
    Row,
    Column,
}

/// <summary> 主轴对其方式 </summary>
public enum MainAlignment
{
    /// <summary> 总体靠左 </summary>
    Start,
    /// <summary> 总体靠右 </summary>
    End,
    /// <summary> 总体居中 </summary>
    Center,
    /// <summary> 平分空间 </summary>
    SpaceEvenly,
    /// <summary> 两端对齐 </summary>
    SpaceBetween,
}

/// <summary> 交叉轴对齐方式 </summary>
public enum CrossAlignment
{
    /// <summary> 总体靠上 </summary>
    Start,
    /// <summary> 总体居中 </summary>
    Center,
    /// <summary> 总体靠下 </summary>
    End,
    /// <summary> 拉伸 </summary>
    Stretch,
}

public enum CrossContentAlignment
{
    Start,
    Center,
    End,
    Stretch,
}

#endregion

public interface IFlexbox : IFlexboxItem
{
    bool FlexWrap { get; }
    FlexDirection FlexDirection { get; }
    MainAlignment MainAlignment { get; }
    CrossAlignment CrossAlignment { get; }
    CrossContentAlignment CrossContentAlignment { get; }
    List<FlexLine> FlexLines { get; }
}

public interface IFlexboxItem : IView
{
    float FlexGrow { get; }
    float FlexShrink { get; }
}

public class WrapCounter(float max, float gap)
{
    #region Fields

    private readonly float _maxSize = max, _gap = gap;
    private bool _first = true;
    private float _size;

    #endregion

    #region Properties

    public float MaxSize => _maxSize;
    public float Gap => _gap;
    public float Size => _size;

    #endregion

    /// <summary>
    /// 追加尺寸并返回是否可以放下, 如若放不下返回 false 并重置计数
    /// </summary>
    /// <param name="size"></param>
    /// <returns>代表当前元素可放下</returns>
    public bool Append(float size)
    {
        if (_first)
        {
            _size = size;
            _first = false;
            return true;
        }

        _size += size + _gap;

        // 能容纳的话，继续
        if (_size <= _maxSize) return true;
        else
        {
            _size = size;
            return false;
        }
    }
}

public class FlexLine(FlexDirection flexDirection, float start)
{
    public readonly FlexDirection FlexDirection = flexDirection;
    public readonly List<Other> Elements = [];

    public float Start { get; set; } = start;
    public float Size { get; set; } = 0;
    public float End { get => Start + Size; set => Start = value - Size; }

    public void Append(Other item)
    {
        Elements.Add(item);
        Size = FlexDirection switch
        {
            FlexDirection.Column => Math.Max(item.OuterBounds.Height, Size),
            FlexDirection.Row or _ => Math.Max(item.OuterBounds.Width, Size),
        };
    }
}