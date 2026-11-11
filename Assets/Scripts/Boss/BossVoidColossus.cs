using System.Collections;
using UnityEngine;

public class BossVoidColossus : MonoBehaviour
{
    private EnemyScript enemyScript;
    private Transform player;

    [Header("Skill Timers")]
    [SerializeField] private float novaCooldown = 6f;
    [SerializeField] private float chargeCooldown = 9f;
    private float nextNovaTime = 3f;
    private float nextChargeTime = 6f;

    [Header("Skill Prefabs")]
    public GameObject darkOrbPrefab;

    private bool isCharging = false;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyScript = GetComponent<EnemyScript>();
        if (enemyScript != null)
        {
            enemyScript.enemyType = EnemyType.Golem;
            enemyScript.isBoss = true;
            enemyScript.bossName = "🐲 MA LONG HƯ KHÔNG - VOID COLOSSUS";
        }

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.Show("🐲 MA LONG HƯ KHÔNG - VOID COLOSSUS", 800f);
        }
    }

    private void Update()
    {
        if (enemyScript == null || enemyScript.enemyHealth <= 0 || player == null) return;

        // Kỹ năng 1: Bắn chùm cầu lửa hắc ám 360 độ (Nova Barrage)
        if (Time.time >= nextNovaTime)
        {
            ShootDarkNova();
            nextNovaTime = Time.time + novaCooldown;
        }

        // Kỹ năng 2: Lao vút càn quét (Void Charge)
        if (Time.time >= nextChargeTime && !isCharging)
        {
            StartCoroutine(ChargeRoutine());
            nextChargeTime = Time.time + chargeCooldown;
        }
    }

    private void ShootDarkNova()
    {
        int projectileCount = 8;
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            if (darkOrbPrefab != null)
            {
                GameObject orb = Instantiate(darkOrbPrefab, transform.position, Quaternion.identity);
                EnemyProjectile ep = orb.GetComponent<EnemyProjectile>();
                if (ep != null)
                {
                    ep.Setup(dir, 25f, 4.5f);
                }
            }
        }
    }

    private IEnumerator ChargeRoutine()
    {
        isCharging = true;
        Vector2 targetDir = (player.position - transform.position).normalized;

        float chargeSpeed = 7.0f;
        float chargeDuration = 1.0f;
        float timer = 0f;

        while (timer < chargeDuration && enemyScript.enemyHealth > 0)
        {
            timer += Time.deltaTime;
            if (rb != null)
            {
                rb.linearVelocity = targetDir * chargeSpeed;
            }
            yield return null;
        }

        if (rb != null) rb.linearVelocity = Vector2.zero;
        isCharging = false;
    }
}
