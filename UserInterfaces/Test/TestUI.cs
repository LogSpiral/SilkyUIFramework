using SilkyUIFramework.BasicComponents;
using Terraria;

namespace SilkyUIFramework.UserInterfaces.Test;

[RegisterUI("Vanilla: Radial Hotbars", "TestUI")]
public class TestUI : BasicBody
{
    protected override void OnInitialize()
    {
        // 设置位置
        SetLeft(alignment: 0.5f);
        SetTop(alignment: 0.75f);

        // 子元素排列方向
        FlexDirection = FlexDirection.Row;

        // 适应子元素大小（不用根据子元素手动设置自己的大小了）
        FitWidth = true;
        FitHeight = true;

        // 圆角大小（豪堪）
        BorderRadius = new Vector4(4f);

        // 内边距
        Padding = new Margin(8f);

        // 添加十个 Image 元素
        for (int i = 0; i < 10; i++)
        {
            // 传入参数
            var image = new SUIImage(TextureAssets.Item[1]).Join(this);
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
