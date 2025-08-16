using SilkyUIFramework.BasicComponents;

namespace SilkyUIFramework.UserInterfaces.Test;

[RegisterUI("Vanilla: Radial Hotbars", "TestUI")]
public partial class TestUI : BasicBody
{
    protected override void OnInitialize()
    {
        InitializeComponent();

        // 添加十个 Image 元素
        for (int i = 0; i < 10; i++)
        {
            // 传入参数
            var image = new SUIImage(TextureAssets.Item[1]).Join(Container);
            image.SetSize(50, 50);
            image.ImageAlign = new Vector2(0.5f, 0.5f);

            // 默认会 Fit 大小，这里给个固定的
            image.FitWidth = false;
            image.FitHeight = false;

            // 圆角（豪堪）
            image.BorderRadius = new Vector4(4f);

            // 背景颜色
            image.BackgroundColor = Color.Red * 0.5f;
        }
    }

    protected override void UpdateStatus(GameTime gameTime)
    {
        base.UpdateStatus(gameTime);
    }
}
