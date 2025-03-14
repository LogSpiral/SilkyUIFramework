using SilkyUIFramework.Core;

namespace SilkyUIFramework.MyElement;

public class ViewGroupSystem : ModSystem
{
    private readonly ViewGroup Root = new();

    public override void Load()
    {
        Root.SetWidth(0f, 1f);
        Root.SetHeight(0f, 1f);

        for (var i = 0; i < 4; i++)
        {
            var col = new ViewGroup
            {
                AutomaticHeight = true,
            };
            col.SetWidth(100f);
            col.SetGap(4, 4);
            col.Padding = new(4f);
            col.LayoutDirection = LayoutDirection.Column;
            Root.AppendChild(col);

            for (var e = 0; e < 4; e++)
            {
                var block = new UIView();
                block.SetWidth(100f);
                block.SetHeight(50f);
                col.AppendChild(block);
            }
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        Root.BoxSizing = BoxSizing.Content;
        Root.Padding = new Margin(4f);
        Root.LayoutDirection = LayoutDirection.Row;
        Root.SetGap(4f, 4f);

        // 子元素
        var groups = Root.ReadOnlyChildren.OfType<ViewGroup>().ToArray();

        for (var i = 0; i < groups.Length; i++)
        {
            var col = groups[i];
            col.FlexGrow = 0f;
            col.FlexShrink = 1f;
            col.CrossAlignment = CrossAlignment.Stretch;
            if (i == 0)
            {
                col.FlexGrow = 0f;
                col.FlexShrink = 1f;
                col.AutomaticWidth = true;
                col.SetWidth(200f);
            }
            else
            {
                col.AutomaticWidth = true;
                col.AutomaticHeight = true;
            }
            col.SetGap(4, 4);
            col.Padding = new Margin(4f);

            foreach (var item in col.ReadOnlyChildren)
            {
                item.FlexGrow = 1f;
                item.FlexShrink = 1f;
                item.SetWidth(200f);
                item.SetHeight(100f);
            }
        }

        Root.Positioning = Positioning.Relative;
        Root.SetLeft(Main.rand.NextFloat(0.001f), 0f, 0.5f);
        Root.SetTop(0f, 0f, 0.5f);

        Root.SetWidth(Main.rand.NextFloat(0.001f), 0.25f);
        Root.SetHeight(0f, 0.5f);

        Root.AutomaticWidth = false;
        Root.AutomaticHeight = true;

        // 更新 UI 的各种状态，比如动画
        Root.HandleUpdateStatus(gameTime);
        // Bounds and Layout
        Root.UpdateBounds();
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