namespace SilkyUIFramework.MyElement;

public partial class UIView
{
    /// <summary> 弹性项目的增长因子 </summary>
    public float FlexGrow
    {
        get => _flexGrow;
        set
        {
            if (value == _flexGrow) return;
            _flexGrow = value;
            MarkDirty();
        }
    }

    private float _flexGrow;

    /// <summary> 弹性项目的收缩因子 </summary>
    public float FlexShrink
    {
        get => _flexShrink;
        set
        {
            if (value == _flexShrink) return;
            _flexShrink = value;
            MarkDirty();
        }
    }

    private float _flexShrink;
}