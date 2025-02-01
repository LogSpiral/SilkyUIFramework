namespace SilkyUIFramework;

public struct TemplateUnit(float pixels, float fraction = 0f, float percent = 0f)
{
    public readonly float Fraction = fraction;
    public readonly float Pixels = pixels;
    public readonly float Percent = percent;

    public float Value { get; private set; }

    public void RecalculateValue(float container, float cell) =>
        Value = Pixels + container * Percent + cell * Fraction;

    public static TemplateUnit[] Repeat(int quantity, float pixels, float fraction = 0f, float percent = 0f)
    {
        var units = new TemplateUnit[quantity];
        for (int i = 0; i < units.Length; i++)
        {
            units[i] = new TemplateUnit(pixels, fraction, percent);
        }
        return units;
    }
}