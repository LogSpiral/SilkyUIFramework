using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace SilkyUIFramework.Configs;

public class ClientConfig : ModConfig
{
    public static ClientConfig Instance => ModContent.GetInstance<ClientConfig>();

    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(7f)]
    [Range(0, 10f)]
    [Slider]
    [Increment(0.5f)]
    public float DeathTextOffset;

    [DefaultValue(16f)]
    [Range(0, 20f)]
    [Slider]
    [Increment(0.5f)]
    public float MouseTextOffset;

    public override void OnChanged()
    {
        SUIText.DeathTextOffset = DeathTextOffset;
        SUIText.MouseTextOffset = MouseTextOffset;
    }
}