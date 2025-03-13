namespace SilkyUIFramework.MyElement;

/// <summary>
/// 记录 flexbox 轨道中的元素和轨道尺寸
/// </summary>
public class FlexboxAxis(bool isRow, UIView firstElement)
{
    public readonly bool IsRow = isRow;
    public readonly List<UIView> Elements = [firstElement];


    public float MainAxisSize { get; set; }

    public float CrossAxisSize { get; set; }

    public float Width { get; set; }
    public float Height { get; set; }
}