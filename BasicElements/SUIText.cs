using System.Text.RegularExpressions;
using SilkyUIFramework.Configs;
using SilkyUIFramework.Helper;
using Terraria.UI.Chat;

namespace SilkyUIFramework.BasicElements;

public class SUIText : View
{
    public static float DeathTextOffset { get; internal set; }
    public static float MouseTextOffset { get; internal set; }

    public void UseDeathText() => Font = FontAssets.DeathText.Value;
    public void UseMouseText() => Font = FontAssets.MouseText.Value;
    public bool IsDeathText => Font == FontAssets.DeathText.Value;
    public bool IsMouseText => Font == FontAssets.MouseText.Value;

    #region 控制属性

    protected bool TextChanges;

    public virtual DynamicSpriteFont Font
    {
        get => _font ?? FontAssets.MouseText.Value;
        set
        {
            if (_font == value) return;
            _font = value;
            TextChanges = true;
        }
    }

    private DynamicSpriteFont _font = FontAssets.MouseText.Value;

    private string _text = string.Empty;
    private bool _wordWrap;
    private int _maxWordLength = 19;
    private int _maxLines = -1;
    private float _textScale = 1f;

    public event Action OnTextChanged;

    public string Text
    {
        get => _text;
        set
        {
            if (_text.Equals(value)) return;
            _text = value;
            TextChanges = true;
            OnTextChanged?.Invoke();
        }
    }

    /// <summary>
    /// 是否自动换行
    /// </summary>
    public bool WordWrap
    {
        get => _wordWrap;
        set
        {
            if (_wordWrap == value) return;
            _wordWrap = value;
            TextChanges = true;
        }
    }

    /// <summary>
    /// 最大单词长度
    /// </summary>
    public int MaxWordLength
    {
        get => _maxWordLength;
        set
        {
            if (_maxWordLength == value) return;
            _maxWordLength = value;
            TextChanges = true;
        }
    }

    public int MaxLines
    {
        get => _maxLines;
        set
        {
            if (_maxLines == value) return;
            _maxLines = value;
            TextChanges = true;
        }
    }

    public float TextScale
    {
        get => _textScale;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_textScale == value) return;
            _textScale = value;
            TextChanges = true;
        }
    }

    public Color TextColor { get; set; } = Color.White;

    public float TextBorder { get; set; } = 2f;

    public Color TextBorderColor { get; set; } = Color.Black;

    public Vector2 TextOffset { get; set; } = Vector2.Zero;
    public Vector2 TextPercentOffset { get; set; } = Vector2.Zero;
    public Vector2 TextPercentOrigin { get; set; } = Vector2.Zero;
    public Vector2 TextAlign { get; set; } = Vector2.Zero;

    #endregion

    protected readonly List<TextSnippet> FinalSnippets = [];

    public Vector2 TextSize { get; protected set; } = Vector2.Zero;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (TextChanges) Recalculate();
    }

    public override Vector2 CalculateContentSize()
    {
        RecalculateText();

        var content = base.CalculateContentSize();
        if (!SpecifyWidth) content.X = TextSize.X * TextScale;
        if (!SpecifyHeight) content.Y = TextSize.Y * TextScale;
        return content;
    }

    protected virtual void RecalculateText()
    {
        // text -> textSnippet -> plainTextSnippet & textSnippetSubclasses
        var parsed = TextSnippetHelper.ParseMessage(Text, TextColor);
        var converted = TextSnippetHelper.ConvertNormalSnippets(parsed);

        // 自动换行 & 指定宽度
        if (WordWrap && SpecifyWidth)
        {
            // 宽度与 TextScale 相关
            var maxWidth = _innerDimensions.Width / TextScale;
            // 进行换行
            TextSnippetHelper.WordwrapString(converted, FinalSnippets,
                TextColor, Font, maxWidth, MaxWordLength, MaxLines);
        }
        else
        {
            FinalSnippets.Clear();
            FinalSnippets.AddRange(converted);
        }

        // 计算文本大小
        TextSize = TextSnippetHelper.GetStringSize(Font, FinalSnippets, new Vector2(1f));
        TextChanges = false;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (TextChanges)
            RecalculateText();
        base.Draw(spriteBatch);
    }

    public override void DrawChildren(SpriteBatch spriteBatch)
    {
        DrawText(spriteBatch, FinalSnippets.ToList());
        base.DrawChildren(spriteBatch);
    }

    protected virtual void DrawText(SpriteBatch spriteBatch, List<TextSnippet> finalSnippets)
    {
        var innerSize = _innerDimensions.Size();

        var textSize = TextSize;
        // 无字符时会出问题，加上这行就好了
        textSize.Y = Math.Max(Font.LineSpacing, textSize.Y);

        var textPos =
            _innerDimensions.Position()
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
            textPos, TextColor, 0f, Vector2.Zero, new Vector2(TextScale), out _, -1f);
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

    protected static readonly Vector2[] ShadowOffsets = [-Vector2.UnitX, Vector2.UnitX, -Vector2.UnitY, Vector2.UnitY];

    protected static void DrawColorCodedStringShadow(SpriteBatch spriteBatch, DynamicSpriteFont font,
        List<TextSnippet> snippets, Vector2 position, Color baseColor, float rotation, Vector2 origin,
        Vector2 baseScale, float maxWidth = -1f,
        float spread = 2f)
    {
        foreach (var offset in ShadowOffsets)
            DrawColorCodedString(spriteBatch, font, snippets, position + offset * spread, baseColor,
                rotation, origin, baseScale, out var _, maxWidth, ignoreColors: true);
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
                        vector2_1.Y += (float)font.LineSpacing * num2 * baseScale.Y;
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
                                if ((double)vector2_1.X - (double)position.X + (double)num3 > (double)maxWidth)
                                {
                                    vector2_1.X = position.X;
                                    vector2_1.Y += (float)font.LineSpacing * num2 * baseScale.Y;
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
                            vector2_1.Y += (float)font.LineSpacing * num2 * baseScale.Y;
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