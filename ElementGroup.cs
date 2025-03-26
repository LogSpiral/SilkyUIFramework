namespace SilkyUIFramework;

///// <summary>
///// 元素组
///// </summary>
//public class ElementGroup(View view)
//{
//    public readonly View View = view;
//    private readonly List<ElementWrapper> _wrappers = [];
//    public IReadOnlyList<ElementWrapper> Children => _wrappers;
//    public int Count => _wrappers.Count;
//    public float Width { get; set; }
//    public float Height { get; set; }

//    public void Append(View element) =>
//        Append(element, element._outerDimensions.Width, element._outerDimensions.Height);

//    public void Append(View element, float width, float height)
//    {
//        _wrappers.Add(new ElementWrapper(element, width, height));
//        switch (View.ArrangeDirection)
//        {
//            default:
//            case LayoutDirection.Row:
//                if (_wrappers.Count > 1) Width += width + View.Gap.X;
//                else Width += width;

//                Height = MathHelper.Max(height, Height);
//                break;
//            case LayoutDirection.Column:
//                if (_wrappers.Count > 1) Height += height + View.Gap.Y;
//                else Height += height;

//                Width = MathHelper.Max(width, Width);
//                break;
//        }
//    }

//    public bool IsEnoughSpace(View elemment, float max)
//    {
//        if (_wrappers.Count == 0) return true;

//        return View.ArrangeDirection switch
//        {
//            LayoutDirection.Row => Width + View.Gap.X + elemment._outerDimensions.Width <= max,
//            LayoutDirection.Column => Height + View.Gap.Y + elemment._outerDimensions.Height <= max,
//            _ => true,
//        };
//    }

//    public void Arrange(float originLeft, float originTop, float? overrideGap = null)
//    {
//        var gap = overrideGap ?? View.Gap.X;
//        switch (View.ArrangeDirection)
//        {
//            default:
//            case LayoutDirection.Row:
//            {
//                foreach (var wrapper in _wrappers)
//                {
//                    var top = originTop + View.CrossAlignment switch
//                    {
//                        CrossAlignment.Center => (Height - wrapper.View._outerDimensions.Height) / 2,
//                        CrossAlignment.End => Height - wrapper.View._outerDimensions.Height,
//                        _ => 0f,
//                    };

//                    wrapper.View.Offset = new Vector2(originLeft, top);
//                    originLeft += wrapper.Width + gap;
//                }
//                break;
//            }
//            case LayoutDirection.Column:
//            {
//                foreach (var wrapper in _wrappers)
//                {
//                    var left = originLeft + View.CrossAlignment switch
//                    {
//                        CrossAlignment.Center => (Width - wrapper.View._outerDimensions.Width) / 2,
//                        CrossAlignment.End => Width - wrapper.View._outerDimensions.Width,
//                        _ => 0f,
//                    };

//                    wrapper.View.Offset = new Vector2(left, originTop);
//                    originTop += wrapper.Height + gap;
//                }
//                break;
//            }
//        }
//    }
//}

//public readonly struct ElementWrapper
//{
//    public readonly View View;
//    public readonly float Width;
//    public readonly float Height;

//    public ElementWrapper(View view)
//    {
//        View = view;
//        Width = view._outerDimensions.Width;
//        Height = view._outerDimensions.Height;
//    }

//    public ElementWrapper(View view, float width, float height)
//    {
//        View = view;
//        Width = width;
//        Height = height;
//    }

//    // public static implicit operator ElementWrapper(View view)
//    // {
//    //     return new ElementWrapper(view);
//    // }

//    // public static implicit operator View(ElementWrapper elementWrapper)
//    // {
//    //     return elementWrapper.View;
//    // }
//}