namespace SilkyUIFramework;

public static class FlexboxToolset
{
    public static void Wrap(IFlexbox container, List<IFlexboxItem> items)
    {
        switch (container.FlexDirection)
        {
            default:
            case FlexDirection.Row:
                WrapRow(container, items);
                break;
            case FlexDirection.Column:
                WrapColumn(container, items);
                break;
        }
    }

    public static void WrapRow(IFlexbox container, List<IFlexboxItem> items)
    {
        var wrap = new WrapAccumulator(container.InnerBounds.Width, container.Gap.X);

        container.FlexboxItemLines.Clear();
        var currentLine = new FlexboxLine(container.FlexDirection, 0);
        container.FlexboxItemLines.Add(currentLine);

        foreach (var item in items)
        {
            if (wrap.Append(item.OuterBounds.Width))
            {
                currentLine.Append(item);
            }
            else
            {
                currentLine = new FlexboxLine(container.FlexDirection, currentLine.End + container.Gap.Y);
                currentLine.Append(item);
                container.FlexboxItemLines.Add(currentLine);
            }
        }
    }

    public static void WrapColumn(IFlexbox container, List<IFlexboxItem> items)
    {
        var wrap = new WrapAccumulator(container.InnerBounds.Height, container.Gap.Y);

        container.FlexboxItemLines.Clear();
        var currentLine = new FlexboxLine(container.FlexDirection, 0);
        container.FlexboxItemLines.Add(currentLine);

        foreach (var item in items)
        {
            if (wrap.Append(item.OuterBounds.Height))
            {
                currentLine.Append(item);
            }
            else
            {
                currentLine = new FlexboxLine(container.FlexDirection, currentLine.End + container.Gap.X);
                currentLine.Append(item);
                container.FlexboxItemLines.Add(currentLine);
            }
        }
    }
}

public class WrapAccumulator(float max, float gap)
{
    private readonly float _maxSize = max;
    private readonly float _gap = gap;
    private bool _first = true;
    private float _size;

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

public class FlexboxLine(FlexDirection flexDirection, float start)
{
    public readonly FlexDirection FlexDirection = flexDirection;
    private readonly List<IFlexboxItem> _elements = [];
    private float _start = start;
    public float _size = 0;

    public float End => _start + _size;

    public void Append(IFlexboxItem item)
    {
        _elements.Add(item);
        _size = FlexDirection switch
        {
            FlexDirection.Column => Math.Max(item.OuterBounds.Height, _size),
            FlexDirection.Row or _ => Math.Max(item.OuterBounds.Width, _size),
        };
    }

    public void SetStart(float start) => _start = start;
    public void SetSize(float size) => _size = size;
}

public interface IFlexbox : IFlexboxItem
{
    bool FlexWrap { get; }
    FlexDirection FlexDirection { get; }
    MainAlignment MainAlignment { get; }
    CrossAlignment CrossAlignment { get; }
    CrossContentAlignment CrossContentAlignment { get; }
    List<FlexboxLine> FlexboxItemLines { get; }
}

public interface IFlexboxItem : IView
{
    float FlexGrow { get; }
    float FlexShrink { get; }
}

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