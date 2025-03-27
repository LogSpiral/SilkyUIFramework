namespace SilkyUIFramework.BasicElements;

public partial class UIView
{
    /// <summary> 弹性项目的增长因子 </summary>
    public float FlexGrow
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    /// <summary> 弹性项目的收缩因子 </summary>
    public float FlexShrink
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            MarkLayoutDirty();
        }
    }
}