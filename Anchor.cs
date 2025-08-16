using System.Diagnostics.CodeAnalysis;

namespace SilkyUIFramework;

/// <summary>
/// 表示一个定位锚点，包含像素偏移、百分比偏移和对齐比例
/// </summary>
public readonly struct Anchor(float pixels = 0f, float percent = 0f, float alignment = 0f) : IEquatable<Anchor>, IParsable<Anchor>
{
    public float Pixels { get; } = pixels;
    public float Percent { get; } = percent;
    public float Alignment { get; } = alignment;

    public Anchor With(float? pixels = null, float? percent = null, float? alignment = null)
    {
        return new Anchor(pixels ?? Pixels, percent ?? Percent, alignment ?? Alignment);
    }

    /// <summary>
    /// 计算最终位置（需容器尺寸和对齐轴长度）
    /// </summary>
    /// <param name="containerSize">容器在对应轴的长度</param>
    /// <param name="elementSize">元素自身的尺寸（用于对齐计算）</param>
    public float CalculatePosition(float containerSize, float elementSize = 0)
    {
        // 基础偏移 = 绝对偏移 + 容器尺寸的百分比偏移
        var baseOffset = Pixels + containerSize * Percent;
        // 对齐调整 = (容器尺寸 - 元素尺寸) * 对齐比例
        var alignmentOffset = (containerSize - elementSize) * Alignment;

        return baseOffset + alignmentOffset;
    }

    //========= 运算符重载 =========//
    public static bool operator ==(Anchor left, Anchor right) => left.Equals(right);
    public static bool operator !=(Anchor left, Anchor right) => !left.Equals(right);

    public bool Equals(Anchor other) =>
        Pixels.Equals(other.Pixels) &&
        Percent.Equals(other.Percent) &&
        Alignment.Equals(other.Alignment);

    public override bool Equals(object obj) => obj is Anchor other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Pixels, Percent, Alignment);

    public override string ToString()
    {
        return $"{Pixels}px, {Percent}%, {Alignment}";
    }

    public static Anchor Parse(string s, IFormatProvider provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(s, nameof(s));

        var parts = s.Split(' ');
        switch (parts.Length)
        {
            case 1:
            {
                if (parts[0].EndsWith("px"))
                {
                    var value = float.Parse(parts[0].TrimEnd("px"), provider);
                    return new Anchor(0f, value, 0f);
                }
                else if (parts[0].EndsWith('%'))
                {
                    var value = float.Parse(parts[0].TrimEnd('%'), provider);
                    return new Anchor(0f, value, 0f);
                }
                else if (parts[0].EndsWith('#'))
                {
                    var value = float.Parse(parts[0].TrimEnd('#'), provider);
                    return new Anchor(0f, 0f, value);
                }

                throw new FormatException("Invalid anchor format. Expected format: 'pixels, percent%, alignment'");
            }
            case 3:
            {
                var arg1 = float.Parse(parts[0].TrimEnd("px"), provider);
                var arg2 = float.Parse(parts[1].TrimEnd('%'), provider);
                var arg3 = float.Parse(parts[2].TrimEnd('#'), provider);

                return new Anchor(arg1, arg2, arg3);
            }
            default:
                throw new FormatException("Invalid anchor format. Expected format: 'pixels, percent%, alignment'");
        }
    }

    public static bool TryParse([NotNullWhen(true)] string s, IFormatProvider provider, [MaybeNullWhen(false)] out Anchor result)
    {
        result = new Anchor();
        if (string.IsNullOrWhiteSpace(s)) return false;

        var parts = s.Split(' ');
        switch (parts.Length)
        {
            case 1:
            {
                if (parts[0].EndsWith("px") && float.TryParse(parts[0].TrimEnd("px"), out var arg1))
                {
                    result = new Anchor(0f, arg1, 0f);
                    return true;
                }
                else if (parts[0].EndsWith('%') && float.TryParse(parts[0].TrimEnd('%'), out var arg2))
                {
                    result = new Anchor(0f, arg2, 0f);
                    return true;
                }
                else if (parts[0].EndsWith("px") && float.TryParse(parts[0].TrimEnd("px"), out var arg3))
                {
                    result = new Anchor(0f, arg3, 0f);
                    return true;
                }

                return false;
            }
            case 3:
            {
                if (float.TryParse(parts[0].TrimEnd("px"), out var arg1) &&
                    float.TryParse(parts[1].TrimEnd('%'), out var arg2) &&
                    float.TryParse(parts[0].TrimEnd("px"), out var arg3))
                {
                    result = new Anchor(arg1, arg2, arg3);
                    return true;
                }

                return false;
            }
            default: return false;
        }
    }
}