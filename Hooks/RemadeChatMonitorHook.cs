// using System.Diagnostics;
// using System.Text.RegularExpressions;
// using SilkyUIFramework.UserInterfaces.ChatReset;
// using Terraria.GameContent.UI.Chat;
//
// namespace SilkyUIFramework.Hooks;
//
// public partial class RemadeChatMonitorHook : ModSystem
// {
//     public override void Load()
//     {
//         On_RemadeChatMonitor.AddNewMessage += AddNewMessageHook;
//     }
//
//     private void AddNewMessageHook(On_RemadeChatMonitor.orig_AddNewMessage orig, RemadeChatMonitor self, string text,
//         Color color, int widthlimitinpixels)
//     {
//         // orig?.Invoke(self, text, color, widthlimitinpixels);
//
//         if (ChatWindowUI.Instance is not { } chatUI || string.IsNullOrEmpty(text)) return;
//         var sender = MatchUsername().Match(text);
//         if (sender.Success)
//         {
//             text = text[(sender.Value.Length + 1)..];
//             var playerName = sender.Groups[1].Value;
//             if (Main.LocalPlayer is { } localPlayer && localPlayer.name.Equals(playerName))
//             {
//                 chatUI.AppendMessage(sender.Groups[1].Value, text, color, true);
//             }
//             else
//                 chatUI.AppendMessage(sender.Groups[1].Value, text, color, false);
//         }
//         else
//         {
//             chatUI.AppendMessage("系统消息", text, color, false);
//         }
//     }
//
//     // public static bool HasChatSource() => ModLoader.HasMod("ChatSource");
//
//     [GeneratedRegex(@"^\[n:(.*?)\]")]
//     private static partial Regex MatchUsername();
// }