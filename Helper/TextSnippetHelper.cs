using System.Text.RegularExpressions;
using Terraria.UI.Chat;

namespace SilkyUIFramework.Helpers;

public static class TextSnippetHelper
{
    public static Vector2 GetStringSize(
        DynamicSpriteFont font,
        List<TextSnippet> snippets,
        Vector2 baseScale,
        float maxWidth = -1f)
    {
        Vector2 vec = new Vector2((float)Main.mouseX, (float)Main.mouseY);
        Vector2 zero = Vector2.Zero;
        Vector2 minimum = zero;
        Vector2 stringSize = minimum;
        float x = font.MeasureString(" ").X;
        float num1 = 0.0f;
        foreach (TextSnippet snippet in snippets)
        {
            snippet.Update();
            float scale1 = snippet.Scale;
            TextSnippet textSnippet = snippet;
            Vector2 vector2_1 = Vector2.Zero;
            ref Vector2 local = ref vector2_1;
            float num2 = baseScale.X * scale1;
            Vector2 position = new Vector2();
            Color color = new Color();
            double scale2 = (double)num2;
            if (textSnippet.UniqueDraw(true, out local, (SpriteBatch)null, position, color, (float)scale2))
            {
                minimum.X += vector2_1.X;
                stringSize.X = Math.Max(stringSize.X, minimum.X);
                stringSize.Y = Math.Max(stringSize.Y, minimum.Y + vector2_1.Y);
            }
            else
            {
                string[] strArray1 = snippet.Text.Split('\n');
                string[] strArray2 = strArray1;
                for (int index1 = 0; index1 < strArray2.Length; ++index1)
                {
                    string[] strArray3 = strArray2[index1].Split(' ');
                    for (int index2 = 0; index2 < strArray3.Length; ++index2)
                    {
                        if (index2 != 0)
                            minimum.X += x * baseScale.X * scale1;
                        if ((double)maxWidth > 0.0)
                        {
                            float num3 = font.MeasureString(strArray3[index2]).X * baseScale.X * scale1;
                            if ((double)minimum.X - (double)zero.X + (double)num3 > (double)maxWidth)
                            {
                                minimum.X = zero.X;
                                minimum.Y += (float)font.LineSpacing * num1 * baseScale.Y;
                                stringSize.Y = Math.Max(stringSize.Y, minimum.Y);
                                num1 = 0.0f;
                            }
                        }

                        if ((double)num1 < (double)scale1)
                            num1 = scale1;
                        Vector2 vector2_2 = font.MeasureString(strArray3[index2]);
                        vec.Between(minimum, minimum + vector2_2);
                        minimum.X += vector2_2.X * baseScale.X * scale1;
                        stringSize.X = Math.Max(stringSize.X, minimum.X);
                        stringSize.Y = Math.Max(stringSize.Y, minimum.Y + vector2_2.Y);
                    }

                    if (strArray1.Length > 1 && index1 < strArray2.Length - 1)
                    {
                        minimum.X = zero.X;
                        minimum.Y += (float)font.LineSpacing * num1 * baseScale.Y;
                        stringSize.Y = Math.Max(stringSize.Y, minimum.Y);
                        num1 = 0.0f;
                    }
                }
            }
        }

        return stringSize;
    }

    /// <summary>
    /// 将文本转换为文本片段 <br/>
    /// 根据格式: [tag/options:text] 拆分
    /// </summary>
    public static List<TextSnippet> ParseMessage(string input, Color baseColor)
    {
        // 删除文本中回车 (怎么会有回车捏?)
        input = input.Replace("\r", "");

        // 创建正则列表
        var matchCollection = ChatManager.Regexes.Format.Matches(input);

        // 文字片段列表
        List<TextSnippet> snippets = [];
        var inputIndex = 0;

        // 遍历匹配到的正则之间的文本
        foreach (var match in matchCollection.Cast<Match>())
        {
            // match.Index 是该匹配到的正则在原文中的其实位置
            // match.Length 是该正则在原文中的长度

            // 如果有, 添加两正则之间的文本.
            if (match.Index > inputIndex)
            {
                // AddTextSnippetWithCursorCheck(input[inputIndex..match.Index], snippets, baseColor);
                snippets.Add(new TextSnippet(input[inputIndex..match.Index], baseColor));
            }

            // 移动下标至当前正则后的第一个字符
            inputIndex = match.Index + match.Length;

            // 获取指定文本
            var tag = match.Groups["tag"].Value;
            var text = match.Groups["text"].Value;
            var options = match.Groups["options"].Value;

            var handler = ChatManager.GetHandler(tag);

            // 没有, 插入文本
            if (handler is null)
            {
                snippets.Add(new TextSnippet(text, baseColor));
            }
            // 如果有, 按照处理程序设定插入
            else
            {
                var snippet = handler.Parse(text, baseColor, options);
                snippet.TextOriginal = match.ToString();
                snippets.Add(snippet);
            }
        }

        // 如果有, 添加最后一个正则后的文本.
        if (input.Length > inputIndex)
        {
            // AddTextSnippetWithCursorCheck(input[inputIndex..], snippets, baseColor);
            snippets.Add(new TextSnippet(input[inputIndex..], baseColor));
        }

        return snippets;
    }

    /// <summary>
    /// 把 TextSnippet 转换为 PlainSnippet<br/>
    /// 因为 TextSnippet 文字颜色会闪烁.
    /// </summary>
    public static List<TextSnippet> ConvertNormalSnippets(List<TextSnippet> originalSnippets)
    {
        List<TextSnippet> finalSnippets = [];
        finalSnippets.AddRange(originalSnippets.Select(snippet => snippet.GetType() == typeof(TextSnippet)
            ? new PlainSnippet(snippet.Text, snippet.Color, snippet.Scale)
            : snippet));
        return finalSnippets;
    }

    public class TextLine
    {
        public readonly List<TextSnippet> Snippets = [];
        public float Width { get; set; }
        public float Height { get; set; }
    }

    /// <summary> TextSnippet 特殊文本换行 </summary>
    public static void WordWrapString(List<TextSnippet> original, List<TextSnippet> final,
        Color textColor, DynamicSpriteFont font, float maxWidth, int maxWordLength = 19, int maxLines = -1)
    {
        final.Clear();

        var lineCount = 0;
        var currentLineLength = 0f;

        foreach (var snippet in original)
        {
            // 普通文本
            if (snippet is PlainSnippet)
            {
                var cacheString = ""; // 缓存字符串 - 准备输入的字符

                // 遍历字符
                for (var i = 0; i < snippet.Text.Length; i++)
                {
                    var character = snippet.Text[i];
                    var characterMetrics = font.GetCharacterMetrics(character);
                    currentLineLength += font.CharacterSpacing + characterMetrics.KernedWidth;

                    if (currentLineLength > maxWidth && !char.IsWhiteSpace(character))
                    {
                        // 如果第一个字符是空格，单词长度小于19（实际上是18因为第一个字符为空格），可以空格换行
                        var canWrapWord = cacheString.Length > 1 && cacheString.Length < maxWordLength;

                        // 找不到空格，或者拆腻子，则强制换行
                        if (!canWrapWord || i > 0 && CanBreakBetween(snippet.Text[i - 1], character))
                        {
                            final.Add(new PlainSnippet(cacheString, snippet.Color));
                            final.AddLineBreak();
                            currentLineLength = characterMetrics.KernedWidthOnNewLine;
                            cacheString = "";
                            lineCount++;
                        }
                        // 空格换行
                        else
                        {
                            // 由于下面那一段“将CJK字符与非CJK字符分割”可能会导致空格换行后的第一个字符不是空格，所以这里手动加一个空格
                            // 就不改下面的cacheString[1..]了
                            if (cacheString[0] != ' ')
                                cacheString = " " + cacheString;
                            final.Add(new PlainSnippet(cacheString[1..], snippet.Color));
                            final.AddLineBreak();
                            currentLineLength = font.MeasureString(cacheString).X;
                            cacheString = "";
                            lineCount++;
                        }
                    }

                    // 这么做可以分割单词，并且使自然分割单词（即不因换行过长强制分割的单词）第一个字符总是空格
                    // 或者是将CJK字符与非CJK字符分割

                    if (!string.IsNullOrEmpty(cacheString) &&
                        (char.IsWhiteSpace(character) || IsCjk(cacheString[^1]) != IsCjk(character)))
                    {
                        final.AddPlainSnippet(cacheString, snippet.Color);
                        cacheString = "";
                    }

                    // 原有换行则将当前行长度重置
                    if (character is '\n')
                    {
                        currentLineLength = 0;
                        lineCount++;
                    }

                    cacheString += character;
                }

                final.AddPlainSnippet(cacheString, snippet.Color);
            }
            // 富文本
            else
            {
                var length = snippet.GetStringLength(font);
                currentLineLength += length;

                if (currentLineLength > maxWidth)
                {
                    final.AddLineBreak();
                    lineCount++;
                    currentLineLength = length;
                }

                final.Add(snippet);
            }

            // 限制行数
            if (maxLines == -1 || lineCount < maxLines) continue;
            RemoveRowFromBackToFront(final, lineCount - maxLines);
        }
    }

    private static void AddLineBreak(this List<TextSnippet> snippets) =>
        snippets.Add(new PlainSnippet("\n"));

    private static void AddPlainSnippet(this List<TextSnippet> snippets, string text, Color color) =>
        snippets.Add(new PlainSnippet(text, color));

    // 从后向前删除行
    private static void RemoveRowFromBackToFront(List<TextSnippet> final, int rows)
    {
        for (var i = 0; i < rows; i++)
        {
            while (final.Count > 0 && final[^1].Text != "\n")
            {
                final.RemoveAt(final.Count - 1);
            }

            final.RemoveAt(final.Count - 1);
        }
    }

    /// <summary>
    /// 针对 <see cref="TextSnippet"/> 特殊文本的换行
    /// </summary>
    public static void WordwrapString(List<TextSnippet> final, string text, Color textColor,
        DynamicSpriteFont font, float maxWidth, int maxWordLength = 19, int maxLines = -1)
    {
        var original = ConvertNormalSnippets(ParseMessage(text, textColor));
        WordWrapString(original, final, textColor, font, maxWidth, maxWordLength, maxLines);
    }

    // https://unicode-table.com/cn/blocks/cjk-unified-ideographs/ 中日韩统一表意文字
    // https://unicode-table.com/cn/blocks/cjk-symbols-and-punctuation/ 中日韩符号和标点
    public static bool IsCjk(char a) =>
        (a >= 0x4E00 && a <= 0x9FFF) || (a >= 0x3000 && a <= 0x303F);

    public static bool CanBreakBetween(char previousChar, char nextChar) =>
        IsCjk(previousChar) || IsCjk(nextChar);

    /* 有Bug，不支持英文空格换行
    public static List<List<TextSnippet>> WordwrapString(string input, Color color, DynamicSpriteFont font, float maxWidth)
    {
        List<TextSnippet> originalSnippets = ConvertNormalSnippets(ParseMessage(input, color));

        List<List<TextSnippet>> firstSnippets = [];
        List<TextSnippet> cacheSnippets = [];

        #region 处理原文中的换行
        foreach (TextSnippet textSnippet in originalSnippets)
        {
            // 以换行分隔开
            string[] strings = textSnippet.Text.Split('\n');

            // length - 1:
            // 最后一行开头前分行
            for (int i = 0; i < strings.Length - 1; i++)
            {
                cacheSnippets.Add(textSnippet.CopyMorph(strings[i]));
                firstSnippets.Add(cacheSnippets);
                cacheSnippets = [];
            }

            // 最后一行开头
            cacheSnippets.Add(textSnippet.CopyMorph(strings[^1]));
        }

        firstSnippets.Add(cacheSnippets);
        cacheSnippets = [];
        #endregion

        #region 根据限制宽度添加换行
        // 最终列表
        List<List<TextSnippet>> finalSnippets = [];

        if (maxWidth > 0)
        {
            foreach (List<TextSnippet> snippets in firstSnippets)
            {
                float cacheWidth = 0f;

                foreach (TextSnippet snippet in snippets)
                {
                    // 简单片段
                    if (snippet is PlainSnippet)
                    {
                        float width = snippet.GetStringLength(font);

                        // 越界, 计算在哪个字符位置断开换行
                        if (cacheWidth + width > maxWidth)
                        {
                            // 缓存字符串
                            string cacheString = "";

                            // 遍历当前字符串
                            foreach (char cacheChar in snippet.Text)
                            {
                                // 此字符宽度
                                float kernedWidth = font.GetCharacterMetrics(cacheChar).KernedWidth;

                                // 缓存宽度 + 间距 + 此字符宽度超行从此处断开
                                // cacheString 加入 cacheSnippets, cacheSnippets 加入 finalSnippets, cacheChar 加入 cacheString
                                // cacheWidth 等于 kernedWidth
                                if (cacheWidth + font.CharacterSpacing + kernedWidth > maxWidth)
                                {
                                    if (!string.IsNullOrEmpty(cacheString))
                                    {
                                        cacheSnippets.Add(snippet.CopyMorph(cacheString));
                                    }

                                    finalSnippets.Add(cacheSnippets);
                                    cacheSnippets = [];

                                    cacheString = cacheChar.ToString();
                                    cacheWidth = kernedWidth;
                                }
                                // 不越界, cacheWidth 加上 字间距和字符宽度
                                else
                                {
                                    cacheString += cacheChar;
                                    cacheWidth += font.CharacterSpacing + kernedWidth;
                                }
                            }

                            // 遍历完, 如果有, 剩下的 cacheString 加入到 cacheSnippets
                            if (!string.IsNullOrEmpty(cacheString))
                            {
                                cacheSnippets.Add(snippet.CopyMorph(cacheString));
                            }
                        }
                        // 未越界, 添加到缓存行
                        else
                        {
                            cacheSnippets.Add(snippet);
                            cacheWidth += width;
                        }
                    }
                    // 非简单片段
                    // 如: [centeritem/stack:type]
                    else
                    {
                        float width = snippet.GetStringLength(font);

                        // 特殊 Snippet 超过宽度限制
                        if (cacheWidth + width > maxWidth)
                        {
                            // 之前的行添加到最终列表
                            finalSnippets.Add(cacheSnippets);
                            cacheSnippets = [snippet];
                            cacheWidth = width;
                        }
                        // 未越界, 添加到缓存行
                        else
                        {
                            cacheSnippets.Add(snippet);
                            cacheWidth += width;
                        }
                    }
                }

                // 遍历完, 如果有, 剩下的 cacheSnippets 加入到 finalSnippets
                if (cacheSnippets.Count > 0)
                {
                    finalSnippets.Add(cacheSnippets);
                    cacheSnippets = [];
                }
            }
        }
        #endregion

        return finalSnippets;
    }
    */
}