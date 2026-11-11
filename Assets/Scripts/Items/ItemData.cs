using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Roguelike/Item Data")]
public class ItemData : ScriptableObject
{
    public enum ItemRarity 
    { 
        White, 
        Blue, 
        Red, 
        Legendary = Red, 
        Special 
    }

    [Header("Định danh vật phẩm (ID viết liền)")]
    public string itemID;
    public string id 
    { 
        get => itemID; 
        set => itemID = value; 
    }

    [Header("Tên hiển thị")]
    public string itemName;

    [Header("Mô tả")]
    [TextArea(2, 5)]
    public string description;

    [Header("Icon Sprite")]
    public Sprite itemIcon;

    [Header("Độ hiếm")]
    public ItemRarity rarity = ItemRarity.White;

    [Header("Màu sắc đại diện")]
    public Color itemColor = Color.white;

    [Header("Chỉ số cộng thêm mỗi Stack")]
    public float moveSpeedBonusPercent = 0f;    // Ví dụ: 0.15 = +15%
    public float attackSpeedBonusPercent = 0f;  // Ví dụ: 0.15 = +15%
    public float damageBonusPercent = 0f;       // Ví dụ: 0.20 = +20%
    public float maxHealthBonus = 0f;           // Ví dụ: +25 HP
    public float critChanceBonus = 0f;          // Ví dụ: 0.10 = +10%
    public float lifestealPerKill = 0f;         // Hồi HP khi diệt quái
    public float slowAmount = 0f;               // Làm chậm quái (Cold Snap)
    public float aoeBlastPercent = 0f;          // Sát thương nổ lan (Abyss Nova)
    public bool hasHomingStar = false;          // Ngôi sao truy đuổi (Star Piece)
    public bool hasShield = false;              // Giáp ảo hồi lại sau 3s
    public bool hasRevive = false;              // Hồi sinh (Rebirth Story)

    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.White: return new Color(0.9f, 0.9f, 0.9f);
            case ItemRarity.Blue: return new Color(0.3f, 0.7f, 1f);
            case ItemRarity.Red: return new Color(1f, 0.25f, 0.25f);
            case ItemRarity.Special: return new Color(1f, 0.85f, 0.2f);
            default: return Color.white;
        }
    }
}
