using System;
using UnityEngine;

// Rarity values are named after key colors so the Inspector is self-explanatory.
public enum ItemRarity { Red, Blue, Green, Yellow, Master }

[Serializable]
public class Equipment
{
    public string itemName;
    public ItemRarity rarity;
    public Sprite itemSprite;

    // Key identity — replaces combat stats
    public KeyColor keyColor;
    public bool isMasterKey;

    // roomLevel is kept so ItemGenerator's existing scaling hooks still compile
    public int roomLevel;

    public Equipment(ItemRarity rarity, int roomLevel, float difficulty, float penaltyScale, bool applyTradeOff)
    {
        this.rarity    = rarity;
        this.roomLevel = roomLevel;

        // Legendary rarity is reserved for the master key (set externally by ItemGenerator)
        isMasterKey = (rarity == ItemRarity.Master);
        keyColor    = RarityToColor(rarity);
        itemName    = isMasterKey ? "Master Key" : $"{keyColor} Key";
    }

    // Directly maps rarity name to matching KeyColor
    public static KeyColor RarityToColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Red    => KeyColor.Red,
            ItemRarity.Blue   => KeyColor.Blue,
            ItemRarity.Green  => KeyColor.Green,
            ItemRarity.Yellow => KeyColor.Yellow,
            ItemRarity.Master => KeyColor.Master,
            _                 => KeyColor.Red
        };
    }

    // Kept so any existing GetStatString() calls don't break
    public string GetStatString()
    {
        return isMasterKey ? "Opens the final room" : $"Opens {keyColor} doors";
    }
}