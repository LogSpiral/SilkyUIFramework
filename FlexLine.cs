namespace SilkyUIFramework;

public class FlexLine
{
    public List<UIView> Elements { get; }

    public FlexLine(UIView firstElement, float mainSize = 0, float crossSize = 0)
    {
        Elements = [firstElement];
        MainSize = mainSize;
        CrossSize = crossSize;
    }

    private FlexLine(List<UIView> elements) => Elements = elements;

    public float MainSize { get; set; }

    public float CrossSize { get; set; }

    public float CrossSpace { get; set; }

    public float GetFenceGap(float gap) => (Elements.Count - 1) * gap;

    public static FlexLine SingleRow(List<UIView> elements, float gap)
    {
        var line = new FlexLine(elements)
        {
            MainSize = elements.Sum(element => element.OuterBounds.Width) + (elements.Count - 1) * gap,
            CrossSize = elements.Max(element => element.OuterBounds.Height)
        };

        return line;
    }

    public static FlexLine SingleColumn(List<UIView> elements, float gap)
    {
        var line = new FlexLine(elements)
        {
            MainSize = elements.Sum(element => element.OuterBounds.Height) + (elements.Count - 1) * gap,
            CrossSize = elements.Max(element => element.OuterBounds.Width)
        };

        return line;
    }
}