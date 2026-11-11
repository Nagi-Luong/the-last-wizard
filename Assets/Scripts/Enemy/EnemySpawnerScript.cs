using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    [Header("Enemy Prefab (Base)")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnArea = new Vector2(10f, 10f);
    [SerializeField] private float minSpawnTime = 5.0f;  // Giãn cách spawn lâu hơn
    [SerializeField] private float maxSpawnTime = 8.5f;
    [SerializeField] private int maxEnemiesAlive = 7;     // Tối đa chỉ 7 quái trên map

    [Header("Teleporter Rush Mode")]
    public bool isTeleporterActive = false;

    private float spawnTime;
    private Transform player;

    void Awake()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        SetSpawnTime();
    }

    void Update()
    {
        if (player == null) return;

        spawnTime -= Time.deltaTime;
        if (spawnTime <= 0)
        {
            // Kiểm tra giới hạn số quái sống trên map
            EnemyScript[] existingEnemies = Object.FindObjectsByType<EnemyScript>(FindObjectsInactive.Exclude);
            if (existingEnemies.Length < maxEnemiesAlive)
            {
                SpawnRandomEnemy();
            }
            SetSpawnTime();
        }
    }

    private void SpawnRandomEnemy()
    {
        if (enemyPrefab == null || player == null) return;

        // Vị trí spawn quanh người chơi (không quá gần, từ 6 đến 11 units)
        Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(6f, 11f);
        Vector3 spawnPosition = player.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        SpawnEnemyOfType(spawnPosition, GetRandomEnemyType());
    }

    private void Start()
    {
        // Đầu trận chỉ sinh 1 quái vật ở cự ly vừa phải để người chơi làm quen
        if (player != null)
        {
            SpawnEnemyOfType(player.position + new Vector3(-7f, 3f, 0), EnemyType.Clayman);
        }
    }

    private EnemyType GetRandomEnemyType()
    {
        float roll = Random.value;
        if (roll < 0.25f) return EnemyType.Skelly;
        if (roll < 0.50f) return EnemyType.Clayman;
        if (roll < 0.70f) return EnemyType.Archer;
        if (roll < 0.85f) return EnemyType.Imp;
        return EnemyType.Golem;
    }

    private void SpawnEnemyOfType(Vector3 pos, EnemyType type)
    {
        if (enemyPrefab == null) return;
        GameObject enemyObj = Instantiate(enemyPrefab, pos, Quaternion.identity);
        EnemyScript enemyScript = enemyObj.GetComponent<EnemyScript>();
        if (enemyScript != null)
        {
            enemyScript.SetEnemyType(type);
        }
    }

    private void SetSpawnTime()
    {
        float min = minSpawnTime;
        float max = maxSpawnTime;

        // Khi Teleporter đang sạc, quái vật ùa ra gấp đôi
        if (isTeleporterActive)
        {
            min *= 0.5f;
            max *= 0.5f;
        }

        spawnTime = Random.Range(min, max);
    }
}
