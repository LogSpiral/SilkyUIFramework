namespace SilkyUIFramework;

public struct Bounds
{
    public static Bounds Zero { get; } = new(0, 0, 0, 0);

    private float _x, _y, _width, _height;

    public Bounds(float x, float y, float width, float height)
    {
        _x = x; _y = y;
        _width = width; _height = height;
    }

    public Bounds(Vector2 position, Size size)
    {
        _x = position.X; _y = position.Y;
        _width = size.Width; _height = size.Height;
    }

    public float X { readonly get => _x; set => _x = value; }
    public float Y { readonly get => _y; set => _y = value; }
    public float Width { readonly get => _width; set => _width = value; }
    public float Height { readonly get => _height; set => _height = value; }

    public Vector2 Position
    {
        readonly get => new(_x, _y); set
        {
            _x = value.X;
            _y = value.Y;
        }
    }

    public Size Size
    {
        readonly get => new(_width, _height); set
        {
            _width = value.Width;
            _height = value.Height;
        }
    }

    public readonly float Right => _x + _width;
    public readonly float Bottom => _y + _height;
    public readonly Vector2 Center => new(_x + _width / 2f, _y + _height / 2f);
    public readonly Vector2 TopLeft => new(_x, _y);
    public readonly Vector2 TopRight => new(_x + _width, _y);
    public readonly Vector2 BottomLeft => new(_x, _y + _height);
    public readonly Vector2 BottomRight => new(_x + _width, _y + _height);

    public static implicit operator Bounds(Rectangle rectangle)
    {
        return new Bounds(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    public static implicit operator Rectangle(Bounds bounds)
    {
        return new Rectangle((int)bounds._x, (int)bounds._y, (int)bounds._width, (int)bounds._height);
    }

    /// <summary>
    /// 判断点是否在内
    /// </summary>
    public readonly bool Contains(Vector2 point)
    {
        return point.X >= _x &&
               point.X <= _x + _width &&
               point.Y >= _y &&
               point.Y <= _y + _height;
    }

    /// <summary>
    /// 判断两个矩形是否有重叠部分
    /// </summary>
    public readonly bool Intersects(Bounds other)
    {
        return _x < other._x + other._width &&
               _x + _width > other._x &&
               _y < other._y + other._height &&
               _y + _height > other._y;
    }

    public override readonly string ToString()
    {
        return $"Bounds: {_x}, {_y}, {_width}, {_height}";
    }
}