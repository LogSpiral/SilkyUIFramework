using SilkyUIFramework.Core;

namespace SilkyUIFramework.MyElement;

public class ViewGroupSystem : ModSystem
{
    private readonly ViewGroup Root = new();

    public override void Load()
    {
        Root.SetWidth(0f, 1f);
        Root.SetHeight(0f, 1f);

        for (var i = 0; i < 100; i++)
        {
            var col = new ViewGroup
            {
                AutomaticHeight = true,
            };
            col.SetWidth(100f);
            col.SetGap(2, 2);
            col.LayoutDirection = LayoutDirection.Column;
            Root.AppendChild(col);

            for (var e = 0; e < 20; e++)
            {
                var block = new ViewGroup();
                block.SetWidth(20f);
                block.SetHeight(20f);
                col.AppendChild(block);
            }
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        Root.SetGap(2f, 2f);
        Root.BoxSizing = BoxSizing.BorderBox;
        Root.Padding = new Margin(4f);
        Root.LayoutDirection = LayoutDirection.Row;

        Root.SetLeft(0f, 0.25f);
        Root.SetTop(0f, 0.25f);

        Root.Positioning = Positioning.Relative;
        Root.SetLeft(Main.rand.NextFloat(0.001f), 0.25f);
        Root.SetWidth(Main.rand.NextFloat(0.001f), 0.5f);
        Root.SetMaxWidth(1000f);
        Root.SetHeight(0f, 0.5f);
        Root.AutomaticWidth = true;
        Root.AutomaticHeight = true;

        // 更新 UI 的各种状态，比如动画
        Root.HandleUpdateStatus(gameTime);
        // 布局脏标记检测
        Root.UpdateLayout();
        // 位置脏标记检测
        Root.UpdatePosition();
        // UI 中的普通更新
        Root.HandleUpdate(gameTime);
    }

    public static readonly List<ColorBlockVertextType> Vertices = [];

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var OtherLayer = new OtherLayer(Root, "other ui", InterfaceScaleType.UI);
        layers.Insert(0, OtherLayer);
    }
}

public class OtherLayer(ViewGroup rootElement, string name, InterfaceScaleType scaleType)
    : GameInterfaceLayer(name, scaleType)
{
    public readonly ViewGroup RootElement = rootElement;

    public int _lastViewportWidth = 0;
    public int _lastViewportHeight = 0;

    public Matrix Matrix { get; private set; }

    public override bool DrawSelf()
    {
        ViewGroupSystem.Vertices.Clear();

        RootElement.HandleDraw(Main.gameTimeCache, Main.spriteBatch);

        var device = Main.graphics.GraphicsDevice;

        if (device.Viewport.Width != _lastViewportWidth || device.Viewport.Height != _lastViewportHeight)
        {
            Matrix = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width / 2, device.Viewport.Height / 2, 0, 0,
                1);

            _lastViewportWidth = device.Viewport.Width;
            _lastViewportHeight = device.Viewport.Height;
        }

        var effect = ModAsset.ColorBlock.Value;
        effect.Parameters["MatrixTransform"].SetValue(Matrix);
        effect.CurrentTechnique.Passes[0].Apply();

        _vertices = [.. ViewGroupSystem.Vertices];
        device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length / 3);

        return true;
    }

    private ColorBlockVertextType[] _vertices = null;
}