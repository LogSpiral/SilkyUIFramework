using Microsoft.Xna.Framework.Input;
using Terraria.UI.Chat;

namespace SilkyUIFramework.Elements;

[XmlElementMapping("EditText")]
public class SUIEditText : UITextView
{
    private float _cursorFlashTimer;

    public bool CanDrawCursor { get; set; }

    public Color CursorColor = Color.White;
    public Color CursorFlashColor { get; set; }

    /// <summary> 别问为什么是 2 </summary>
    protected readonly List<TextSnippet> IntermediateSnippets2 = [];

    public string Placeholder
    {
        get; set
        {
            if (value == null) return;
            if (value.Equals(field)) return;
            field = value;
            if (Text.Equals(string.Empty))
                MarkLayoutDirty();
        }
    } = string.Empty;

    public SUIEditText()
    {
        OccupyPlayerInput = true;
        CursorSnippet = new CursorSnippet(this);
    }

    public readonly CursorSnippet CursorSnippet;

    protected override void RecalculateText(float maxWidth)
    {
        var text = Text.Length == 0 ? Placeholder : Text;

        var beforeText = text[..CursorIndex];
        var afterText = text[CursorIndex..];

        IntermediateSnippets.Parse(beforeText, Color.White).ConvertPlainSnippet();
        IntermediateSnippets2.Parse(afterText, Color.White).ConvertPlainSnippet();

        IntermediateSnippets.Add(CursorSnippet);
        IntermediateSnippets.AddRange(IntermediateSnippets2);

        SnippetModule.UpdateProperties(Font, maxWidth, MaxLines);
        // 自动换行 & 指定宽度
        if (WordWrap)
        {
            // 进行换行
            SnippetModule.WordWrapSnippets(IntermediateSnippets);
        }
        else
        {
            SnippetModule.FromSnippets(IntermediateSnippets);
        }

        // 计算文本大小
        TextSize = SnippetModule.GetStringSize(Font, new Vector2(1f));
    }

    protected override void DrawText(SpriteBatch spriteBatch)
    {
        // 光标颜色
        if (IsFocus)
        {
            const int cycle = 45;
            CursorFlashColor = _cursorFlashTimer switch
            {
                < cycle => CursorColor * (_cursorFlashTimer / cycle),
                >= cycle => CursorColor * (2f - _cursorFlashTimer / cycle),
                { } => CursorColor
            };

            _cursorFlashTimer++;
            _cursorFlashTimer %= 90;
        }
        else CursorFlashColor = Color.Transparent;

        base.DrawText(spriteBatch);
    }

    /// <summary>
    /// 绘制文本阴影
    /// </summary>
    protected override void DrawTextShadow(SpriteBatch spriteBatch, Vector2 textPos)
    {
        CanDrawCursor = false;
        base.DrawTextShadow(spriteBatch, textPos);
    }

    /// <summary>
    /// 绘制文本
    /// </summary>
    protected override void DrawTextSelf(SpriteBatch spriteBatch, Vector2 textPos)
    {
        CanDrawCursor = true;
        base.DrawTextSelf(spriteBatch, textPos);
    }

    private int _cursorIndex;

    public int CursorIndex
    {
        get => MathHelper.Clamp(_cursorIndex, 0, Text.Length);
        set
        {
            if (_cursorIndex == value) return;
            _cursorIndex = value;
            MarkLayoutDirty();
        }
    }

    public event Action OnEnterKeyDown;

    public override void HandlePlayerInput(bool inputMethodStatus)
    {
        if (Main.dedServ) return; // 服务器
        if (!Main.hasFocus) return; // 焦点不在游戏

        // 不能再获取了
        Main.oldInputText = Main.inputText;
        Main.inputText = Keyboard.GetState();

        var inputString = string.Empty;
        // 裁剪，复制，粘贴
        if (Main.inputText.IsControlKeyDown())
        {
            if (Keys.X.JustPressed())
            {
                EditTextHelper.SetClipboard(Text);
                Text = "";
            }
            else if (Keys.C.JustPressed())
            {
                EditTextHelper.SetClipboard(Text);
            }
            else if (Keys.V.JustPressed())
            {
                inputString = Main.PasteTextIn(true, inputString);
            }
        }
        else
        {
            inputString = EditTextHelper.GetPlayerInput();
        }

        if (EditTextHelper.CanLineBreak())
            inputString += "\n";
        else if (Main.inputText.IsKeyDown(Keys.Enter))
            inputString = string.Empty;

        if (inputMethodStatus)
        {
            _longPressBackSpaceTimer = 0;
        }
        else
        {
            if (Main.inputText.IsKeyDown(Keys.Back) && !Main.oldInputText.IsKeyDown(Keys.Back))
                DownBackspace();
            LongPressBackSpace();
        }

        InsertText(inputString);

        UpdateCursorMovement(); // 移动光标靠后点，总不会同一帧就想移动并把文本输入到移动后的地方吧？

        if (!Keys.Enter.JustPressed() || EditTextHelper.CanLineBreak()) return;

        OnEnterKeyDown?.Invoke();
    }

    private int _moveCursorTimer;
    private int _longPressBackSpaceTimer;

    /// <summary>
    /// 光标位置
    /// </summary>
    private void UpdateCursorMovement()
    {
        if (Main.inputText.IsKeyDown(Keys.Left) || Main.inputText.IsKeyDown(Keys.Right))
        {
            switch (_moveCursorTimer)
            {
                case 0:
                    MoveCursor();
                    break;
                case >= 30 and < 60:
                    if (_moveCursorTimer % 10 == 0) MoveCursor();
                    break;
                case >= 60 and < 120:
                    if (_moveCursorTimer % 5 == 0) MoveCursor();
                    break;
                case >= 120 and < 180:
                    if (_moveCursorTimer % 2 == 0) MoveCursor();
                    break;
                case >= 180:
                    MoveCursor();
                    break;
            }

            _moveCursorTimer = Math.Min(180, _moveCursorTimer + 1);
        }
        else _moveCursorTimer = 0;
    }

    /// <summary>
    /// 长按删除
    /// </summary>
    public void LongPressBackSpace()
    {
        if (Main.inputText.IsKeyDown(Keys.Back) && Main.oldInputText.IsKeyDown(Keys.Back))
        {
            switch (_longPressBackSpaceTimer)
            {
                case >= 30 and < 60:
                    if (_longPressBackSpaceTimer % 10 == 0) DownBackspace();
                    break;
                case >= 60 and < 120:
                    if (_longPressBackSpaceTimer % 5 == 0) DownBackspace();
                    break;
                case >= 120 and < 180:
                    if (_longPressBackSpaceTimer % 2 == 0) DownBackspace();
                    break;
                case >= 180:
                    DownBackspace();
                    break;
            }

            _longPressBackSpaceTimer++;
        }
        else _longPressBackSpaceTimer = 0;
    }

    /// 移动光标
    private void MoveCursor()
    {
        if (Main.inputText.IsKeyDown(Keys.Left)) CursorIndex--;
        else if (Main.inputText.IsKeyDown(Keys.Right)) CursorIndex++;

        _cursorFlashTimer = 60;
    }

    /// 删除光标前字符并使光标 -1
    private void DownBackspace()
    {
        if (CursorIndex == 0) return;

        if (Text.Length > 0)
            Text = Text.Remove(--CursorIndex, 1);
    }

    /// 插入字符, 位置由光标决定 
    public void InsertText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        Text = Text.Insert(CursorIndex, text);
        CursorIndex += text.Length;
    }

    /// <summary>
    /// 当开始输入操作时触发
    /// </summary>
    public event MouseEventHandler StartTakingInput;

    /// <summary>
    /// 当结束输入操作时触发
    /// </summary>
    public event EventHandler<ValueChangedEventArgs<string>> EndTakingInput;

    private string LastInputText { get; set; }

    #region 螺线的鼠标定位光标，先搁置。

    // 虽然和GotFocus事件定位重复了，但是我感觉还是专门整个事件来管理开始和结束输入比较好

    public override void OnGotFocus(UIMouseEvent evt)
    {
        base.OnGotFocus(evt);

        RePositioningCursorIndex(evt.MousePosition);
        LastInputText = Text;
        StartTakingInput?.Invoke(this, evt);
    }
    public override void OnLostFocus(UIMouseEvent evt)
    {
        base.OnLostFocus(evt);
        EndTakingInput?.Invoke(this, new(LastInputText, Text));
    }

    void RePositioningCursorIndex(Vector2 mousePosition)
    {
        if (mousePosition.X > Bounds.X + Bounds.Width)
            CursorIndex = Text.Length;
        else if (mousePosition.X < Bounds.X)
            CursorIndex = 0;
        else
        {
            // TODO 螺线瞎写的定位，应该要改

            //int resultIndex = 0;

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

            /*var curPos = textPos;
            var list = FinalSnippets;
            foreach (var line in FinalSnippets)
            {
                var text = line.Text;
                Vector2 size = Font.MeasureString(text) * TextScale;
                var area = new Bounds(textPos.X, textPos.Y, size.X, size.Y);
                if (!area.Contains(mousePosition))
                {
                    if (text.Contains('\n'))
                    {
                        curPos.X = textPos.X;
                        curPos.Y += size.Y;
                    }
                    else
                        curPos.X += textPos.X;
                    resultIndex += text.Length;
                    continue;
                }
                float x = textPos.X;
                int n = 0;
                for (; x < mousePosition.X; n++)
                {
                    x += Font.MeasureString(text[n].ToString()).X * TextScale;
                }
                resultIndex += n;
            }*/

            int textLength = Text.Length;
            int n = 0;
            for (; textPos.X < mousePosition.X && n < textLength; n++)
                textPos.X += Font.MeasureString(Text[n].ToString()).X * TextScale;

            CursorIndex = n;
        }
    }

    #endregion
}