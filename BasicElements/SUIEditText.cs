using System.Net.Mime;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using SilkyUIFramework.Core;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;

namespace SilkyUIFramework.BasicElements;

public class SUIEditText : SUIText
{
    private float _cursorFlashTimer;

    public bool CanDrawCursor { get; set; }
    public Color CursorFlashColor;

    public SUIEditText()
    {
        CursorSnippet = new CursorSnippet(this);
        DragIgnore = false;
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        IsEditing = true;
        base.LeftMouseDown(evt);
    }

    public bool IsFocused =>
        SilkyUIManager.Instance.MouseFocusTarget == this;

    public readonly CursorSnippet CursorSnippet;

    protected override void RecalculateText()
    {
        var text = Text;

        // 是否换行
        if (WordWrap && SpecifyWidth)
        {
            var beforeText = text[..CursorIndex];
            var afterText = text.Substring(CursorIndex, text.Length - CursorIndex);
            var maxWidth = _innerDimensions.Width / TextScale;
            var beforeSnippet =
                TextSnippetHelper.ConvertNormalSnippets(TextSnippetHelper.ParseMessage(beforeText, TextColor));
            var afterSnippet =
                TextSnippetHelper.ConvertNormalSnippets(TextSnippetHelper.ParseMessage(afterText, TextColor));
            beforeSnippet.Add(CursorSnippet);
            beforeSnippet.AddRange(afterSnippet);
            TextSnippetHelper.WordwrapString(beforeSnippet, FinalSnippets, TextColor, Font, maxWidth, MaxWordLength);
        }
        else
        {
            FinalSnippets.Clear();
            FinalSnippets.AddRange(
                TextSnippetHelper.ConvertNormalSnippets(TextSnippetHelper.ParseMessage(text, TextColor)));
        }

        TextSize = ChatManager.GetStringSize(Font, FinalSnippets.ToArray(), new Vector2(1f));
        TextChanges = false;
    }

    protected override void DrawText(SpriteBatch spriteBatch, List<TextSnippet> textSnippets)
    {
        // 光标颜色
        if (IsEditing && SilkyUIManager.Instance.MouseFocusTarget == this)
        {
            const int cycle = 60;
            CursorFlashColor = _cursorFlashTimer switch
            {
                < cycle => Color.White * (_cursorFlashTimer / cycle),
                >= cycle => Color.White * (1 - (_cursorFlashTimer - cycle) / cycle),
                _ => CursorFlashColor
            };

            _cursorFlashTimer++;
            _cursorFlashTimer %= cycle * 2;
        }
        else CursorFlashColor = Color.Transparent;

        base.DrawText(spriteBatch, textSnippets);
    }

    protected override void DrawTextShadow(SpriteBatch spriteBatch, List<TextSnippet> finalSnippets, Vector2 textPos)
    {
        CanDrawCursor = false;
        base.DrawTextShadow(spriteBatch, finalSnippets, textPos);
    }

    protected override void DrawTextSelf(SpriteBatch spriteBatch, List<TextSnippet> finalSnippets, Vector2 textPos)
    {
        CanDrawCursor = true;
        base.DrawTextSelf(spriteBatch, finalSnippets, textPos);
    }

    public bool IsEditing { get; set; } = true;

    public override bool OccupyPlayerInput => IsEditing;

    private int _cursorIndex;

    public int CursorIndex
    {
        get => MathHelper.Clamp(_cursorIndex, 0, Text.Length);
        set
        {
            if (_cursorIndex == value) return;
            _cursorIndex = value;
            TextChanges = true;
        }
    }

    public event Action OnEnterKeyDown;

    public override void HandlePlayerInput()
    {
        if (Main.dedServ) return; // 服务器
        if (!Main.hasFocus) return; // 焦点不在游戏

        // 不能再获取了
        Main.oldInputText = Main.inputText;
        Main.inputText = Keyboard.GetState();

        // 退出编辑
        if (Main.inputText.IsKeyDown(Keys.Escape))
        {
            IsEditing = false;
            return;
        }

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

        UpdateBackSpace();

        InsertText(inputString);

        UpdateCursorMovement(); // 移动光标靠后点，总不会同一帧就想移动并把文本输入到移动后的地方吧？

        if (!Keys.Enter.JustPressed() || EditTextHelper.CanLineBreak()) return;

        OnEnterKeyDown?.Invoke();
    }

    private int _moveCursorTimer;
    private int _backSpaceTimer;

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
    public void UpdateBackSpace()
    {
        if (Main.inputText.IsKeyDown(Keys.Back) && Main.oldInputText.IsKeyDown(Keys.Back))
        {
            switch (_backSpaceTimer)
            {
                case 0:
                    DownBackspace();
                    break;
                case >= 30 and < 60:
                    if (_backSpaceTimer % 10 == 0) DownBackspace();
                    break;
                case >= 60 and < 120:
                    if (_backSpaceTimer % 5 == 0) DownBackspace();
                    break;
                case >= 120 and < 180:
                    if (_backSpaceTimer % 2 == 0) DownBackspace();
                    break;
                case >= 180:
                    DownBackspace();
                    break;
            }

            _backSpaceTimer = Math.Min(180, _backSpaceTimer + 1);
        }
        else _backSpaceTimer = 0;
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