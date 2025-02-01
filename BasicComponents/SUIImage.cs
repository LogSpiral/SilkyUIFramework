using System.Diagnostics;
using SilkyUIFramework.BasicElements;

namespace SilkyUIFramework.BasicComponents;

public class SUIImage : View
{
    #region Texture2D

    public delegate void TextureChangeEventHandler(SUIImage sender, Texture2D newTexture, Texture2D previousTexture);

    private Texture2D _texture;

    public Texture2D Texture
    {
        get => _texture;
        set
        {
            OnTextureChanged(this, value, _texture);
            _texture = value;
        }
    }

    public event TextureChangeEventHandler TextureChanged;

    protected virtual void OnTextureChanged(SUIImage sender, Texture2D newTexture, Texture2D previousTexture) =>
        TextureChanged?.Invoke(sender, newTexture, previousTexture);

    #endregion

    public Vector2 ImagePosition = Vector2.Zero;
    public Vector2 ImagePercent = Vector2.Zero;
    public Vector2 ImageAlign = Vector2.Zero;
    public Vector2 ImageOrigin = Vector2.Zero;

    /// <summary>
    /// 图片缩放
    /// </summary>
    public Vector2 ImageScale { get; set; } = Vector2.One;

    /// <summary>
    /// 图片染色
    /// </summary>
    public Color ImageColor { get; set; } = Color.White;

    public Rectangle? SourceRectangle { get; set; }

    public SUIImage(Texture2D texture, bool setSizeViaTexture = true)
    {
        Texture = texture;
        SpecifyWidth = SpecifyHeight = true;

        if (!setSizeViaTexture || Texture == null) return;
        Width.Pixels = Texture.Width + this.HPadding();
        Height.Pixels = Texture.Height + this.VPadding();
    }

    protected override Vector2 GetOuterSize(float width, float height)
    {
        var size = base.GetOuterSize(width, height);

        if (!SpecifyWidth) size.X = (Texture?.Width ?? 0f) + this.HPadding() + this.HMargin() + Border * 2f;
        if (!SpecifyHeight) size.Y = (Texture?.Height ?? 0f)+ this.VPadding() + this.VMargin() + Border * 2f;

        return size;
    }

    public override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);
        if (Texture is null) return;

        var position = GetDimensions().Position();
        var size = GetDimensions().Size();

        var imagePosition = position + ImagePosition + size * ImagePercent;
        imagePosition += (size - Texture.Size()) * ImageAlign;

        sb.Draw(Texture, imagePosition, SourceRectangle, ImageColor,
            0f, Texture.Size() * ImageOrigin, ImageScale, 0f, 0f);
    }
}