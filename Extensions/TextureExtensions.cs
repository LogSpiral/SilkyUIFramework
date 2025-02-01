using ReLogic.Content;

namespace SilkyUIFramework.Extensions;

public static class TextureExtensions
{
    public static Asset<Texture2D> LoadItem(this Asset<Texture2D> texture) =>
        Main.Assets.Request<Texture2D>(texture.Name, AssetRequestMode.ImmediateLoad);
}