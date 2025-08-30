namespace SilkyUIFramework;

public struct TemplateDimensions(bool auto, float pixels, float fraction, float percent)
{
    public readonly bool Auto = auto;
    public readonly float Fraction = fraction;
    public readonly float Pixels = pixels;
    public readonly float Percent = percent;

    public float Value { get; private set; }

    public static TemplateDimensions[] Repeat(int quantity, bool auto = false, float pixels = 0f, float fraction = 0f,
        float percent = 0f)
    {
        var units = new TemplateDimensions[quantity];
        for (var i = 0; i < units.Length; i++)
        {
            units[i] = new TemplateDimensions(auto, pixels, fraction, percent);
        }

        return units;
    }
}

public class GridModel(UIElementGroup parent) : LayoutModel(parent)
{
    public TemplateDimensions[] Rows { get; set; } = [];
    public TemplateDimensions[] Columns { get; set; } = [];

    public override sealed void OnPrepare()
    {
        throw new NotImplementedException();
    }

    public override sealed void OnPrepareChildren()
    {
        throw new NotImplementedException();
    }

    public override sealed void OnRecalculateChildrenHeight()
    {
        throw new NotImplementedException();
    }

    public override sealed void OnRecalculateHeight()
    {
        throw new NotImplementedException();
    }

    public override sealed void OnResizeChildrenHeight()
    {
        throw new NotImplementedException();
    }

    public override sealed void OnResizeChildrenWidth()
    {
        throw new NotImplementedException();
    }

    public override sealed void OnUpdateChildrenLayoutOffset()
    {
        throw new NotImplementedException();
    }
}