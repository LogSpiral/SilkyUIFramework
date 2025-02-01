using Microsoft.Xna.Framework.Input;

namespace SilkyUIFramework.Extensions;

public static class KeyboardStateExtensions
{
    public static bool IsControlKeyDown(this KeyboardState keyboardState) =>
        keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);

    public static bool IsAltKeyDown(this KeyboardState keyboardState) =>
        keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);

    public static bool IsShiftKeyDown(this KeyboardState keyboardState) =>
        keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

    public static bool JustPressed(this Keys key) =>
        Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
}