using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EnemyType
{
    Skelly,    // Bộ xương cận chiến cơ bản
    Clayman,   // Người đất chạy nhanh áp sát
    Archer,    // Cung thủ / Ong độc bắn tầm xa
    Imp,       // Tiểu quỷ dịch chuyển chớp nhoáng
    Golem      // Người đá khổng lồ tank trâu bò
}

public class EnemyScript : MonoBehaviour
{
    [Header("Enemy Classification")]
    public EnemyType enemyType = EnemyType.Skelly;
    public bool isBoss = false;
    public string bossName = "Skeleton King";

    [Header("Health & Stats")]
    [SerializeField] private int enemyMaxHealth = 50;
    public float enemyHealth;
    public float moveSpeed = 2.0f;
    public int attackDamage = 15;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;

    [Header("Ranged Attack (For Archer/Boss)")]
    public GameObject projectilePrefab;
    public float shootRange = 6.0f;
    public float shootCooldown = 2.0f;
    private float nextShootTime = 0f;

    [Header("Imp Teleport Skill")]
    public float teleportCooldown = 5.0f;
    private float nextTeleportTime = 0f;

    [Header("Item Drop on Death")]
    [Range(0f, 1f)] public float itemDropChance = 0.20f;
    public GameObject itemDropPrefab;

    private float originalMoveSpeed;
    private float slowTimer = 0f;

    private Animator animator;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private GameObject player;
    private PlayerStatsScript playerStats;

    [Header("UI References")]
    [SerializeField] private EnemyHealthBarScript healthBar;
    [SerializeField] private Image hp;
    [SerializeField] private Image border;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStatsScript>();
        }

        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        hp = GetComponentInChildren<Image>();

        ConfigureEnemyTypeStats();

        enemyHealth = enemyMaxHealth;
        originalMoveSpeed = moveSpeed;
    }

    public void SetEnemyType(EnemyType type)
    {
        enemyType = type;
        ConfigureEnemyTypeStats();
        enemyHealth = enemyMaxHealth;
        originalMoveSpeed = moveSpeed;
        if (healthBar != null)
        {
            healthBar.SetHealth(enemyHealth);
        }
    }

    private void ConfigureEnemyTypeStats()
    {
        Color enemyColor = Color.white;
        Vector3 enemyScale = Vector3.one;
        string typeTag = "[Bộ Xương]";
        Color tagColor = Color.white;
        Color lightColor = Color.white;

        switch (enemyType)
        {
            case EnemyType.Skelly:
                enemyMaxHealth = isBoss ? 450 : 50;
                attackDamage = isBoss ? 35 : 20; // Đòn chém mất 20 máu
                moveSpeed = isBoss ? 1.6f : 2.0f;
                attackRange = isBoss ? 2.5f : 1.0f;
                enemyColor = isBoss ? new Color(1f, 0.2f, 0.2f) : Color.white;
                enemyScale = isBoss ? Vector3.one * 2.5f : Vector3.one * 1.0f;
                typeTag = isBoss ? "👑 [CỐT TINH CHÚA]" : "[Bộ Xương Cận Chiến]";
                tagColor = isBoss ? Color.red : Color.white;
                lightColor = isBoss ? Color.red : new Color(0.8f, 0.8f, 0.9f);
                break;

            case EnemyType.Clayman:
                enemyMaxHealth = 35;
                attackDamage = 18; // Lao nhanh và húc 18 máu
                moveSpeed = 3.5f; // Chạy nhanh vượt trội
                attackRange = 0.9f;
                enemyColor = new Color(0.95f, 0.55f, 0.15f); // Cam đất nung
                enemyScale = Vector3.one * 0.85f; // Nhỏ gọn, linh hoạt
                typeTag = "⚡ [Người Đất - Tốc Độ]";
                tagColor = new Color(1f, 0.6f, 0.2f);
                lightColor = new Color(1f, 0.6f, 0.2f);
                break;

            case EnemyType.Archer:
                enemyMaxHealth = 40;
                attackDamage = 20; // Bắn tên mất 20 máu
                moveSpeed = 1.8f;
                attackRange = 0.8f;
                shootRange = 7.0f;
                enemyColor = new Color(0.2f, 0.95f, 0.55f); // Xanh lục ngọc độc
                enemyScale = Vector3.one * 1.05f;
                typeTag = "🏹 [Cung Thủ Xương]";
                tagColor = new Color(0.3f, 1f, 0.6f);
                lightColor = new Color(0.2f, 0.9f, 0.5f);
                break;

            case EnemyType.Imp:
                enemyMaxHealth = 60;
                attackDamage = 26; // Tiểu quỷ cào 26 máu
                moveSpeed = 2.4f;
                attackRange = 1.2f;
                enemyColor = new Color(0.95f, 0.15f, 0.45f); // Đỏ tím ác ma
                enemyScale = Vector3.one * 0.75f; // Bé hạt tiêu luồn lách
                typeTag = "🔮 [Tiểu Quỷ - Dịch Chuyển]";
                tagColor = new Color(1f, 0.3f, 0.7f);
                lightColor = new Color(0.8f, 0.2f, 0.9f);
                break;

            case EnemyType.Golem:
                enemyMaxHealth = isBoss ? 800 : 160;
                attackDamage = isBoss ? 50 : 38; // Đấm nửa cây máu (38 máu)
                moveSpeed = isBoss ? 1.2f : 1.1f;
                attackRange = isBoss ? 3.0f : 1.8f;
                enemyColor = isBoss ? new Color(0.65f, 0.15f, 0.95f) : new Color(0.45f, 0.52f, 0.62f); // Xám đá phiến
                enemyScale = isBoss ? Vector3.one * 2.8f : Vector3.one * 1.85f; // KHỔNG LỒ
                typeTag = isBoss ? "🐲 [MA LONG HƯ KHÔNG]" : "🛡️ [Người Đá - Khổng Lồ]";
                tagColor = isBoss ? new Color(0.8f, 0.2f, 1f) : new Color(0.7f, 0.8f, 0.9f);
                lightColor = isBoss ? new Color(0.7f, 0.1f, 1f) : new Color(0.4f, 0.7f, 1f);
                break;
        }

        // Áp dụng màu sắc và kích thước vật lý
        if (sprite != null)
        {
            sprite.color = enemyColor;
        }
        transform.localScale = enemyScale;

        if (isBoss)
        {
            itemDropChance = 1.0f;
        }

        // Cập nhật hoặc tạo mới nhãn tên trên đầu
        Transform existingTag = transform.Find("EnemyTag");
        TMPro.TextMeshPro tmp = null;
        if (existingTag != null)
        {
            tmp = existingTag.GetComponent<TMPro.TextMeshPro>();
        }
        else
        {
            GameObject tagObj = new GameObject("EnemyTag");
            tagObj.transform.SetParent(transform, false);
            tmp = tagObj.AddComponent<TMPro.TextMeshPro>();
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.sortingOrder = 12;
        }

        if (tmp != null)
        {
            tmp.text = typeTag;
            tmp.fontSize = isBoss ? 3.8f : 2.2f;
            tmp.color = tagColor;
            tmp.transform.localPosition = new Vector3(0, (isBoss ? 1.8f : 1.1f), 0);
        }

        // Cập nhật hoặc thêm ánh sáng hào quang đặc trưng quanh quái
        UnityEngine.Rendering.Universal.Light2D light = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        if (light == null) light = gameObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
        light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
        light.pointLightOuterRadius = isBoss ? 6.5f : 2.5f;
        light.pointLightInnerRadius = isBoss ? 1.5f : 0.3f;
        light.color = lightColor;
        light.intensity = isBoss ? 2.0f : 0.9f;
    }

    private void Update()
    {
        if (enemyHealth <= 0 || player == null) return;

        // Xử lý làm chậm
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                moveSpeed = originalMoveSpeed;
            }
        }

        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Kỹ năng đặc biệt của Imp: Dịch chuyển chớp nhoáng
        if (enemyType == EnemyType.Imp && Time.time >= nextTeleportTime && distToPlayer > 3f)
        {
            TeleportNearPlayer();
            nextTeleportTime = Time.time + teleportCooldown;
        }

        // Tấn công tầm xa (Archer / Bee / Ranged Boss)
        if ((enemyType == EnemyType.Archer || isBoss) && distToPlayer <= shootRange && Time.time >= nextShootTime)
        {
            ShootProjectileAtPlayer();
            nextShootTime = Time.time + shootCooldown;
        }

        // Cập nhật hoạt ảnh
        if (animator != null)
        {
            bool isWalking = (playerStats != null && playerStats.playerHealth > 0);
            animator.SetBool("IsWalking", isWalking);
        }
    }

    private void ShootProjectileAtPlayer()
    {
        if (player == null) return;
        Vector2 dir = (player.transform.position - transform.position).normalized;

        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null)
            {
                ep.Setup(dir, attackDamage, 5f);
            }
        }
    }

    private void TeleportNearPlayer()
    {
        if (player == null) return;
        Vector2 offset = Random.insideUnitCircle.normalized * 1.5f;
        transform.position = (Vector2)player.transform.position + offset;
    }

    public void ApplySlow(float amount, float duration)
    {
        if (isBoss) amount *= 0.5f; // Boss kháng 50% hiệu ứng làm chậm
        moveSpeed = originalMoveSpeed * (1f - Mathf.Clamp01(amount));
        slowTimer = duration;
    }

    public void TakeDamage(float damage)
    {
        enemyHealth -= damage;
        if (healthBar != null) healthBar.SetHealth(enemyHealth);

        if (isBoss && BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.UpdateHealth(enemyHealth, enemyMaxHealth);
        }

        if (enemyHealth <= 0)
        {
            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.SetBool("IsDead", true);
            }
            if (enemyCollider != null) Destroy(enemyCollider);
            Die();
        }
        else if (animator != null)
        {
            animator.SetTrigger("IsHit");
        }
    }

    private void Die()
    {
        if (sprite != null) sprite.sortingOrder = 3;

        // Thưởng XP cho người chơi
        int xpReward = isBoss ? 50 : (enemyType == EnemyType.Golem ? 15 : 5);
        if (playerStats != null) playerStats.GainXP(xpReward);

        // Kích hoạt nội tại diệt quái của PlayerInventory (Vampirism hút máu)
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnEnemyKilled();
        }

        // Thông báo Boss đã chết nếu là Boss
        if (isBoss)
        {
            if (BossHealthBarUI.Instance != null) BossHealthBarUI.Instance.Hide();
            TeleportPortal portal = Object.FindAnyObjectByType<TeleportPortal>();
            if (portal != null) portal.OnBossDefeated();
        }

        // Rơi vật phẩm (Risk of Rain loot drop)
        TryDropItem();

        if (hp != null) Destroy(hp, 0.5f);
        if (border != null) Destroy(border, 0.5f);
        Destroy(gameObject, 15f);
    }

    private void TryDropItem()
    {
        if (Random.value <= itemDropChance && PlayerInventory.Instance != null)
        {
            ItemData.ItemRarity? targetRarity = null;
            if (isBoss)
            {
                targetRarity = (Random.value < 0.4f) ? ItemData.ItemRarity.Legendary : ItemData.ItemRarity.Blue;
            }

            ItemData dropData = PlayerInventory.Instance.GetRandomItem(targetRarity);
            if (dropData != null)
            {
                GameObject dropPrefab = itemDropPrefab ?? PlayerInventory.Instance.itemDropPrefab;
                if (dropPrefab != null)
                {
                    GameObject dropObj = Instantiate(dropPrefab, transform.position, Quaternion.identity);
                    ItemDrop itemDrop = dropObj.GetComponent<ItemDrop>();
                    if (itemDrop != null) itemDrop.Setup(dropData);
                }
            }
        }
    }

    public void Attack()
    {
        if (playerStats == null || playerStats.playerHealth <= 0) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist <= attackRange + 0.5f)
        {
            playerStats.TakeDamage(attackDamage);
        }
    }
}
