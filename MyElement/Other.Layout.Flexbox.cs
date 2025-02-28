
namespace SilkyUIFramework;

public partial class Other : IFlexbox
{
    public bool FlexWrap
    {
        get => _flexWrap;
        set
        {
            if (value == _flexWrap) return;
            _flexWrap = value;
            MarkLayoutDirty();
        }
    }
    private bool _flexWrap;

    public FlexDirection FlexDirection
    {
        get => _flexDirection;
        set
        {
            if (value == _flexDirection) return;
            _flexDirection = value;
            MarkLayoutDirty();
        }
    }
    private FlexDirection _flexDirection;

    public MainAlignment MainAlignment
    {
        get => _mainAlignment;
        set
        {
            if (value == _mainAlignment) return;
            _mainAlignment = value;
            MarkLayoutDirty();
        }
    }
    private MainAlignment _mainAlignment;

    public CrossAlignment CrossAlignment
    {
        get => _crossAlignment;
        set
        {
            if (value == _crossAlignment) return;
            _crossAlignment = value;
            MarkLayoutDirty();
        }
    }
    private CrossAlignment _crossAlignment;

    public CrossContentAlignment CrossContentAlignment
    {
        get => _crossContentAlignment;
        set
        {
            if (value == _crossContentAlignment) return;
            _crossContentAlignment = value;
            MarkLayoutDirty();
        }
    }
    private CrossContentAlignment _crossContentAlignment;

    public List<FlexLine> FlexLines { get; } = [];

    public void WrapRow()
    {
        FlexLines.Clear();
        var currentLine = new FlexLine(FlexDirection, 0);
        FlexLines.Add(currentLine);
        var wrap = new WrapCounter(InnerBounds.Width, Gap.Width);

        foreach (var item in LayoutElements)
        {
            if (wrap.Append(item.OuterBounds.Width))
            {
                currentLine.Append(item);
            }
            else
            {
                currentLine = new FlexLine(FlexDirection, currentLine.End + Gap.Height);
                currentLine.Append(item);
                FlexLines.Add(currentLine);
            }
        }
    }

    public void WrapColumn()
    {
        FlexLines.Clear();
        var currentLine = new FlexLine(FlexDirection, 0);
        FlexLines.Add(currentLine);
        var wrap = new WrapCounter(InnerBounds.Height, Gap.Height);

        foreach (var item in LayoutElements)
        {
            if (wrap.Append(item.OuterBounds.Height))
            {
                currentLine.Append(item);
            }
            else
            {
                currentLine = new FlexLine(FlexDirection, currentLine.End + Gap.Width);
                currentLine.Append(item);
                FlexLines.Add(currentLine);
            }
        }
    }

    public void FlexboxArrange()
    {
        foreach (var line in FlexLines)
        {
        }
    }

}