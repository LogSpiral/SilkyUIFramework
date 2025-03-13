namespace SilkyUIFramework.Animation;

#region Enums

/// <summary>
/// 动画当前的状态
/// </summary>
public enum AnimationTimerStaus
{
    ForwardUpdating,
    ReverseUpdating,
    ForwardCompleted,
    ReverseCompleted
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
    public float Schedule { get; private set; }

    public AnimationTimerStaus Status = AnimationTimerStaus.ReverseCompleted;

    /// <summary>
    /// 在正向更新完成时回调
    /// </summary>
    public event Action OnForwardUpdateCompleted;

    /// <summary>
    /// 在反向更新完成时回调
    /// </summary>
    public event Action OnReverseUpdateCompleted;

    #region IsComplete Updating

    public bool IsComplete => Status is AnimationTimerStaus.ForwardCompleted or AnimationTimerStaus.ReverseCompleted;
    public bool IsUpdating => Status is AnimationTimerStaus.ForwardUpdating or AnimationTimerStaus.ReverseUpdating;

    public bool IsForward =>
        Status is AnimationTimerStaus.ForwardUpdating or AnimationTimerStaus.ForwardCompleted;

    public bool IsForwardUpdating => Status is AnimationTimerStaus.ForwardUpdating;
    public bool IsForwardCompleted => Status is AnimationTimerStaus.ForwardCompleted;

    public bool IsReverse =>
        Status is AnimationTimerStaus.ReverseUpdating or AnimationTimerStaus.ReverseCompleted;

    public bool IsReverseUpdating => Status is AnimationTimerStaus.ReverseUpdating;
    public bool IsReverseCompleted => Status is AnimationTimerStaus.ReverseCompleted;

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
        Status = AnimationTimerStaus.ReverseCompleted;
        OnReverseUpdateCompleted?.Invoke();
    }

    /// <summary>
    /// 直接跳到完全开启状态
    /// </summary>
    public void ImmediateForwardUpdateCompleted()
    {
        Timer = TimerMax;
        Status = AnimationTimerStaus.ForwardCompleted;
        OnForwardUpdateCompleted?.Invoke();
    }

    #endregion

    public void Update(GameTime gameTime)
    {
        var speedFactor = Main.FrameSkipMode == Terraria.Enums.FrameSkipMode.Subtle ? 1f :
            (float)gameTime.ElapsedGameTime.TotalSeconds * 60f;
        switch (Status)
        {
            case AnimationTimerStaus.ForwardUpdating:
            {
                Timer += (TimerMax - Timer) / Speed * speedFactor;

                if (TimerMax - Timer < TimerMax * 0.0001f)
                {
                    Timer = TimerMax;
                    Status = AnimationTimerStaus.ForwardCompleted;
                    OnForwardUpdateCompleted?.Invoke();
                }

                break;
            }
            case AnimationTimerStaus.ReverseUpdating:
            {
                Timer -= Timer / Speed * speedFactor;

                if (Timer < TimerMax * 0.0001f)
                {
                    Timer = 0;
                    Status = AnimationTimerStaus.ReverseCompleted;
                    OnReverseUpdateCompleted?.Invoke();
                }

                break;
            }
        }
        Schedule = Timer / TimerMax;
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