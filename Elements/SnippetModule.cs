using Terraria.UI.Chat;

namespace SilkyUIFramework.Elements;

/// <summary>
/// 管理换行文本中的行集合。
/// </summary>
public sealed class SnippetModule
{
    public List<SnippetLine> SnippetLines { get; } = [];
    public int Count => SnippetLines.Count;

    #region Properties

    public DynamicSpriteFont Font
    {
        get; private set
        {
            if (value == null || field == value) return;
            field = value;
            _characterSpacing = field.CharacterSpacing;
        }
    }
    public float MaxWidth { get; private set; }
    public int MaxLines { get; private set; }

    public void UpdateProperties(DynamicSpriteFont font, float maxWidth, int maxLines)
    {
        Font = font;
        MaxWidth = maxWidth;
        MaxLines = maxLines;
    }

    #endregion

    private float _characterSpacing;
    public bool IsFull => MaxLines > 0 && SnippetLines.Count >= MaxLines;

    public bool TryCreateNewLine()
    {
        if (IsFull) return false;
        SnippetLines.Add(new SnippetLine());
        return true;
    }

    /// <summary>
    /// 检查是否有足够的空间容纳给定的文本片段。<br/>
    /// 如果当前行没有文本片段，则总是返回 true。
    /// </summary>
    public bool EnoughSpace(float width)
    {
        var current = SnippetLines[^1];
        if (current.Count == 0) return true;
        return current.Width + _characterSpacing + width <= MaxWidth;
    }

    public bool TryAdd(TextSnippet snippet, float width)
    {
        var current = SnippetLines[^1];
        if (current.Count == 0)
        {
            current.Append(snippet, _characterSpacing, width);
            return true;
        }

        if (current.Width + _characterSpacing + width > MaxWidth)
        {
            if (!TryCreateNewLine())
                return false;
            current = SnippetLines[^1];
        }

        current.Append(snippet, _characterSpacing, width);
        return true;
    }

    public bool TryCommitToken(ref SnippetToken token)
    {
        if (token.Word == 0) return true;

        var width = token.Width;
        var snippet = token.ExtractSnippets();

        var current = SnippetLines[^1];
        if (current.Count == 0)
        {
            current.Append(snippet, _characterSpacing, width);
            return true;
        }

        if (current.Width + _characterSpacing + width > MaxWidth)
        {
            if (!TryCreateNewLine())
                return false;
            current = SnippetLines[^1];
        }

        current.Append(snippet, _characterSpacing, width);
        return true;
    }

    public void FromSnippets(List<TextSnippet> snippets)
    {
        SnippetLines.Clear();
        TryCreateNewLine();
        if (Font == null) return;

        var spacing = Font.CharacterSpacing;

        foreach (var snippet in snippets)
        {
            if (snippet is PlainSnippet)
            {
                var last = 0;
                var text = snippet.Text;
                var width = 0f;
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c.Equals('\n'))
                    {
                        if (last < i)
                            SnippetLines[^1].Append(snippet.Copy(text[last..i]), spacing, width);
                        if (!TryCreateNewLine()) return;
                        width = 0f;
                        last = i + 1;
                    }
                    else
                    {
                        if (width > 0) width += spacing;
                        width += Font.GetCharacterMetrics(c).KernedWidth;
                    }
                }

                if (last < text.Length)
                {
                    var part = text[last..];
                    SnippetLines[^1].Append(snippet.Copy(part), spacing, width);
                }
            }
            else
            {
                snippet.UniqueDraw(true, out var size, null);
                SnippetLines[^1].Append(snippet, spacing, size.X);
            }
        }
    }

    public void WordWrapSnippets(List<TextSnippet> snippets)
    {
        SnippetLines.Clear();
        if (Font == null) return;

        TryCreateNewLine();

        if (snippets is null || snippets.Count == 0) return;

        var font = Font;
        var token = new SnippetToken(MaxWidth, font.CharacterSpacing);

        foreach (var snippet in snippets)
        {
            if (snippet is CursorSnippet)
            {
                token.Snippets.Add(snippet);
                token.Append(0, false);
            }
            else if (snippet is PlainSnippet plainSnippet)
            {
                token.Snippets.Add(plainSnippet);

                foreach (var c in snippet.Text)
                {
                    var metrics = font.GetCharacterMetrics(c);

                    // 1. 判断当前字符是否为空白字符
                    bool isWhiteSpace = char.IsWhiteSpace(c);

                    if (isWhiteSpace != token.IsWhiteSpace)
                    {
                        if (!TryCommitToken(ref token)) return;
                    }

                    token.IsWhiteSpace = isWhiteSpace;

                    if (isWhiteSpace)
                    {
                        if (c.Equals('\n'))
                        {
                            if (!TryCommitToken(ref token)) return;
                            token.Append(0);
                            TryCreateNewLine();
                        }
                        else
                        {
                            // 空格时候判断是否足够放入剩余空间
                            if (!EnoughSpace(token.Width + metrics.KernedWidth))
                            {
                                if (!TryCommitToken(ref token)) return;
                            }

                            token.Append(metrics.KernedWidth);
                        }
                    }
                    else
                    {
                        if (!token.TryAdd(metrics.KernedWidth))
                        {
                            if (!TryCommitToken(ref token)) return;
                            token.Append(metrics.KernedWidth);
                        }
                    }
                }
            }
            else
            {
                if (!TryCommitToken(ref token)) return;

                var width = snippet.GetStringLength(font);

                if (!TryAdd(snippet, width))
                    return;
            }
        }

        TryCommitToken(ref token);
    }

    /// <summary>
    /// 计算当前管理器中所有行的最终渲染尺寸。
    /// </summary>
    /// <param name="font">用于获取行高的字体。</param>
    /// <param name="baseScale">应用于所有文本的基础缩放比例。</param>
    /// <returns>一个 Vector2，其中 X 是最宽行的宽度，Y 是所有行的总高度，均已应用基础缩放。</returns>
    public Vector2 GetStringSize(DynamicSpriteFont font, Vector2 baseScale)
    {
        if (SnippetLines.Count == 0) return new Vector2(0, font.LineSpacing * baseScale.Y);

        var size = Vector2.Zero;

        foreach (var line in SnippetLines)
        {
            size.X = Math.Max(size.X, line.Width);
            size.Y += line.Snippets.Count > 0 ?
                font.LineSpacing * line.Snippets.Max(snippet => snippet.Scale) : font.LineSpacing;
        }

        return size * baseScale;
    }

    /// <summary>
    /// 绘制已经过布局的所有文本行。
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch。</param>
    /// <param name="font">用于绘制文本的字体。</param>
    /// <param name="position">整个文本块的起始绘制位置（左上角）。</param>
    /// <param name="baseColor">基础颜色，用于忽略颜色或作为默认色。</param>
    /// <param name="rotation">文本旋转角度。</param>
    /// <param name="origin">文本旋转的原点。</param>
    /// <param name="baseScale">应用于所有文本的基础缩放比例。</param>
    /// <param name="hoveredSnippet">输出参数，返回鼠标悬停的 TextSnippet。</param>
    /// <param name="ignoreColors">如果为 true，则所有文本都使用 baseColor 绘制。</param>
    public void DrawText(SpriteBatch spriteBatch, DynamicSpriteFont font, Vector2 position, Color baseColor,
        float rotation, Vector2 origin, Vector2 baseScale, out TextSnippet hoveredSnippet, bool ignoreColors = false)
    {
        hoveredSnippet = null;
        if (baseColor == Color.Transparent) return;

        var currentPosition = position;

        foreach (var line in SnippetLines)
        {
            if (line.Snippets.Count == 0)
            {
                currentPosition.Y += font.LineSpacing;
                continue;
            }

            var maxScale = line.Snippets.Max(l => l.Scale);

            foreach (var snippet in line.Snippets)
            {

                snippet.Update();

                var snippetColor = ignoreColors ?
                    baseColor : Color.FromNonPremultiplied(snippet.GetVisibleColor().ToVector4() * baseColor.ToVector4());

                var scale = snippet.Scale * baseScale;

                if (snippet is CursorSnippet cursor)
                {
                    cursor.Height = font.LineSpacing;
                }

                if (!snippet.UniqueDraw(false, out var snippetSize, spriteBatch, currentPosition, snippetColor, scale.X))
                {
                    spriteBatch.DrawString(font, snippet.Text, currentPosition, snippetColor, rotation, origin, scale.X, SpriteEffects.None, 0.0f);
                    snippetSize = font.MeasureString(snippet.Text) * scale.X;
                }

                // 鼠标悬浮检测
                if (hoveredSnippet == null)
                {
                    if (new Bounds(currentPosition, snippetSize).Contains(Main.MouseScreen))
                    {
                        hoveredSnippet = snippet;
                    }
                }

                currentPosition.X += _characterSpacing * scale.X + snippetSize.X;
            }

            currentPosition.X = position.X;
            currentPosition.Y += font.LineSpacing * maxScale * baseScale.Y;
        }
    }

    public static readonly Vector2[] ShadowOffsets = [-Vector2.UnitX, Vector2.UnitX, -Vector2.UnitY, Vector2.UnitY];

    public void DrawTextShadow(SpriteBatch spriteBatch, DynamicSpriteFont font, Vector2 position, Color baseColor,
        float rotation, Vector2 origin, Vector2 baseScale, float spread = 2f)
    {
        var span = ShadowOffsets.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            DrawText(spriteBatch, font, position + span[i] * spread,
                baseColor, rotation, origin, baseScale, out _, ignoreColors: true);
        }
    }
}
