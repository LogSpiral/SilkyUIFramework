using System.Text.RegularExpressions;
using Terraria.UI.Chat;

namespace SilkyUIFramework.BasicElements;

public class ContentChangingEventArgs(string newText, string oldText) : EventArgs
{
    public string NewText { get; set; } = newText;
    public string OldText { get; } = oldText;
}


public class ContentChangedEventArgs(string text) : EventArgs
{
    public string Text { get; } = text;
}


public delegate string ContentChangingEventHandler(UITextView sender, ContentChangingEventArgs e);
public delegate void ContentChangedEventHandler(UITextView sender, ContentChangedEventArgs e);

public class UITextView : UIView
{
    public static readonly Vector2[] ShadowOffsets = [-Vector2.UnitX, Vector2.UnitX, -Vector2.UnitY, Vector2.UnitY];
    public static float DeathTextOffset { get; internal set; }
    public static float MouseTextOffset { get; internal set; }

    public void UseDeathText() => Font = FontAssets.DeathText.Value;
    public void UseMouseText() => Font = FontAssets.MouseText.Value;
    public bool IsDeathText => Font == FontAssets.DeathText.Value;
    public bool IsMouseText => Font == FontAssets.MouseText.Value;

    #region 控制属性

    public virtual DynamicSpriteFont Font
    {
        get => _font ?? FontAssets.MouseText.Value;
        set
        {
            if (_font == value) return;
            _font = value;
            MarkLayoutDirty();
        }
    }

    private DynamicSpriteFont _font = FontAssets.MouseText.Value;

    /// <summary>
    /// 当输入内容更改时触发
    /// </summary>
    public event ContentChangingEventHandler ContentChanging;

    /// <summary>
    /// 当内容更改后触发
    /// </summary>
    public event ContentChangedEventHandler ContentChanged;

    /// <summary>
    /// 当输入内容更改时触发
    /// </summary>
    /// <returns>新值</returns>
    protected virtual string OnContentChanging(string newText, string oldText) { return newText; }

    /// <summary>
    /// 当内容更改后触发
    /// </summary>
    protected virtual void OnContentChanged(string text) { }

    public virtual string Text
    {
        get => field;
        set
        {
            if (field?.Equals(value) ?? value is null) return;

            if (ContentChanging != null)
                value = ContentChanging.Invoke(this, new ContentChangingEventArgs(value, field));
            value = OnContentChanging(value, field);

            if (field?.Equals(value) ?? value is null) return;

            field = value;
            MarkLayoutDirty();

            ContentChanged?.Invoke(this, new ContentChangedEventArgs(field));
            OnContentChanged(field);
        }
    } = string.Empty;

    /// <summary>
    /// 是否自动换行
    /// </summary>
    public bool WordWrap
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    }

    /// <summary>
    /// 最大单词长度
    /// </summary>
    public int MaxWordLength
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    } = 19;

    public int MaxLines
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    } = -1;

    public float TextScale
    {
        get => field;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (field == value) return;
            field = value;
            MarkLayoutDirty();
        }
    } = 1f;

    public Color TextColor { get; set; } = Color.White;
    public float TextBorder { get; set; } = 2f;
    public Color TextBorderColor { get; set; } = Color.Black;

    public Vector2 TextOffset { get; set; } = Vector2.Zero;
    public Vector2 TextPercentOffset { get; set; } = Vector2.Zero;
    public Vector2 TextPercentOrigin { get; set; } = Vector2.Zero;
    public Vector2 TextAlign { get; set; } = Vector2.Zero;
    public bool IgnoreTextColor { get; set; } = false;

    #endregion

    protected readonly List<TextSnippet> FinalSnippets = [];

    public UITextView()
    {
        Padding = 2;
        FitWidth = true;
        FitHeight = true;
    }

    public Vector2 TextSize { get; protected set; } = Vector2.Zero;

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Prepare(float? width, float? height)
    {
        ComputeWidthConstraint(width ?? 0);
        ComputeHeightConstraint(height ?? 0);

        if (FitWidth)
        {
            RecalculateText(MaxInnerWidth);
            DefineInnerBoundsWidth(MathHelper.Clamp(TextSize.X * TextScale, MinInnerWidth, MaxInnerWidth));
        }
        else
        {
            RecalculateBoundsWidth(width ?? 0);
            RecalculateText(InnerBounds.Width);
        }

        if (FitHeight)
        {
            DefineInnerBoundsHeight(MathHelper.Clamp(TextSize.Y * TextScale, MinInnerHeight, MaxInnerHeight));
        }
        else
        {
            RecalculateBoundsHeight(height ?? 0);
        }
    }

    public override void RecalculateHeight()
    {
        RecalculateText(InnerBounds.Width);
        if (FitHeight)
        {
            DefineInnerBoundsHeight(MathHelper.Clamp(TextSize.Y * TextScale, MinInnerHeight, MaxInnerHeight));
        }
    }

    protected virtual void RecalculateText(float maxWidth)
    {
        // text -> textSnippet -> plainTextSnippet & textSnippetSubclasses
        var parsed = TextSnippetHelper.ParseMessage(Text, TextColor);
        var converted = TextSnippetHelper.ConvertNormalSnippets(parsed);

        // 自动换行 & 指定宽度
        if (WordWrap)
        {
            // 进行换行
            TextSnippetHelper.WordWrapString(converted,
                FinalSnippets, Color.White, Font, maxWidth, MaxWordLength, MaxLines);
        }
        else
        {
            FinalSnippets.Clear();
            FinalSnippets.AddRange(converted);
        }

        // 计算文本大小
        TextSize = TextSnippetHelper.GetStringSize(Font, FinalSnippets, new Vector2(1f));
    }

    protected override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);
        DrawText(spriteBatch, [.. FinalSnippets]);
    }

    protected virtual void DrawText(SpriteBatch spriteBatch, List<TextSnippet> finalSnippets)
    {
        var innerSize = (Vector2)InnerBounds.Size;

        var textSize = TextSize;
        // 无字符时会出问题，加上这行就好了
        textSize.Y = Math.Max(Font.LineSpacing, textSize.Y);

        var textPos =
            InnerBounds.Position
            + TextOffset
            + TextPercentOffset * innerSize
            + TextAlign * (innerSize - textSize * TextScale)
            - TextPercentOrigin * TextSize * TextScale;
        var fontOffset = GetFontOffset();
        textPos.Y += TextScale * fontOffset;

        DrawTextShadow(spriteBatch, finalSnippets, textPos);
        DrawTextSelf(spriteBatch, finalSnippets, textPos);
    }

    protected virtual void DrawTextShadow
        (SpriteBatch spriteBatch, List<TextSnippet> finalSnippets, Vector2 textPos)
    {
        DrawColorCodedStringShadow(spriteBatch, Font, finalSnippets,
            textPos, TextBorderColor, 0f, Vector2.Zero, new Vector2(TextScale), spread: TextBorder);
    }

    protected virtual void DrawTextSelf
        (SpriteBatch spriteBatch, List<TextSnippet> finalSnippets, Vector2 textPos)
    {
        DrawColorCodedString(spriteBatch, Font, finalSnippets,
            textPos, TextColor, 0f, Vector2.Zero, new Vector2(TextScale), out _, -1f, ignoreColors: IgnoreTextColor);
    }

    protected virtual float GetFontOffset()
    {
        if (IsDeathText)
        {
            return DeathTextOffset;
        }
        else if (IsMouseText)
        {
            return MouseTextOffset;
        }

        return 0f;
    }

    protected static void DrawColorCodedStringShadow(SpriteBatch spriteBatch, DynamicSpriteFont font,
        List<TextSnippet> snippets, Vector2 position, Color baseColor, float rotation, Vector2 origin,
        Vector2 baseScale, float maxWidth = -1f,
        float spread = 2f)
    {
        foreach (var offset in ShadowOffsets)
        {
            DrawColorCodedString(spriteBatch, font, snippets, position + offset * spread, baseColor,
                rotation, origin, baseScale, out var _, maxWidth, ignoreColors: true);
        }
    }

    protected static Vector2 DrawColorCodedString(SpriteBatch spriteBatch, DynamicSpriteFont font,
        List<TextSnippet> snippets,
        Vector2 position, Color baseColor, float rotation, Vector2 origin, Vector2 baseScale, out int hoveredSnippet,
        float maxWidth, bool ignoreColors = false)
    {
        if (baseColor == Color.Transparent)
        {
            hoveredSnippet = -1;
            return Vector2.Zero;
        }

        var num1 = -1;
        var vec = new Vector2(Main.mouseX, Main.mouseY);
        var vector2_1 = position;
        var vector2_2 = vector2_1;
        var x = font.MeasureString(" ").X;
        var color = baseColor;
        var num2 = 0.0f;
        for (var index1 = 0; index1 < snippets.Count; ++index1)
        {
            var snippet = snippets[index1];
            snippet.Update();
            if (!ignoreColors)
                color = snippet.GetVisibleColor();
            var scale = snippet.Scale;
            if (snippet.UniqueDraw(false, out var size, spriteBatch, vector2_1, color, baseScale.X * scale))
            {
                if (vec.Between(vector2_1, vector2_1 + size))
                    num1 = index1;
                vector2_1.X += size.X;
                vector2_2.X = Math.Max(vector2_2.X, vector2_1.X);
            }
            else
            {
                snippet.Text.Split('\n');
                string[] strArray1 = Regex.Split(snippet.Text, "(\n)");
                bool flag = true;
                foreach (string input in strArray1)
                {
                    Regex.Split(input, "( )");
                    string[] strArray2 = input.Split(' ');
                    if (input == "\n")
                    {
                        vector2_1.Y += font.LineSpacing * num2 * baseScale.Y;
                        vector2_1.X = position.X;
                        vector2_2.Y = Math.Max(vector2_2.Y, vector2_1.Y);
                        num2 = 0.0f;
                        flag = false;
                    }
                    else
                    {
                        for (int index2 = 0; index2 < strArray2.Length; ++index2)
                        {
                            if (index2 != 0)
                                vector2_1.X += x * baseScale.X * scale;
                            if ((double)maxWidth > 0.0)
                            {
                                float num3 = font.MeasureString(strArray2[index2]).X * baseScale.X * scale;
                                if (vector2_1.X - (double)position.X + (double)num3 > (double)maxWidth)
                                {
                                    vector2_1.X = position.X;
                                    vector2_1.Y += font.LineSpacing * num2 * baseScale.Y;
                                    vector2_2.Y = Math.Max(vector2_2.Y, vector2_1.Y);
                                    num2 = 0.0f;
                                }
                            }

                            if ((double)num2 < (double)scale)
                                num2 = scale;
                            spriteBatch.DrawString(font, strArray2[index2], vector2_1, color, rotation, origin,
                                baseScale * snippet.Scale * scale, SpriteEffects.None, 0.0f);
                            Vector2 vector2_3 = font.MeasureString(strArray2[index2]);
                            if (vec.Between(vector2_1, vector2_1 + vector2_3))
                                num1 = index1;
                            vector2_1.X += vector2_3.X * baseScale.X * scale;
                            vector2_2.X = Math.Max(vector2_2.X, vector2_1.X);
                        }

                        if (strArray1.Length > 1 & flag)
                        {
                            vector2_1.Y += font.LineSpacing * num2 * baseScale.Y;
                            vector2_1.X = position.X;
                            vector2_2.Y = Math.Max(vector2_2.Y, vector2_1.Y);
                            num2 = 0.0f;
                        }

                        flag = true;
                    }
                }
            }
        }

        hoveredSnippet = num1;
        return vector2_2;
    }
}