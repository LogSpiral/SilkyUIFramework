namespace SilkyUIFramework.BasicComponents;

public class SUIDividingLine : UIView
{
    private SUIDividingLine(Color bgColor)
    {
        //ZIndex = 1f;
        BackgroundColor = bgColor;
        Selectable = false;
    }

    public static SUIDividingLine Horizontal(Color? bgColor = null)
    {
        var line = new SUIDividingLine(bgColor ?? new Color(18, 18, 38) * 0.75f);
        line.SetSize(0f, 2f, 1f);
        return line;
    }

    public static SUIDividingLine Vertical(Color? bgColor = null)
    {
        var line = new SUIDividingLine(bgColor ?? new Color(18, 18, 38) * 0.75f);
        line.SetSize(2f, 0f, 0f, 1f);
        return line;
    }
}