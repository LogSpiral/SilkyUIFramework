namespace SilkyUIFramework;

/// <summary>
/// 表示一个定位锚点，包含像素偏移、百分比偏移和对齐比例
/// </summary>
public readonly struct Anchor(float pixels = 0f, float percent = 0f, float alignment = 0f) : IEquatable<Anchor>
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
}