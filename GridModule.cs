namespace SilkyUIFramework;

public class GridModule(UIElementGroup parent) : LayoutModule(parent)
{
    public TemplateDimensions[] Rows { get; set; } = [];
    public TemplateDimensions[] Columns { get; set; } = [];

    public bool[,] Marks;

    private float _rowsFraction = 0f;
    private float _columnsFraction = 0f;
    private float _rowsFenceGap = 0f;
    private float _columnsFenceGap = 0f;

    public override void UpdateCacheStatus()
    {
        base.UpdateCacheStatus();

        _rowsFraction = 0;
        for (var i = 0; i < Rows.Length; i++)
        {
            _rowsFraction = Rows[i].Fraction;
        }

        _columnsFraction = 0;
        for (var i = 0; i < Columns.Length; i++)
        {
            _columnsFraction = Columns[i].Fraction;
        }

        _columnsFenceGap = (Rows.Length - 1) * Gap.Width;
        _rowsFenceGap = (Rows.Length - 1) * Gap.Height;

        Marks = new bool[Rows.Length, Columns.Length];

        var availableSize = Parent.InnerBounds;

        if (FitWidth)
        {
            for (var i = 0; i < Columns.Length; i++) Columns[i].Value = Columns[i].Pixels;
        }
        else
        {
            var remaining = availableSize.Width - _columnsFenceGap;

            for (var i = 0; i < Columns.Length; i++)
            {
                Columns[i].Recalculate(availableSize.Width);
                remaining -= Columns[i].Pixels;
            }

            if (remaining > 0)
            {
                var share = remaining / Columns.Length;
                for (var i = 0; i < Columns.Length; i++)
                {
                    Columns[i].Value += share;
                }
            }
        }

        if (FitHeight)
        {
            for (var i = 0; i < Rows.Length; i++) Rows[i].Value = Rows[i].Pixels;
        }
        else
        {
            var remaining = availableSize.Height - _rowsFenceGap;

            for (var i = 0; i < Rows.Length; i++)
            {
                Rows[i].Recalculate(availableSize.Height);
                remaining -= Rows[i].Pixels;
            }

            if (remaining > 0)
            {
                var share = remaining / Rows.Length;
                for (var i = 0; i < Rows.Length; i++)
                {
                    Rows[i].Value += share;
                }
            }
        }

        if (Rows.Length > 1) _rowsFenceGap = (Rows.Length - 1) * Gap.Height;
        if (Columns.Length > 1) _columnsFenceGap = (Columns.Length - 1) * Gap.Width;
    }

    private void UpdateLocking(int rowStart, int rowEnd, int columnStart, int columnEnd)
    {
        rowStart--;
        columnStart--;
        if (rowStart < 0) rowStart = 0;
        if (columnStart < 0) columnStart = 0;
        if (rowEnd > Columns.Length) rowEnd = Columns.Length;
        if (columnEnd > Columns.Length) columnEnd = Columns.Length;

        for (var i = rowStart; i < rowEnd; i++)
        {
            for (var j = columnStart; j < columnEnd; j++)
            {
                Marks[i, j] = true;
            }
        }
    }

    private float GetColumnWidth(int start, int end)
    {
        start--;
        if (start < 0) start = 0;
        if (end > Columns.Length) end = Columns.Length;

        var width = 0f;
        for (var i = start; i < end; i++)
        {
            width = Columns[i].Value;
        }

        return width;
    }

    private float GetRowHeight(int start, int end)
    {
        start--;
        if (start < 0) start = 0;
        if (end > Rows.Length) end = Rows.Length;

        var height = 0f;
        for (var i = start; i < end; i++)
        {
            height = Rows[i].Value;
        }

        return height;
    }

    public sealed override void PostPrepare() { }

    public override void ModifyAvailableSize(UIView view,
        int index, ref float? availableWidth, ref float? availableHeight)
    {
        if (!view.GridArea) return;

        availableWidth = GetColumnWidth(view.ColumnStart, view.ColumnEnd);
        availableHeight = GetRowHeight(view.RowStart, view.RowEnd);
        UpdateLocking(view.RowStart, view.RowEnd, view.ColumnStart, view.ColumnEnd);
    }
}

public struct TemplateDimensions(bool auto, float pixels, float fraction, float percent)
{
    public readonly bool Auto = auto;
    public readonly float Fraction = fraction;
    public readonly float Pixels = pixels;
    public readonly float Percent = percent;

    public float Value { get; set; }

    public void Recalculate(float availableSize)
    {
        Value = Pixels + availableSize * Percent;
    }

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