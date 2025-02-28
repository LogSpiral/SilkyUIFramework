using SilkyUIFramework.Core;

namespace SilkyUIFramework;

public partial class Other
{
    private Size _containerForMeasure;
    public float MinWidth { get; private set; }
    public float MaxWidth { get; private set; }
    public float Width { get; private set; }

    public float MinHeight { get; private set; }
    public float MaxHeight { get; private set; }
    public float Height { get; private set; }

    private Unit _minWidthUnit = new(0, 0);
    private Unit _maxWidthUnit = new(ushort.MaxValue, 0);
    private Unit _widthUnit = new(0, 0);

    private Unit _minHeightUnit = new(0, 0);
    private Unit _maxHeightUnit = new(ushort.MaxValue, 0);
    private Unit _heightUnit = new(0, 0);

    protected void UpdateConstraints(Size container)
    {
        MinWidth = _minWidthUnit.GetValue(container.Width);
        MaxWidth = _maxWidthUnit.GetValue(container.Width);
        MinHeight = _minHeightUnit.GetValue(container.Height);
        MaxHeight = _maxHeightUnit.GetValue(container.Height);
    }

    protected void UpdateWidth(float containerWidth)
    {
        Width = MathHelper.Clamp(_widthUnit.GetValue(containerWidth), MinWidth, MaxWidth);
    }

    protected void UpdateHeight(float containerHeight)
    {
        Height = MathHelper.Clamp(_heightUnit.GetValue(containerHeight), MinHeight, MaxHeight);
    }

    public void UpdateBoundsWidth(float containerWidth)
    {
        UpdateWidth(containerWidth);
        switch (BoxSizing)
        {
            default:
            case BoxSizing.BorderBox:
                SetBoundsWidth(Width);
                break;
            case BoxSizing.ContentBox:
                SetInnerBoundsWidth(Width);
                break;
        }
    }

    public void UpdateBoundsHeight(float containerHeight)
    {
        UpdateHeight(containerHeight);
        switch (BoxSizing)
        {
            default:
            case BoxSizing.BorderBox:
                SetBoundsHeight(Height);
                break;
            case BoxSizing.ContentBox:
                SetInnerBoundsHeight(Height);
                break;
        }
    }
}