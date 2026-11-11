using System.Collections;
using UnityEngine;

public class BossSkeletonKing : MonoBehaviour
{
    private EnemyScript enemyScript;
    private Transform player;

    [Header("Skill Timers")]
    [SerializeField] private float summonCooldown = 12f;
    [SerializeField] private float shockwaveCooldown = 8f;
    private float nextSummonTime = 5f;
    private float nextShockwaveTime = 4f;

    [Header("Minion Prefabs")]
    public GameObject minionPrefab;
    public GameObject shockwavePrefab;

    private void Start()
    {
        enemyScript = GetComponent<EnemyScript>();
        if (enemyScript != null)
        {
            enemyScript.isBoss = true;
            enemyScript.bossName = "⚔️ CỐT TINH CHÚA - SKELETON WARLORD";
        }

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        if (BossHealthBarUI.Instance != null)
        {
            BossHealthBarUI.Instance.Show("⚔️ CỐT TINH CHÚA - SKELETON WARLORD", 450f);
        }
    }

    private void Update()
    {
        if (enemyScript == null || enemyScript.enemyHealth <= 0 || player == null) return;

        // Kỹ năng 1: Triệu hồi tiểu quái
        if (Time.time >= nextSummonTime)
        {
            SummonMinions();
            nextSummonTime = Time.time + summonCooldown;
        }

        // Kỹ năng 2: Sóng xung kích chấn động
        if (Time.time >= nextShockwaveTime)
        {
            TriggerShockwave();
            nextShockwaveTime = Time.time + shockwaveCooldown;
        }
    }

    private void SummonMinions()
    {
        if (minionPrefab == null) return;
        for (int i = 0; i < 2; i++)
        {
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 2.5f;
            GameObject minion = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            EnemyScript es = minion.GetComponent<EnemyScript>();
            if (es != null)
            {
                es.SetEnemyType((Random.value < 0.5f) ? EnemyType.Clayman : EnemyType.Skelly);
                es.isBoss = false;
            }
        }
    }

    private void TriggerShockwave()
    {
        float range = 4.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerStatsScript stats = hit.GetComponent<PlayerStatsScript>();
                if (stats != null)
                {
                    stats.TakeDamage(35); // Đòn chấn động 35 máu!
                }
            }
        }
    }
}
