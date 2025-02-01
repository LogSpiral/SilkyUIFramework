namespace SilkyUIFramework;

/// <summary>
/// 似乎在密谋着什么，再等等...
/// </summary>
public struct Unit(float pixels = 0f, float percent = 0f)
{
    /// <summary>
    /// 固定值
    /// </summary>
    public float Pixels
    {
        readonly get => _pixels;
        set => _pixels = value;
    }
    private float _pixels = pixels;

    /// <summary>
    /// 父容器的百分比
    /// </summary>
    public float Percent
    {
        readonly get => _percent;
        set => _percent = value;
    }
    private float _percent = percent;

    public void Set(float? pixels = null, float? percent = null)
    {
        _pixels = pixels ?? _pixels;
        _percent = percent ?? _percent;
    }

    public readonly float GetValue(float container)
    {
        return _pixels + container * _percent;
    }
}