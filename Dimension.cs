namespace SilkyUIFramework;

public readonly struct Dimension(float pixels = 0f, float percent = 0f) : IEquatable<Dimension>
{
    public float Pixels { get; } = pixels;
    public float Percent { get; } = percent;

    public float CalculateSize(float containerSize) => Pixels + containerSize * Percent;

    public Dimension With(float? pixels = null, float? percent = null) =>
        new(pixels ?? Pixels, percent ?? Percent);

    public static bool operator ==(Dimension left, Dimension right) => left.Equals(right);
    public static bool operator !=(Dimension left, Dimension right) => !left.Equals(right);

    public bool Equals(Dimension other) => Pixels.Equals(other.Pixels) && Percent.Equals(other.Percent);
    public override bool Equals(object obj) => obj is Dimension other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Pixels, Percent);

    public override string ToString() => $"{Pixels}px {Percent}%";
}