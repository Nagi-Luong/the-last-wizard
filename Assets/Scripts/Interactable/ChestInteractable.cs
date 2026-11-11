using UnityEngine;
using TMPro;

public class ChestInteractable : MonoBehaviour
{
    public bool isOpened = false;
    public bool isRareChest = false;

    [Header("UI Visual")]
    [SerializeField] private TextMeshPro promptText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite openSprite;

    [Header("Item Drop")]
    public GameObject itemDropPrefab;

    private Transform player;
    private bool playerInRange = false;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isOpened || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        playerInRange = (dist <= 2.0f);

        if (promptText != null)
        {
            promptText.gameObject.SetActive(playerInRange);
            if (playerInRange)
            {
                promptText.text = isRareChest ? "[E] Mở Rương Hiếm" : "[E] Mở Rương";
            }
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        isOpened = true;
        if (promptText != null) promptText.gameObject.SetActive(false);

        if (openSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = openSprite;
        }

        // Rơi ra Item
        if (PlayerInventory.Instance != null)
        {
            ItemData.ItemRarity? rarity = null;
            if (isRareChest)
            {
                rarity = (Random.value < 0.35f) ? ItemData.ItemRarity.Legendary : ItemData.ItemRarity.Blue;
            }
            else
            {
                float r = Random.value;
                rarity = (r < 0.70f) ? ItemData.ItemRarity.White : (r < 0.90f ? ItemData.ItemRarity.Blue : ItemData.ItemRarity.Red);
            }

            ItemData data = PlayerInventory.Instance.GetRandomItem(rarity);
            if (data != null)
            {
                GameObject prefab = itemDropPrefab ?? PlayerInventory.Instance.itemDropPrefab;
                if (prefab != null)
                {
                    Vector3 dropPos = transform.position + new Vector3(0, 0.5f, 0);
                    GameObject dropObj = Instantiate(prefab, dropPos, Quaternion.identity);
                    ItemDrop dropScript = dropObj.GetComponent<ItemDrop>();
                    if (dropScript != null) dropScript.Setup(data);
                }
            }
        }
    }
}
