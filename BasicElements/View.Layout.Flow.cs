namespace SilkyUIFramework.BasicElements;

public partial class View
{
    public Vector2 CalculateContentSizeByFlow()
    {
        if (FlowElements is null || FlowElements.Count == 0) return Vector2.Zero;
        return new Vector2(0f, FlowElements.Sum(element => element._outerDimensions.Height + Gap.X) - Gap.X);
    }

    protected virtual void ArrangeByFlow()
    {
        var currentTop = 0f;

        foreach (var element in FlowElements)
        {
            element.Offset = new Vector2(0, currentTop);
            currentTop += element._outerDimensions.Height + Gap.Y;
        }
    }
}