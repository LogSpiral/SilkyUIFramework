using Terraria.UI.Chat;

namespace SilkyUIFramework.BasicElements;

public class SUIItemSlot : View
{
    protected Item ItemInside = new();

    public virtual Item Item { get => ItemInside; set => ItemInside = value; }

    /// <summary>
    /// 允许显示物品信息
    /// </summary>
    public bool DisplayItemInfo { get; set; } = true;

    /// <summary>
    /// 允许显示物品堆叠数量
    /// </summary>
    public bool DisplayItemStack { get; set; } = true;

    /// <summary>
    /// 总是显示物品堆叠数量，即使堆叠数量为 1
    /// </summary>
    public bool AlwaysDisplayItemStack { get; set; } = false;

    public float ItemIconSizeLimit { get; set; } = 32f;
    public float ItemScale { get; set; } = 1f;
    public Color ItemColor { get; set; } = Color.White;
    public Vector2 ItemOffset { get; set; } = Vector2.Zero;

    public SUIItemSlot()
    {
        Border = 2;
        BorderColor = Color.Black * 0.75f;
        BgColor = Color.Black * 0.5f;

        DragIgnore = false;
    }

    /// <summary>
    /// 设置 BaseItemSlot 基本属性
    /// </summary>
    /// <param name="displayItemInfo"></param>
    /// <param name="displayItemStack"></param>
    public void SetBaseItemSlotValues(bool displayItemInfo, bool displayItemStack)
    {
        DisplayItemInfo = displayItemInfo;
        DisplayItemStack = displayItemStack;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (DisplayItemInfo && IsMouseHovering)
        {
            Main.hoverItemName = Item.Name;
            Main.HoverItem = Item.Clone();
        }

        var dimensions = _dimensions;
        dimensions.X += ItemOffset.X;
        dimensions.Y += ItemOffset.Y;

        DrawItemIcon(spriteBatch, Item, Color.White, dimensions.Center(), 32f, ItemScale);

        if (AlwaysDisplayItemStack || (DisplayItemStack && Item.stack > 1))
        {
            DrawItemStack(spriteBatch);
        }
    }

    public void DrawItemStack(SpriteBatch spriteBatch)
    {
        var font = FontAssets.ItemStack.Value;
        Vector2 textSize = font.MeasureString(Item.stack.ToString()) * 0.75f * ItemScale;
        Vector2 position = _dimensions.Position() + new Vector2(_dimensions.Width * 0.18f, (_dimensions.Height - textSize.Y) * 0.9f);
        spriteBatch.DrawString(font, $"{Item.stack}", position, Color.White, 0f, Vector2.Zero, 0.75f, 0f, 1f);
    }

    public static void DrawItemIcon(SpriteBatch spriteBatch, Item item, Color color, Vector2 center,
        float sizeLimit = 32f, float itemScale = 1f)
    {
        Main.instance.LoadItem(item.type);
        Texture2D texture2D = TextureAssets.Item[item.type].Value;
        Rectangle frame = Main.itemAnimations[item.type]?.GetFrame(texture2D) ?? texture2D.Frame();

        itemScale *= (frame.Width > sizeLimit || frame.Height > sizeLimit)
            ? frame.Width > frame.Height ? sizeLimit / frame.Width : sizeLimit / frame.Height
            : 1f;
        Vector2 origin = frame.Size() / 2f;

        if (ItemLoader.PreDrawInInventory(item, spriteBatch, center, frame, item.GetAlpha(color),
                item.GetColor(color), origin, itemScale))
        {
            spriteBatch.Draw(texture2D, center, frame, item.GetAlpha(color), 0f, origin, itemScale,
                SpriteEffects.None, 0f);
            if (item.color != Color.Transparent)
                spriteBatch.Draw(texture2D, center, frame, item.GetColor(color), 0f, origin, itemScale,
                    SpriteEffects.None, 0f);
        }

        ItemLoader.PostDrawInInventory(item, spriteBatch, center, frame, item.GetAlpha(color),
            item.GetColor(color), origin, itemScale);

        if (ItemID.Sets.TrapSigned[item.type])
            Main.spriteBatch.Draw(TextureAssets.Wire.Value, center + new Vector2(14f) * itemScale,
                new Rectangle(4, 58, 8, 8), color, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);

        if (ItemID.Sets.DrawUnsafeIndicator[item.type])
        {
            Vector2 vector2 = new Vector2(-4f, -4f) * itemScale;
            Texture2D value7 = TextureAssets.Extra[258].Value;
            Rectangle rectangle2 = value7.Frame();
            Main.spriteBatch.Draw(value7, center + vector2 + new Vector2(14f) * itemScale, rectangle2, color, 0f,
                rectangle2.Size() / 2f, 1f, SpriteEffects.None, 0f);
        }

        if (item.type is ItemID.RubblemakerSmall or ItemID.RubblemakerMedium or ItemID.RubblemakerLarge)
        {
            Vector2 vector3 = new Vector2(2f, -6f) * itemScale;
            switch (item.type)
            {
                case 5324:
                {
                    Texture2D value10 = TextureAssets.Extra[257].Value;
                    Rectangle rectangle5 = value10.Frame(3, 1, 2);
                    Main.spriteBatch.Draw(value10, center + vector3 + new Vector2(16f) * itemScale, rectangle5,
                        color, 0f, rectangle5.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    break;
                }
                case 5329:
                {
                    Texture2D value9 = TextureAssets.Extra[257].Value;
                    Rectangle rectangle4 = value9.Frame(3, 1, 1);
                    Main.spriteBatch.Draw(value9, center + vector3 + new Vector2(16f) * itemScale, rectangle4,
                        color, 0f, rectangle4.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    break;
                }
                case 5330:
                {
                    Texture2D value8 = TextureAssets.Extra[257].Value;
                    Rectangle rectangle3 = value8.Frame(3);
                    Main.spriteBatch.Draw(value8, center + vector3 + new Vector2(16f) * itemScale, rectangle3,
                        color, 0f, rectangle3.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    break;
                }
            }
        }
    }
}