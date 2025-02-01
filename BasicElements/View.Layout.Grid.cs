namespace SilkyUIFramework.BasicElements;

public partial class View
{
    public TemplateUnit[] TemplateRows { get; set; } = [];
    public TemplateUnit[] TemplateColumns { get; set; } = [];

    protected void SortElementGroupsByGrid()
    {
        var rows = TemplateRows?.Length ?? 0;
        var columns = TemplateColumns?.Length ?? 0;

        // 行列都有, 那就只管范围内的
        if (rows > 0 && columns > 0)
        {
            ArrangeDirection = LayoutDirection;
            var elementGroups = new ElementGroup(this);
            ElementGroups.Add(elementGroups);

            var max = ArrangeDirection is LayoutDirection.Column ? rows : columns;
            for (int i = 0; i < FlowElements.Count; i++)
            {
                if (elementGroups.Count >= max)
                {
                    ElementGroups.Add(elementGroups = new ElementGroup(this));
                }

                var size = GetTemplateValue(i);
                elementGroups.Append(FlowElements[i], size.X, size.Y);
            }
        }
        // 只有行的
        else if (rows > 0)
        {
            ArrangeDirection = LayoutDirection.Column;
            var elementGroups = new ElementGroup(this);
            ElementGroups.Add(elementGroups);

            for (int i = 0; i < FlowElements.Count; i++)
            {
                if (elementGroups.Count >= rows)
                {
                    ElementGroups.Add(elementGroups = new ElementGroup(this));
                }

                var size = GetTemplateValue(i);
                elementGroups.Append(FlowElements[i], FlowElements[i]._outerDimensions.Width, size.Y);
            }
        }
        // 只有列的
        else if (columns > 0)
        {
            ArrangeDirection = LayoutDirection.Row;
            var elementGroups = new ElementGroup(this);
            ElementGroups.Add(elementGroups);

            for (int i = 0; i < FlowElements.Count; i++)
            {
                if (elementGroups.Count >= columns)
                    ElementGroups.Add(elementGroups = new ElementGroup(this));

                var size = GetTemplateValue(i);
                elementGroups.Append(FlowElements[i], size.X, FlowElements[i]._outerDimensions.Height);
            }
        }
    }

    public Vector2 CalculateContentSizeByGrid()
    {
        if (ElementGroups is null || ElementGroups.Count == 0)
            return Vector2.Zero;

        if (ArrangeDirection is LayoutDirection.Row)
        {
            var width = ElementGroups.Max(temp => temp.Width);
            var height = ElementGroups.Sum(temp => temp.Height) + (ElementGroups.Count - 1) * Gap.Y;
            return new Vector2(width, height);
        }
        else
        {
            var width = ElementGroups.Sum(temp => temp.Width) + (ElementGroups.Count - 1) * Gap.X;
            var height = ElementGroups.Max(temp => temp.Height);
            return new Vector2(width, height);
        }
    }

    /// <summary>
    /// 计算网格布局
    /// </summary>
    public void RecalculateTemplateUnitsValue()
    {
        var rows = TemplateRows?.Length ?? 0;
        var columns = TemplateColumns?.Length ?? 0;

        // 行
        if (rows > 0)
        {
            var height = _innerDimensions.Height;
            var sumFraction = TemplateRows.Sum(unit => unit.Fraction);

            float cell = sumFraction == 0 ?
                0 : (height - TemplateRows.Sum(unit => unit.Pixels + unit.Percent * height) - (rows - 1) * Gap.Y) / sumFraction;

            for (int i = 0; i < rows; i++)
            {
                TemplateRows[i].RecalculateValue(height, cell);
            }
        }

        // 列
        if (columns > 0)
        {
            var width = _innerDimensions.Width;
            var sumFraction = TemplateColumns.Sum(unit => unit.Fraction);

            float cell = sumFraction == 0 ?
                0 : (width - TemplateColumns.Sum(unit => unit.Pixels + width * unit.Percent) - (columns - 1) * Gap.X) / sumFraction;

            for (int i = 0; i < columns; i++)
            {
                TemplateColumns[i].RecalculateValue(width, cell);
            }
        }
    }

    /// <summary>
    /// 获取网格容器
    /// </summary>
    public Vector2 GetTemplateValue(int flowIndex)
    {
        var rows = TemplateRows?.Length ?? 0;
        var columns = TemplateColumns?.Length ?? 0;

        var container = Vector2.Zero;
        if (columns > 0)
        {
            if (rows > 0)
            {
                container.Y = TemplateRows[Math.Min(flowIndex / rows, rows - 1)].Value;
            }

            container.X = TemplateColumns[Math.Min(flowIndex % columns, columns - 1)].Value;
        }
        else if (rows > 0)
        {
            container.Y = TemplateRows[Math.Min(flowIndex % rows, rows - 1)].Value;
        }

        return container;
    }

    public virtual void ArrangeElementsByGird()
    {
        if (ElementGroups.Count <= 0 || ElementGroups[0].Count <= 0)
            return;

        switch (ArrangeDirection)
        {
            default:
            case LayoutDirection.Row:
            {
                var top = 0f;
                foreach (var elementGroup in ElementGroups)
                {
                    elementGroup.Arrange(0f, top);
                    top += Gap.Y + elementGroup.Height;
                }

                break;
            }
            case LayoutDirection.Column:
            {
                var left = 0f;
                foreach (var elementGroup in ElementGroups)
                {
                    elementGroup.Arrange(left, 0f);
                    left += Gap.X + elementGroup.Width;
                }

                break;
            }
        }
    }
}
