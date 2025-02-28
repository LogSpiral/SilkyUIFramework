// namespace SilkyUIFramework;

// public class OtherSystem : ModSystem
// {
//     public readonly Other RootElement = new();

//     public readonly Other child1 = new();
//     public readonly Other child2 = new();

//     public readonly Other child3 = new();
//     public readonly Other child4 = new();

//     public override void Load()
//     {
//         RootElement.SetSize(0, 0, 1f, 1f);

//         for (int i = 0; i < 10; i++)
//         {
//             var col = new Other();
//             col.SetWidth(100f);
//             col.AutoHeight();
//             col.Gap = new Vector2(2);
//             col.LayoutDirection = LayoutDirection.Column;
//             RootElement.AppendChild(col);

//             for (int e = 0; e < 10; e++)
//             {
//                 var block = new Other();
//                 block.SetSize(10f, 10f);
//                 col.BackgroundColor = Color.Red * 0.1f;
//                 col.AppendChild(block);
//             }
//         }

//         // RootElement.AppendChildren(child1, child2);
//     }

//     public override void UpdateUI(GameTime gameTime)
//     {
//         RootElement.Gap = new Vector2(2f);
//         RootElement.Padding = new Margin(2f);
//         RootElement.LayoutDirection = LayoutDirection.Row;

//         RootElement.SetLeft(0f, 0.25f);
//         RootElement.SetTop(0f, 0.25f);

//         RootElement.SetWidth(Main.rand.NextFloat(0.001f), 0.5f);
//         RootElement.SetHeight(0f, 0.5f);

//         RootElement.AutoSize(false, false);

//         // 更新 UI 的各种状态，比如动画
//         RootElement.HandleUpdateStatus(gameTime);
//         // 布局脏标记检测
//         RootElement.LayoutDirtyCheck();
//         // 位置脏标记检测
//         RootElement.PositionDirtyCheck();
//         // UI 中的各种更新
//         RootElement.HandleUpdate(gameTime);
//     }

//     public static readonly List<ColorBlockVertextType> Vertices = [];

//     public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
//     {
//         var OtherLayer = new OtherLayer(RootElement, "other ui", InterfaceScaleType.UI);
//         layers.Insert(0, OtherLayer);
//     }
// }

// public class OtherLayer(Other rootElement, string name, InterfaceScaleType scaleType) : GameInterfaceLayer(name, scaleType)
// {
//     public readonly Other RootElement = rootElement;

//     public int _lastViewportWidth = 0;
//     public int _lastViewportHeight = 0;

//     public Matrix Matrix { get; private set; }

//     public override bool DrawSelf()
//     {
//         OtherSystem.Vertices.Clear();

//         RootElement.HandleDraw(Main.gameTimeCache, Main.spriteBatch);

//         var device = Main.graphics.GraphicsDevice;

//         if (device.Viewport.Width != _lastViewportWidth || device.Viewport.Height != _lastViewportHeight)
//         {
//             Matrix = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width / 2, device.Viewport.Height / 2, 0, 0, 1);

//             _lastViewportWidth = device.Viewport.Width;
//             _lastViewportHeight = device.Viewport.Height;
//         }

//         var effect = ModAsset.ColorBlock.Value;
//         effect.Parameters["MatrixTransform"].SetValue(Matrix);
//         effect.CurrentTechnique.Passes[0].Apply();

//         _vertices = [.. OtherSystem.Vertices];
//         device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length / 3);

//         return true;
//     }

//     private ColorBlockVertextType[] _vertices = null;
// }