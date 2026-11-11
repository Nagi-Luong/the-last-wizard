using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TeleportPortal : MonoBehaviour
{
    private enum PortalState { Idle, Charging, WaitingForBoss, Completed }
    [SerializeField] private PortalState currentState = PortalState.Idle;

    [Header("Next Level Settings")]
    public string nextSceneName = "SecondLevel";
    public bool isFinalLevel = false;

    [Header("Charge Settings")]
    public float chargeDuration = 45f;
    public float chargeRadius = 7.0f;
    private float currentChargeTimer = 0f;

    [Header("Boss Spawn Settings")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    private bool bossSpawned = false;
    private bool bossDefeated = false;

    [Header("UI Visuals")]
    [SerializeField] private TextMeshPro promptText;
    [SerializeField] private TextMeshPro statusText;
    [SerializeField] private SpriteRenderer portalGlow;
    [SerializeField] private LineRenderer radiusRenderer;

    private Transform playerTransform;
    private bool playerInRange = false;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        DrawRadiusCircle();
        UpdateVisuals();
    }

    private void DrawRadiusCircle()
    {
        if (radiusRenderer == null)
        {
            radiusRenderer = gameObject.AddComponent<LineRenderer>();
            radiusRenderer.useWorldSpace = false;
            radiusRenderer.loop = true;
            radiusRenderer.positionCount = 40;
            radiusRenderer.startWidth = 0.08f;
            radiusRenderer.endWidth = 0.08f;
            radiusRenderer.material = new Material(Shader.Find("Sprites/Default"));
            radiusRenderer.startColor = new Color(0.3f, 0.7f, 1f, 0.4f);
            radiusRenderer.endColor = new Color(0.3f, 0.7f, 1f, 0.4f);

            float deltaTheta = (2f * Mathf.PI) / 40;
            float theta = 0f;
            for (int i = 0; i < 40; i++)
            {
                float x = chargeRadius * Mathf.Cos(theta);
                float y = chargeRadius * Mathf.Sin(theta);
                radiusRenderer.SetPosition(i, new Vector3(x, y, 0));
                theta += deltaTheta;
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = (dist <= 2.5f);
        bool insideZone = (dist <= chargeRadius);

        // Xử lý phím tương tác E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (currentState == PortalState.Idle)
            {
                StartCharging();
            }
            else if (currentState == PortalState.Completed)
            {
                TeleportToNextLevel();
            }
        }

        // Logic sạc đếm ngược
        if (currentState == PortalState.Charging)
        {
            // Sạc nhanh khi ở trong vùng, ngoài vùng sạc chậm
            float speedMult = insideZone ? 1.0f : 0.2f;
            currentChargeTimer += Time.deltaTime * speedMult;

            float progress = Mathf.Clamp01(currentChargeTimer / chargeDuration);
            int percent = (int)(progress * 100);

            if (statusText != null)
            {
                statusText.text = $"SẠC CỔNG: {percent}%\n{(insideZone ? "<color=#00FF88>Trong vùng</color>" : "<color=#FF8800>Ngoài vùng (Chậm)</color>")}";
            }

            // Đồng hồ đạt mốc cuối nhưng chờ Boss
            if (currentChargeTimer >= chargeDuration)
            {
                if (bossDefeated)
                {
                    CompletePortal();
                }
                else
                {
                    currentState = PortalState.WaitingForBoss;
                    if (statusText != null)
                    {
                        statusText.text = "SẠC: 99%\n<color=#FF3333>TIÊU DIỆT BOSS ĐỂ MỞ CỔNG!</color>";
                    }
                }
            }
        }

        UpdateVisuals();
    }

    private void StartCharging()
    {
        currentState = PortalState.Charging;
        currentChargeTimer = 0f;

        // Triệu hồi Boss
        if (!bossSpawned && bossPrefab != null)
        {
            bossSpawned = true;
            Vector3 spawnPos = (bossSpawnPoint != null) ? bossSpawnPoint.position : transform.position + new Vector3(4f, 0f, 0f);
            Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }

        // Tăng tốc sinh quái vật
        EnemySpawnerScript spawner = Object.FindAnyObjectByType<EnemySpawnerScript>();
        if (spawner != null)
        {
            spawner.isTeleporterActive = true;
        }
    }

    public void OnBossDefeated()
    {
        bossDefeated = true;
        if (currentState == PortalState.WaitingForBoss || currentChargeTimer >= chargeDuration)
        {
            CompletePortal();
        }
    }

    private void CompletePortal()
    {
        currentState = PortalState.Completed;
        if (statusText != null)
        {
            statusText.text = "<color=#00FF88>CỔNG ĐÃ MỞ!</color>\nNhấn [E] để dịch chuyển";
        }
        if (portalGlow != null)
        {
            portalGlow.color = new Color(0.2f, 1f, 0.4f, 0.8f);
        }
    }

    private void TeleportToNextLevel()
    {
        // Lưu trạng thái Player
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.SaveToGameManager();
        }

        if (isFinalLevel)
        {
            // Chiến thắng trò chơi
            GameManagerScript gm = Object.FindAnyObjectByType<GameManagerScript>();
            if (gm != null) gm.ShowVictory();
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void UpdateVisuals()
    {
        if (promptText != null)
        {
            if (currentState == PortalState.Idle && playerInRange)
            {
                promptText.text = "[E] Kích hoạt Cổng Dịch Chuyển";
                promptText.gameObject.SetActive(true);
            }
            else if (currentState == PortalState.Completed && playerInRange)
            {
                promptText.text = isFinalLevel ? "[E] Thoát Khỏi Hầm Ngục (Chiến Thắng)!" : "[E] Tiến Vào Màn 2!";
                promptText.gameObject.SetActive(true);
            }
            else
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
}
