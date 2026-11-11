using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ItemDrop : MonoBehaviour
{
    [Header("Item Config")]
    public ItemData itemData;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer glowRenderer;

    [Header("Floating / Bounce Animation")]
    [SerializeField] private float bounceHeight = 0.25f;
    [SerializeField] private float bounceSpeed = 3f;
    [SerializeField] private float magnetRange = 2.5f;
    [SerializeField] private float magnetSpeed = 6f;

    private Vector3 basePosition;
    private Transform playerTransform;
    private bool isCollected = false;

    private void Start()
    {
        basePosition = transform.position;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        UpdateVisuals();
    }

    public void Setup(ItemData data)
    {
        itemData = data;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (itemData == null) return;

        if (spriteRenderer != null)
        {
            if (itemData.itemIcon != null)
            {
                spriteRenderer.sprite = itemData.itemIcon;
            }
            spriteRenderer.color = itemData.GetRarityColor();
        }

        if (glowRenderer != null)
        {
            glowRenderer.color = new Color(itemData.GetRarityColor().r, itemData.GetRarityColor().g, itemData.GetRarityColor().b, 0.4f);
        }
    }

    private void Update()
    {
        if (isCollected) return;

        // Hiệu ứng lơ lửng nhẹ nhàng
        float newY = basePosition.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        transform.position = new Vector3(basePosition.x, newY, basePosition.z);

        // Hiệu ứng nam châm hút về phía Player khi lại gần
        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= magnetRange)
            {
                basePosition = Vector3.MoveTowards(basePosition, playerTransform.position, magnetSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;

        if (collision.CompareTag("Player"))
        {
            PlayerInventory inventory = collision.GetComponent<PlayerInventory>();
            if (inventory != null && itemData != null)
            {
                isCollected = true;
                inventory.AddItem(itemData);
                Destroy(gameObject);
            }
        }
    }
}
