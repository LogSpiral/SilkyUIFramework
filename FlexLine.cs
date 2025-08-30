using System.Collections;

namespace SilkyUIFramework;

public class FlexLine : IEnumerable<UIView>
{
    public List<UIView> Elements { get; }
    private FlexLine() => Elements = [];
    private FlexLine(IReadOnlyList<UIView> elements) => Elements = [.. elements];

    public IEnumerator<UIView> GetEnumerator() => Elements.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public float MainSize { get; set; }
    public float CrossSize { get; set; }

    public float GetFenceGap(float gap) => (Elements.Count - 1) * gap;

    public void UpdateMainSizeByRow(float gap)
    {
        MainSize = Elements.Sum(element => element.OuterBounds.Width) + GetFenceGap(gap);
    }

    public void UpdateMainSizeByColumn(float gap)
    {
        MainSize = Elements.Sum(element => element.OuterBounds.Height) + GetFenceGap(gap);
    }

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

    public float MainOffset { get; private set; }
    public float MainGap { get; private set; }

    public void UpdateMainAlignment(MainAlignment mainAlignment, float space, float baseGap)
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
                MainOffset = (space - MainSize) / 2f;
                MainGap = baseGap;
                break;
            case MainAlignment.End:
                MainOffset = space - MainSize;
                MainGap = baseGap;
                break;
            case MainAlignment.SpaceEvenly:
            {
                var contentSpace = MainSize - baseGap * (Elements.Count - 1);
                MainGap = (space - contentSpace) / (Elements.Count + 1);
                MainOffset = MainGap;
                break;
            }
            case MainAlignment.SpaceBetween:
            {
                var contentSpace = MainSize - baseGap * (Elements.Count - 1);
                if (Elements.Count > 1)
                {
                    MainGap = (space - contentSpace) / (Elements.Count - 1);
                    MainOffset = 0f;
                }
                else
                {
                    MainGap = 0f;
                    MainOffset = (space - contentSpace) / 2f;
                }

                break;
            }
        }
    }
}