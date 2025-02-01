// using SilkyUIFramework.UserInterfaces.ChatReset;
//
// namespace SilkyUIFramework;
//
// public class SilkyUICommand : ModCommand
// {
//     public override CommandType Type => CommandType.Chat;
//     public override string Command => "clear";
//     public override string Usage => "Usage";
//     public override string Description => "Description";
//
//     public override void Action(CommandCaller caller, string input, string[] args)
//     {
//         if (ChatWindowUI.Instance is { } chatUI)
//         {
//             chatUI.ClearMessage();
//             // caller.Reply("clear message.", Color.Green);
//         }
//     }
// }