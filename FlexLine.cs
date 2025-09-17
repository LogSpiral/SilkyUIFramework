using System.Collections;

namespace SilkyUIFramework;

public class FlexLine
{
    public readonly List<UIView> Elements;
    private FlexLine() => Elements = [];
    private FlexLine(IReadOnlyList<UIView> elements) => Elements = [.. elements];

    public float MainSize { get; set; }
    public float CrossSize { get; set; }

    public float GetFenceGap(float gap) => (Elements.Count - 1) * gap;

    public float MaxOuterWidth()
    {
        var width = 0f;

        for (var i = 0; i < Elements.Count; i++)
            width = Math.Max(Elements[i].OuterBounds.Width, width);

        return width;
    }

    public float MaxOuterHeight()
    {
        var height = 0f;

        for (var i = 0; i < Elements.Count; i++)
            height = Math.Max(Elements[i].OuterBounds.Height, height);

        return height;
    }

    public float SumOuterWidth()
    {
        var width = 0f;

        for (var i = 0; i < Elements.Count; i++)
            width += Elements[i].OuterBounds.Width;

        return width;
    }

    public float SumOuterHeight()
    {
        var height = 0f;

        for (var i = 0; i < Elements.Count; i++)
            height += Elements[i].OuterBounds.Height;

        return height;
    }

    public void UpdateMainSizeByRow(float gap) => MainSize = SumOuterWidth() + GetFenceGap(gap);
    public void UpdateMainSizeByColumn(float gap) => MainSize = SumOuterHeight() + GetFenceGap(gap);

    public float MainOffset { get; private set; }
    public float MainGap { get; private set; }

    public void UpdateMainAlignment(MainAlignment mainAlignment, float availableSize, float baseGap)
    {
        if (Elements.Count == 0)
        {
            MainOffset = 0f;
            MainGap = baseGap;
            return;
        }

        switch (mainAlignment)
        {
            default:
            case MainAlignment.Start:
                MainOffset = 0f;
                MainGap = baseGap;
                break;
            case MainAlignment.Center:
                MainOffset = (availableSize - MainSize) / 2f;
                MainGap = baseGap;
                break;
            case MainAlignment.End:
                MainOffset = availableSize - MainSize;
                MainGap = baseGap;
                break;
            case MainAlignment.SpaceEvenly:
            {
                var contentSize = MainSize - baseGap * (Elements.Count - 1);
                MainGap = (availableSize - contentSize) / (Elements.Count + 1);
                MainOffset = MainGap;
                break;
            }
            case MainAlignment.SpaceBetween:
            {
                var contentSize = MainSize - baseGap * (Elements.Count - 1);
                if (Elements.Count > 1)
                {
                    MainGap = (availableSize - contentSize) / (Elements.Count - 1);
                    MainOffset = 0f;
                }
                else
                {
                    MainGap = 0f;
                    MainOffset = (availableSize - contentSize) / 2f;
                }

                break;
            }
        }
    }

    #region Static

    public static FlexLine CreateRow(UIView view)
    {
        var line = new FlexLine();
        line.Elements.Add(view);
        line.MainSize = view.OuterBounds.Width;
        line.CrossSize = view.OuterBounds.Height;

        return line;
    }

    public static FlexLine CreateColumn(UIView view)
    {
        var line = new FlexLine();
        line.Elements.Add(view);
        line.MainSize = view.OuterBounds.Height;
        line.CrossSize = view.OuterBounds.Width;

        return line;
    }

    public static FlexLine CreateSingleRow(IReadOnlyList<UIView> elements, float gap)
    {
        var line = new FlexLine(elements)
        {
            MainSize = elements.Sum(element => element.OuterBounds.Width) + (elements.Count - 1) * gap,
            CrossSize = elements.Max(element => element.OuterBounds.Height)
        };

        return line;
    }

    public static FlexLine CreateSingleColumn(IReadOnlyList<UIView> elements, float gap)
    {
        var line = new FlexLine(elements)
        {
            MainSize = elements.Sum(element => element.OuterBounds.Height) + (elements.Count - 1) * gap,
            CrossSize = elements.Max(element => element.OuterBounds.Width)
        };

        return line;
    }

    #endregion
}