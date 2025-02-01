// using Microsoft.Xna.Framework.Input;
// using SilkyUIFramework.UserInterfaces.ChatReset;
// using Terraria.UI.Chat;
//
// namespace SilkyUIFramework.Hooks;
//
// public class MainHook : ModSystem
// {
//     public override void Load()
//     {
//         On_Main.OpenPlayerChat += On_MainOnOpenPlayerChat;
//
//         On_ItemSlot.OverrideHover_ItemArray_int_int += On_ItemSlotOnOverrideHover_ItemArray_int_int;
//
//         On_ChatManager.AddChatText += On_ChatManagerOnAddChatText;
//
//         On_Main.DoUpdate_HandleChat += On_MainOnDoUpdate_HandleChat;
//
//         On_Main.DrawPlayerChat += On_MainOnDrawPlayerChat;
//     }
//
//     private static void On_MainOnDrawPlayerChat(On_Main.orig_DrawPlayerChat orig, Main self)
//     {
//     }
//
//     private static void On_MainOnDoUpdate_HandleChat(On_Main.orig_DoUpdate_HandleChat orig)
//     {
//         orig?.Invoke();
//         if (ChatWindowUI.Instance is { } chatUI && chatUI.EditText.IsFocused && chatUI.EditText.OccupyPlayerInput)
//         {
//             Main.drawingPlayerChat = true;
//         }
//         else
//         {
//             Main.drawingPlayerChat = false;
//         }
//     }
//
//     private bool On_ChatManagerOnAddChatText(On_ChatManager.orig_AddChatText orig, DynamicSpriteFont font, string text,
//         Vector2 basescale)
//     {
//         if (ChatWindowUI.Instance is { } chatUI && chatUI.EditText.IsFocused && chatUI.EditText.OccupyPlayerInput)
//         {
//             chatUI.EditText.InsertText(text);
//         }
//
//         return true;
//     }
//
//     private static void On_ItemSlotOnOverrideHover_ItemArray_int_int(
//         On_ItemSlot.orig_OverrideHover_ItemArray_int_int orig,
//         Item[] inv, int context, int slot)
//     {
//         orig?.Invoke(inv, context, slot);
//         if (ChatWindowUI.Instance is { } chatUI && chatUI.EditText.IsFocused && chatUI.EditText.OccupyPlayerInput &&
//             Main.inputText.IsAltKeyDown())
//             Main.cursorOverride = 2;
//     }
//
//     private static void On_MainOnOpenPlayerChat(On_Main.orig_OpenPlayerChat orig)
//     {
//         // orig?.Invoke();
//
//         if (ChatWindowUI.Instance is not { SilkyUI: { } silkyUI } chatUI ||
//             (chatUI.EditText.IsFocused && chatUI.EditText.OccupyPlayerInput)) return;
//         silkyUI.SetFocusTarget(chatUI.EditText);
//     }
// }