using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemUI : MonoBehaviour
{
    public static ItemUI Instance { get; private set; }

    [Header("Notification UI")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationTitle;
    [SerializeField] private TextMeshProUGUI notificationDesc;
    [SerializeField] private Image notificationBorder;

    [Header("Item Rack HUD (Bottom of screen)")]
    [SerializeField] private Transform itemRackContainer;
    [SerializeField] private GameObject itemIconPrefab;

    private Coroutine notificationCoroutine;
    private Dictionary<string, GameObject> activeIcons = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (notificationPanel != null) notificationPanel.SetActive(false);
    }

    public void ShowPickupNotification(ItemData item, int stack)
    {
        if (notificationPanel == null) return;

        if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
        notificationCoroutine = StartCoroutine(ShowNotificationRoutine(item, stack));
    }

    private IEnumerator ShowNotificationRoutine(ItemData item, int stack)
    {
        notificationPanel.SetActive(true);
        if (notificationTitle != null)
        {
            string stackStr = stack > 1 ? $" (x{stack})" : "";
            notificationTitle.text = $"{item.itemName}{stackStr}";
            notificationTitle.color = item.GetRarityColor();
        }

        if (notificationDesc != null)
        {
            notificationDesc.text = item.description;
        }

        if (notificationBorder != null)
        {
            notificationBorder.color = item.GetRarityColor();
        }

        yield return new WaitForSeconds(3f);

        notificationPanel.SetActive(false);
    }

    public void RefreshUI(PlayerInventory inventory)
    {
        if (itemRackContainer == null || inventory == null) return;

        var items = inventory.GetOwnedItems();

        foreach (var kvp in items)
        {
            string id = kvp.Key;
            int count = kvp.Value;
            ItemData data = inventory.GetItemData(id);
            if (data == null) continue;

            if (!activeIcons.ContainsKey(id))
            {
                GameObject iconObj = null;
                if (itemIconPrefab != null)
                {
                    iconObj = Instantiate(itemIconPrefab, itemRackContainer);
                }
                else
                {
                    iconObj = CreateDefaultIconSlot(data);
                }
                activeIcons[id] = iconObj;
            }

            GameObject slot = activeIcons[id];
            if (slot != null)
            {
                TextMeshProUGUI badge = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (badge != null)
                {
                    badge.text = count > 1 ? count.ToString() : "";
                }
                Image img = slot.GetComponentInChildren<Image>();
                if (img != null)
                {
                    if (data.itemIcon != null) img.sprite = data.itemIcon;
                    img.color = data.GetRarityColor();
                }
            }
        }
    }

    private GameObject CreateDefaultIconSlot(ItemData data)
    {
        GameObject slot = new GameObject($"ItemSlot_{data.id}");
        slot.transform.SetParent(itemRackContainer, false);

        RectTransform rt = slot.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40);

        Image img = slot.AddComponent<Image>();
        if (data.itemIcon != null) img.sprite = data.itemIcon;
        img.color = data.GetRarityColor();

        GameObject badgeObj = new GameObject("Badge");
        badgeObj.transform.SetParent(slot.transform, false);
        RectTransform badgeRt = badgeObj.AddComponent<RectTransform>();
        badgeRt.anchorMin = new Vector2(0.5f, 0f);
        badgeRt.anchorMax = new Vector2(1f, 0.5f);
        badgeRt.offsetMin = Vector2.zero;
        badgeRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = badgeObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.BottomRight;
        tmp.color = Color.yellow;
        tmp.fontStyle = FontStyles.Bold;

        return slot;
    }
}
