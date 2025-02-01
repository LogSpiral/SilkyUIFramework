namespace SilkyUIFramework.BasicComponents;

public class SUIDividingLine : View
{
    private SUIDividingLine(Color bgColor)
    {
        ZIndex = 1f;
        BgColor = bgColor;
    }

    public static SUIDividingLine Horizontal(Color? bgColor = null)
    {
        return new SUIDividingLine(bgColor ?? new Color(18, 18, 38) * 0.75f)
        {
            SpecifyWidth = true,
            SpecifyHeight = true,
            Width = { Pixels = 0f, Percent = 1f },
            Height = { Pixels = 2f, Percent = 0f }
        };
    }

    public static SUIDividingLine Vertical(Color? bgColor = null)
    {
        return new SUIDividingLine(bgColor ?? new Color(18, 18, 38) * 0.75f)
        {
            SpecifyWidth = true,
            SpecifyHeight = true,
            Width = { Pixels = 2f, Percent = 0f },
            Height = { Pixels = 0f, Percent = 1f }
        };
    }
}