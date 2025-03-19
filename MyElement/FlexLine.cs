namespace SilkyUIFramework.MyElement;

public class FlexLine(UIView firstElement, float mainAxisSize = 0, float crossAxisSize = 0)
{
    public readonly List<UIView> Elements = [firstElement];

    /// <summary>
    /// content and gap
    /// </summary>
    public float MainSize { get; set; } = mainAxisSize;

    /// <summary>
    /// content and gap
    /// </summary>
    public float CrossSize { get; set; } = crossAxisSize;

    public float AvailableCrossSpace { get; set; }

    public float CalculateTotalGap(float gap) => (Elements.Count - 1) * gap;
}