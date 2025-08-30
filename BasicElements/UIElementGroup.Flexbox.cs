namespace SilkyUIFramework;

public partial class UIElementGroup
{
    private FlexboxModel FlexboxModel { get; set; }

    public FlexDirection FlexDirection
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public bool FlexWrap
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public MainAlignment MainAlignment
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public CrossAlignment CrossAlignment
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    public CrossContentAlignment CrossContentAlignment
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    } = CrossContentAlignment.Stretch;
}