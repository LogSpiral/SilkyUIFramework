namespace SilkyUIFramework.Extensions;

public static class ItemExtensions
{
    public static bool NotAir(this Item item) => !item.IsAir;
    public static bool Full(this Item item) => item.stack >= item.maxStack;
    public static bool NotFull(this Item item) => item.stack < item.maxStack;
}