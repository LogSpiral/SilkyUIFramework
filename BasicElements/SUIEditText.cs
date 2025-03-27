using Microsoft.Xna.Framework.Input;
using Terraria.UI.Chat;

namespace SilkyUIFramework.BasicElements;

public class SUIEditText : UITextView
{
    private float _cursorFlashTimer;

    public bool CanDrawCursor { get; set; }

    public Color CursorColor = Color.White;
    public Color CursorFlashColor { get; set; }

    public SUIEditText()
    {
        OccupyPlayerInput = true;
        CursorSnippet = new CursorSnippet(this);
    }

    public readonly CursorSnippet CursorSnippet;

    protected override void RecalculateText(float maxWidth)
    {
        var text = Text;

        var beforeText = text[..CursorIndex];
        var afterText = text[CursorIndex..];
        var beforeSnippet =
            TextSnippetHelper.ConvertNormalSnippets(TextSnippetHelper.ParseMessage(beforeText, TextColor));
        var afterSnippet =
            TextSnippetHelper.ConvertNormalSnippets(TextSnippetHelper.ParseMessage(afterText, TextColor));
        beforeSnippet.Add(CursorSnippet);
        beforeSnippet.AddRange(afterSnippet);

        // 自动换行 & 指定宽度
        if (WordWrap)
        {
            // 进行换行
            TextSnippetHelper.WordWrapString(beforeSnippet, FinalSnippets,
                TextColor, Font, maxWidth, MaxWordLength, MaxLines);
        }
        else
        {
            FinalSnippets.Clear();
            FinalSnippets.AddRange(beforeSnippet);
        }

        // 计算文本大小
        TextSize = TextSnippetHelper.GetStringSize(Font, FinalSnippets, new Vector2(1f));
    }

    protected override void DrawText(SpriteBatch spriteBatch, List<TextSnippet> textSnippets)
    {
        // 光标颜色
        if (IsFocus)
        {
            const int cycle = 60;
            CursorFlashColor = _cursorFlashTimer switch
            {
                < cycle => CursorColor * (_cursorFlashTimer / cycle),
                >= cycle => CursorColor * (1 - (_cursorFlashTimer - cycle) / cycle),
                _ => CursorColor
            };

            _cursorFlashTimer++;
            _cursorFlashTimer %= cycle * 2;
        }
        else CursorFlashColor = Color.Transparent;

        base.DrawText(spriteBatch, textSnippets);
    }

    /// <summary>
    /// 绘制文本阴影
    /// </summary>
    protected override void DrawTextShadow(SpriteBatch spriteBatch, List<TextSnippet> finalSnippets, Vector2 textPos)
    {
        CanDrawCursor = false;
        base.DrawTextShadow(spriteBatch, finalSnippets, textPos);
    }

    /// <summary>
    /// 绘制文本
    /// </summary>
    protected override void DrawTextSelf(SpriteBatch spriteBatch, List<TextSnippet> finalSnippets, Vector2 textPos)
    {
        CanDrawCursor = true;
        base.DrawTextSelf(spriteBatch, finalSnippets, textPos);
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
}