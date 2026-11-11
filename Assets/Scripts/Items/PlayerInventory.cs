using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SavedItemEntry
{
    public string itemID;
    public int count;
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Item Stacks (Dictionary ID -> Count)")]
    private Dictionary<string, int> ownedItems = new Dictionary<string, int>();
    private Dictionary<string, ItemData> itemDatabase = new Dictionary<string, ItemData>();

    [Header("Default Items Database (Auto populated)")]
    public List<ItemData> availableItems = new List<ItemData>();

    [Header("Prefab for Dropped Items")]
    public GameObject itemDropPrefab;

    [Header("Sound Effects")]
    public AudioClip pickupSound;

    // Các chỉ số tính toán cộng dồn
    public float MoveSpeedMultiplier { get; private set; } = 1f;
    public float AttackSpeedMultiplier { get; private set; } = 1f;
    public float DamageMultiplier { get; private set; } = 1f;
    public float MaxHealthBonus { get; private set; } = 0f;
    public float CritChance { get; private set; } = 0.05f;
    public float LifestealOnKill { get; private set; } = 0f;
    public float SlowEffect { get; private set; } = 0f;
    public float AoeBlastPercent { get; private set; } = 0f;
    public bool HasHomingStars { get; private set; } = false;
    public bool HasShield { get; private set; } = false;

    // Giáp ảo
    public float ShieldAmount { get; private set; } = 0f;
    public float MaxShield { get; private set; } = 0f;
    private float shieldRegenTimer = 0f;

    private PlayerStatsScript playerStats;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        playerStats = GetComponent<PlayerStatsScript>();
        InitializeDefaultItems();
    }

    private void Start()
    {
        // Phục hồi dữ liệu túi đồ từ GameManager nếu chuyển màn
        if (GameManagerScript.savedInventory != null && GameManagerScript.savedInventory.Count > 0)
        {
            foreach (var saved in GameManagerScript.savedInventory)
            {
                if (itemDatabase.TryGetValue(saved.itemID, out ItemData data))
                {
                    for (int i = 0; i < saved.count; i++)
                    {
                        AddItemInternal(data, false);
                    }
                }
            }
            RecalculateStats();
            if (ItemUI.Instance != null) ItemUI.Instance.RefreshUI(this);
        }
    }

    private void Update()
    {
        // Tái tạo giáp ảo sau 3 giây không bị đánh
        if (HasShield && MaxShield > 0f)
        {
            shieldRegenTimer += Time.deltaTime;
            if (shieldRegenTimer >= 3f && ShieldAmount < MaxShield)
            {
                ShieldAmount = Mathf.Min(MaxShield, ShieldAmount + (MaxShield * 0.2f * Time.deltaTime));
            }
        }
    }

    private void InitializeDefaultItems()
    {
        // Khởi tạo các item chuẩn nếu danh sách trống
        if (availableItems.Count == 0)
        {
            availableItems.Add(CreateItem("runway_drive", "Runway Drive", "Tăng 15% tốc độ di chuyển mỗi stack", ItemData.ItemRarity.White, new Color(0.3f, 0.9f, 0.3f), moveSpeed: 0.15f));
            availableItems.Add(CreateItem("little_nova", "Little Nova", "Tăng 15% tốc độ bắn cầu lửa mỗi stack", ItemData.ItemRarity.White, new Color(1f, 0.8f, 0.2f), attackSpeed: 0.15f));
            availableItems.Add(CreateItem("ephemeral", "Ephemeral", "Tăng 20% sát thương mỗi stack", ItemData.ItemRarity.White, new Color(1f, 0.4f, 0.2f), damage: 0.20f));
            availableItems.Add(CreateItem("vampire_fang", "Vampire Fang", "Hồi 2 máu mỗi khi diệt 1 quái vật", ItemData.ItemRarity.Blue, new Color(0.9f, 0.2f, 0.2f), lifesteal: 2f));
            availableItems.Add(CreateItem("cold_snap", "Cold Snap", "Bắn trúng làm chậm quái đi 30%", ItemData.ItemRarity.Blue, new Color(0.4f, 0.8f, 1f), slow: 0.30f));
            availableItems.Add(CreateItem("fairy_queen", "Fairy Queen", "Cung cấp giáp chắn 25 máu hồi lại sau 3s", ItemData.ItemRarity.Blue, new Color(0.9f, 0.4f, 0.9f), shield: true));
            availableItems.Add(CreateItem("titan_heart", "Titan Heart", "Tăng 30 HP tối đa mỗi stack", ItemData.ItemRarity.Red, new Color(1f, 0.1f, 0.1f), maxHealth: 30f));
            availableItems.Add(CreateItem("abyss_nova", "Abyss Nova", "Đòn đánh nổ lan 40% sát thương diện rộng", ItemData.ItemRarity.Red, new Color(0.6f, 0.1f, 0.9f), aoeBlast: 0.40f));
            availableItems.Add(CreateItem("rebirth_story", "Rebirth Story", "Tự động hồi sinh khi bị hạ gục (1 lần dùng)", ItemData.ItemRarity.Red, new Color(1f, 0.9f, 0.4f), revive: true));
        }

        foreach (var itm in availableItems)
        {
            if (itm != null && !string.IsNullOrEmpty(itm.itemID))
            {
                itemDatabase[itm.itemID] = itm;
            }
        }
    }

    private ItemData CreateItem(string id, string name, string desc, ItemData.ItemRarity rarity, Color col,
        float moveSpeed = 0f, float attackSpeed = 0f, float damage = 0f, float maxHealth = 0f, float lifesteal = 0f,
        float slow = 0f, float aoeBlast = 0f, bool shield = false, bool revive = false)
    {
        ItemData item = ScriptableObject.CreateInstance<ItemData>();
        item.id = id;
        item.itemID = id;
        item.itemName = name;
        item.description = desc;
        item.rarity = rarity;
        item.itemColor = col;
        item.moveSpeedBonusPercent = moveSpeed;
        item.attackSpeedBonusPercent = attackSpeed;
        item.damageBonusPercent = damage;
        item.maxHealthBonus = maxHealth;
        item.lifestealPerKill = lifesteal;
        item.slowAmount = slow;
        item.aoeBlastPercent = aoeBlast;
        item.hasShield = shield;
        item.hasRevive = revive;
        return item;
    }

    public void AddItem(ItemData item)
    {
        AddItemInternal(item, true);
    }

    private void AddItemInternal(ItemData item, bool triggerUI)
    {
        if (item == null || string.IsNullOrEmpty(item.itemID)) return;

        if (ownedItems.ContainsKey(item.itemID))
            ownedItems[item.itemID]++;
        else
            ownedItems[item.itemID] = 1;

        if (!itemDatabase.ContainsKey(item.itemID))
            itemDatabase[item.itemID] = item;

        RecalculateStats();

        // Hiển thị thông báo nhặt đồ lên màn hình
        if (triggerUI && ItemUI.Instance != null)
        {
            ItemUI.Instance.ShowPickupNotification(item, ownedItems[item.itemID]);
            ItemUI.Instance.RefreshUI(this);
        }

        // Tự động lưu vào GameManager để giữ khi chuyển màn
        SaveToGameManager();
    }

    public void RecalculateStats()
    {
        MoveSpeedMultiplier = 1f;
        AttackSpeedMultiplier = 1f;
        DamageMultiplier = 1f;
        MaxHealthBonus = 0f;
        CritChance = 0.05f;
        LifestealOnKill = 0f;
        SlowEffect = 0f;
        AoeBlastPercent = 0f;
        HasShield = false;
        MaxShield = 0f;

        foreach (var kvp in ownedItems)
        {
            if (itemDatabase.TryGetValue(kvp.Key, out ItemData data))
            {
                int stack = kvp.Value;
                MoveSpeedMultiplier += data.moveSpeedBonusPercent * stack;
                AttackSpeedMultiplier += data.attackSpeedBonusPercent * stack;
                DamageMultiplier += data.damageBonusPercent * stack;
                MaxHealthBonus += data.maxHealthBonus * stack;
                CritChance += data.critChanceBonus * stack;
                LifestealOnKill += data.lifestealPerKill * stack;
                SlowEffect = Mathf.Max(SlowEffect, data.slowAmount);
                AoeBlastPercent += data.aoeBlastPercent * stack;
                if (data.hasShield)
                {
                    HasShield = true;
                    MaxShield += 25f * stack;
                }
            }
        }

        // Cập nhật máu tối đa của Player
        if (playerStats != null)
        {
            playerStats.UpdateMaxHealthBonus(MaxHealthBonus);
        }
    }

    public int GetItemStack(string id)
    {
        return ownedItems.ContainsKey(id) ? ownedItems[id] : 0;
    }

    public Dictionary<string, int> GetOwnedItems()
    {
        return ownedItems;
    }

    public ItemData GetItemData(string id)
    {
        itemDatabase.TryGetValue(id, out ItemData d);
        return d;
    }

    public bool TryConsumeRevive()
    {
        if (GetItemStack("rebirth_story") > 0)
        {
            ownedItems["rebirth_story"]--;
            if (ownedItems["rebirth_story"] <= 0) ownedItems.Remove("rebirth_story");
            RecalculateStats();
            if (ItemUI.Instance != null) ItemUI.Instance.RefreshUI(this);
            SaveToGameManager();
            return true;
        }
        return false;
    }

    public void OnEnemyKilled()
    {
        if (LifestealOnKill > 0f && playerStats != null)
        {
            playerStats.Heal(LifestealOnKill);
        }
    }

    public void OnPlayerTakeDamage()
    {
        shieldRegenTimer = 0f;
    }

    public void SaveToGameManager()
    {
        List<SavedItemEntry> list = new List<SavedItemEntry>();
        foreach (var kvp in ownedItems)
        {
            list.Add(new SavedItemEntry { itemID = kvp.Key, count = kvp.Value });
        }
        GameManagerScript.savedInventory = list;
    }

    public ItemData GetRandomItem(ItemData.ItemRarity? targetRarity = null)
    {
        List<ItemData> pool = new List<ItemData>();
        foreach (var itm in availableItems)
        {
            if (targetRarity == null || itm.rarity == targetRarity)
            {
                pool.Add(itm);
            }
        }
        if (pool.Count == 0) pool = availableItems;
        return pool[UnityEngine.Random.Range(0, pool.Count)];
    }
}
