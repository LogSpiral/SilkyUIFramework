namespace SilkyUIFramework.Animation;

#region Enums

/// <summary>
/// 动画当前的状态
/// </summary>
public enum AnimationTimerStaus : byte
{
    ForwardUpdating,
    ReverseUpdating,
    ForwardUpdateCompleted,
    ReverseUpdateCompleted
}

#endregion

/// <summary>
/// 动画计时器 <br/>
/// </summary>
public class AnimationTimer(float speed = 5f, float timerMax = 100f)
{
    /// <summary>
    /// 速度
    /// </summary>
    public float Speed = speed;

    /// <summary>
    /// 当前位置
    /// </summary>
    public float Timer;

    /// <summary>
    /// 最大位置
    /// </summary>
    public float TimerMax = timerMax;

    /// <summary>
    /// 进度
    /// </summary>
    public float Schedule => Timer / TimerMax;

    public AnimationTimerStaus Status = AnimationTimerStaus.ReverseUpdateCompleted;

    /// <summary>
    /// 在正向更新完成时回调
    /// </summary>
    public event Action OnForwardUpdateCompleted;

    /// <summary>
    /// 在反向更新完成时回调
    /// </summary>
    public event Action OnReverseUpdateCompleted;

    #region Member Properties

    public bool IsForward =>
        Status is AnimationTimerStaus.ForwardUpdating or AnimationTimerStaus.ForwardUpdateCompleted;

    public bool ForwardUpdating => Status is AnimationTimerStaus.ForwardUpdating;
    public bool ForwardUpdateCompleted => Status is AnimationTimerStaus.ForwardUpdateCompleted;

    public bool IsReverse =>
        Status is AnimationTimerStaus.ReverseUpdating or AnimationTimerStaus.ReverseUpdateCompleted;

    public bool ReverseUpdating => Status is AnimationTimerStaus.ReverseUpdating;
    public bool ReverseUpdateCompleted => Status is AnimationTimerStaus.ReverseUpdateCompleted;

    #endregion

    #region Member Methods

    /// <summary>
    /// 开始正向更新
    /// </summary>
    public virtual void StartForwardUpdate()
    {
        Status = AnimationTimerStaus.ForwardUpdating;
    }

    /// <summary>
    /// 开启并重置
    /// </summary>
    public virtual void StartForwardUpdateAndReset()
    {
        Timer = 0f;
        Status = AnimationTimerStaus.ForwardUpdating;
    }

    /// <summary>
    /// 开始反向更新
    /// </summary>
    public virtual void StartReverseUpdate()
    {
        Status = AnimationTimerStaus.ReverseUpdating;
    }

    /// <summary>
    /// 关闭并重置
    /// </summary>
    public virtual void StartReverseUpdateAndRest()
    {
        Timer = TimerMax;
        Status = AnimationTimerStaus.ReverseUpdating;
    }

    /// <summary>
    /// 直接跳到完全关闭状态
    /// </summary>
    public void ImmediateReverseUpdateCompleted()
    {
        Timer = 0;
        Status = AnimationTimerStaus.ReverseUpdateCompleted;
        OnReverseUpdateCompleted?.Invoke();
    }

    /// <summary>
    /// 直接跳到完全开启状态
    /// </summary>
    public void ImmediateForwardUpdateCompleted()
    {
        Timer = TimerMax;
        Status = AnimationTimerStaus.ForwardUpdateCompleted;
        OnForwardUpdateCompleted?.Invoke();
    }

    #endregion

    /// <summary>
    /// 更新
    /// </summary>
    public virtual void Update(float speedFactor = 1f)
    {
        switch (Status)
        {
            case AnimationTimerStaus.ForwardUpdating:

                Timer += (TimerMax - Timer) / Speed * speedFactor;

                if (TimerMax - Timer < TimerMax * 0.0001f)
                {
                    Timer = TimerMax;
                    Status = AnimationTimerStaus.ForwardUpdateCompleted;
                    OnForwardUpdateCompleted?.Invoke();
                }

                break;
            case AnimationTimerStaus.ReverseUpdating:

                Timer -= Timer / Speed * speedFactor;

                if (Timer < TimerMax * 0.0001f)
                {
                    Timer = 0;
                    Status = AnimationTimerStaus.ReverseUpdateCompleted;
                    OnReverseUpdateCompleted?.Invoke();
                }

                break;
        }
    }

    #region Lerp Methods

    public Color Lerp(Color color1, Color color2)
    {
        return Color.Lerp(color1, color2, Schedule);
    }

    public Vector2 Lerp(Vector2 vector21, Vector2 vector22)
    {
        return new Vector2(Lerp(vector21.X, vector22.X), Lerp(vector21.Y, vector22.Y));
    }

    public float Lerp(float value1, float value2)
    {
        return value1 + (value2 - value1) * Schedule;
    }

    #endregion

    #region operator

    public static Color operator *(AnimationTimer timer, Color color)
    {
        return color * timer.Schedule;
    }

    public static Color operator *(Color color, AnimationTimer timer)
    {
        return color * timer.Schedule;
    }

    public static Vector2 operator *(AnimationTimer timer, Vector2 vector2)
    {
        return vector2 * timer.Schedule;
    }

    public static Vector2 operator *(Vector2 vector2, AnimationTimer timer)
    {
        return vector2 * timer.Schedule;
    }

    public static float operator *(float number, AnimationTimer timer)
    {
        return number * timer.Schedule;
    }

    public static float operator *(AnimationTimer timer, float number)
    {
        return number * timer.Schedule;
    }

    #endregion
}