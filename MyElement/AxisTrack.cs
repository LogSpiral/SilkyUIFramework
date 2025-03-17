namespace SilkyUIFramework.MyElement;

/// <summary>
/// 记录 flexbox 轨道中的元素和轨道尺寸
/// </summary>
public class AxisTrack(UIView firstElement, float mainAxisSize = 0, float crossAxisSize = 0)
{
    public readonly List<UIView> Elements = [firstElement];

    public float MainAxisContent { get; set; } = mainAxisSize;
    public float CrossAxisContent { get; set; } = crossAxisSize;

    public float CrossAxisContainer { get; set; }

    public float GrowSum() => Elements.Sum(el => el.FlexGrow);
    public float ShrinkSum() => Elements.Sum(el => el.FlexShrink);

    public float GapSum(float gap) => (Elements.Count - 1) * gap;
}

public static class AxisTrackExtensions
{
    public static void WrapElements(this List<AxisTrack> flexboxAxes, float maxMainAxisSize, float gap,
        List<UIView> elements, Func<UIView, float> mainAxis, Func<UIView, float> crossAxis)
    {
        flexboxAxes.Clear();
        var axis = new AxisTrack(elements[0], mainAxis(elements[0]), crossAxis(elements[0]));

        for (var i = 1; i < elements.Count; i++)
        {
            var element = elements[i];

            var mainAxisSize = mainAxis(element);
            var crossAxisSize = crossAxis(element);

            if (axis.MainAxisContent + mainAxisSize + gap > maxMainAxisSize)
            {
                flexboxAxes.Add(axis);
                axis = new AxisTrack(element, mainAxisSize, crossAxisSize);
                continue;
            }

            axis.Elements.Add(element);
            axis.MainAxisContent += mainAxisSize + gap;
            axis.CrossAxisContent = MathF.Max(axis.CrossAxisContent, crossAxisSize);
        }

        flexboxAxes.Add(axis);
    }

    public static void Measure(this List<AxisTrack> flexboxAxes, float gap, out float mainAxisSize, out float crossAxisSize)
    {
        mainAxisSize = 0f;
        crossAxisSize = 0f;

        foreach (var axis in flexboxAxes)
        {
            mainAxisSize = Math.Max(mainAxisSize, axis.MainAxisContent);
            crossAxisSize += axis.CrossAxisContent;
        }

        crossAxisSize += (flexboxAxes.Count - 1) * gap;
    }

    public static float CrossAxisContentSum(this List<AxisTrack> flexboxAxes) => flexboxAxes.Sum(axis => axis.CrossAxisContent);
    public static float CrossGapSum(this List<AxisTrack> flexboxAxes, float gap) => (flexboxAxes.Count - 1) * gap;
}