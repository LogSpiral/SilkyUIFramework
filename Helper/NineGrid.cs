// namespace SilkyUIFramework.Helpers;

// public class NineGrid
// {
//     // size 84*84
//     public Texture2D Texture2D { get; set; } = ModAsset.PreviewBorder.Value;

//     public Rectangle LeftTopBlock = new(0, 0, 28, 28);
//     public Rectangle RightTopBlock = new(56, 0, 28, 28);
//     public Rectangle LeftBottomBlock = new(0, 56, 28, 28);
//     public Rectangle RightBottomBlock = new(56, 56, 28, 28);
//     public Rectangle LeftBlock = new(0, 28, 6, 2);
//     public Rectangle TopBlock = new(28, 0, 2, 6);
//     public Rectangle RightBlock = new(78, 28, 6, 2);
//     public Rectangle BottomBlock = new(28, 78, 2, 6);

//     public void Draw(SpriteBatch spriteBatch, Vector2 position, Vector2 size, Color color)
//     {
//         var texture = Texture2D;

//         var leftScale = (size.Y - LeftTopBlock.Height - LeftBottomBlock.Height) / LeftBlock.Height;

//         var topScale = (size.X - LeftTopBlock.Width - RightTopBlock.Width) / TopBlock.Width;

//         var rightScale = (size.Y - RightTopBlock.Height - RightBottomBlock.Height) / RightBlock.Height;

//         var bottomScale = (size.X - LeftBottomBlock.Width - RightBottomBlock.Width) / BottomBlock.Width;

//         // 左上角
//         spriteBatch.Draw(texture, position, LeftTopBlock, color, 0f, default, 1f, 0, 0);

//         // 左
//         spriteBatch.Draw(texture, new Vector2(position.X, position.Y + LeftTopBlock.Height),
//             LeftBlock, color, 0f, default, new Vector2(1f, leftScale), 0, 0);

//         // 上
//         spriteBatch.Draw(texture, new Vector2(position.X + LeftTopBlock.Width, position.Y),
//             TopBlock, color, 0f, default, new Vector2(topScale, 1f), 0, 0);

//         // 左下角
//         spriteBatch.Draw(texture, new Vector2(position.X, position.Y + size.Y), LeftBottomBlock, color, 0f, new Vector2(0f, LeftBottomBlock.Height), 1f, 0, 0);

//         // 右上角
//         spriteBatch.Draw(texture, new Vector2(position.X + size.X, position.Y), RightTopBlock, color, 0f, new Vector2(RightTopBlock.Width, 0f), 1f, 0, 0);

//         // 左
//         spriteBatch.Draw(texture, new Vector2(position.X + size.X - 6, position.Y + 28),
//             RightBlock, color, 0f, default, new Vector2(1f, rightScale), 0, 0);

//         // 上
//         spriteBatch.Draw(texture, new Vector2(position.X + 28, position.Y + size.Y - 6),
//             BottomBlock, color, 0f, default, new Vector2(bottomScale, 1f), 0, 0);

//         // 右下角
//         spriteBatch.Draw(texture, position + size, RightBottomBlock, color, 0f, RightBottomBlock.Size(), 1f, 0, 0f);
//     }
// }