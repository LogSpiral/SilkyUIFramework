using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace SilkyUIFramework.Configs;

public class ClientConfig : ModConfig
{
    public static ClientConfig Instance => ModContent.GetInstance<ClientConfig>();

    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(5.5f)]
    [Range(0, 10f)]
    [Slider]
    [Increment(0.5f)]
    [CustomModConfigItem(typeof(MouseTextOffsetPreview))]
    public float MouseTextOffset;

    [DefaultValue(15.5f)]
    [Range(0, 25f)]
    [Slider]
    [Increment(0.5f)]
    [CustomModConfigItem(typeof(DeathTextOffsetPreview))]
    public float DeathTextOffset;

    public override void OnChanged()
    {
        UITextView.DeathTextOffset = DeathTextOffset;
        UITextView.MouseTextOffset = MouseTextOffset;
    }
}