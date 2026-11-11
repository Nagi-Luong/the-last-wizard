using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.UI;

public class DungeonLevelSetup : MonoBehaviour
{
    [Header("Level Info")]
    public int levelIndex = 1; // 1 = FirstLevel, 2 = SecondLevel
    public string levelName = "HẦM NGỤC CỐT TINH (STAGE 1)";

    [Header("Teleporter Position (Gần người chơi)")]
    public Vector2 teleporterLocation = new Vector2(5.0f, 0f);

    [Header("Chests Setup")]
    public int chestCount = 5;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "Start" || sceneName.Contains("Menu")) return;

        if (Object.FindAnyObjectByType<DungeonLevelSetup>() == null)
        {
            GameObject setupObj = new GameObject("DungeonLevelSetup");
            DungeonLevelSetup setup = setupObj.AddComponent<DungeonLevelSetup>();
            setup.levelIndex = (sceneName == "SecondLevel" || sceneName == "GrassLevel") ? 2 : 1;
            setup.levelName = (setup.levelIndex == 2) ? "RỪNG CẤM HƯ KHÔNG (STAGE 2)" : "HẦM NGỤC CỐT TINH (STAGE 1)";
            setup.teleporterLocation = new Vector2(5.0f, 0f);
        }
    }

    private void Awake()
    {
        SetupArenaBoundaries();
        SetupLighting();
        SetupUIElements();
        SetupTeleporter();
        SetupChests();
        SetupTorchesAndParticles();
    }

    private void SetupArenaBoundaries()
    {
        // Tạo 4 bức tường giới hạn đấu trường để người chơi không chạy mãi ra khoảng đen vô tận
        if (GameObject.Find("ArenaBoundaries") != null) return;

        GameObject boundaryParent = new GameObject("ArenaBoundaries");
        float halfW = 20f;
        float halfH = 14f;

        CreateWall(boundaryParent.transform, "Wall_North", new Vector2(0, halfH), new Vector2(halfW * 2 + 4, 2f));
        CreateWall(boundaryParent.transform, "Wall_South", new Vector2(0, -halfH), new Vector2(halfW * 2 + 4, 2f));
        CreateWall(boundaryParent.transform, "Wall_West", new Vector2(-halfW, 0), new Vector2(2f, halfH * 2 + 4));
        CreateWall(boundaryParent.transform, "Wall_East", new Vector2(halfW, 0), new Vector2(2f, halfH * 2 + 4));
    }

    private void CreateWall(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent, false);
        wall.transform.position = pos;
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = size;

        // Vẽ vạch ranh giới trực quan
        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(0.2f, 0.25f, 0.35f, 0.9f));
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        sr.sortingOrder = 5;
        wall.transform.localScale = new Vector3(size.x, size.y, 1);
    }

    private void SetupLighting()
    {
        Light2D[] lights = Object.FindObjectsByType<Light2D>(FindObjectsInactive.Exclude);
        foreach (var l in lights)
        {
            if (l.lightType == Light2D.LightType.Global)
            {
                l.color = (levelIndex == 1) 
                    ? new Color(0.40f, 0.42f, 0.55f, 1f)    // Xanh tím hầm ngục
                    : new Color(0.25f, 0.45f, 0.35f, 1f);  // Xanh lục ma mị rừng cấm
                l.intensity = 0.9f;
            }
        }
    }

    private void SetupUIElements()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // 1. Tạo BossHealthBarUI nếu chưa có
        if (Object.FindAnyObjectByType<BossHealthBarUI>() == null)
        {
            GameObject bossUIObj = new GameObject("BossHealthBarHUD");
            bossUIObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = bossUIObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 0.92f);
            rt.anchorMax = new Vector2(0.8f, 0.98f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bgImg = bossUIObj.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);

            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(bossUIObj.transform, false);
            RectTransform sRt = sliderObj.AddComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.02f, 0.15f);
            sRt.anchorMax = new Vector2(0.98f, 0.60f);
            sRt.offsetMin = Vector2.zero;
            sRt.offsetMax = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fRt = fillObj.AddComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.offsetMin = Vector2.zero;
            fRt.offsetMax = Vector2.zero;
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = (levelIndex == 1) ? new Color(0.9f, 0.15f, 0.15f) : new Color(0.6f, 0.1f, 0.9f);
            slider.fillRect = fRt;

            GameObject nameObj = new GameObject("BossNameText");
            nameObj.transform.SetParent(bossUIObj.transform, false);
            RectTransform nRt = nameObj.AddComponent<RectTransform>();
            nRt.anchorMin = new Vector2(0.05f, 0.6f);
            nRt.anchorMax = new Vector2(0.95f, 1.2f);
            nRt.offsetMin = Vector2.zero;
            nRt.offsetMax = Vector2.zero;
            TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 18;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.color = Color.white;
            nameTmp.fontStyle = FontStyles.Bold;

            BossHealthBarUI barUI = bossUIObj.AddComponent<BossHealthBarUI>();
            var containerField = typeof(BossHealthBarUI).GetField("container", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (containerField != null) containerField.SetValue(barUI, bossUIObj);
            var sliderField = typeof(BossHealthBarUI).GetField("healthSlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (sliderField != null) sliderField.SetValue(barUI, slider);
            var nameField = typeof(BossHealthBarUI).GetField("bossNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (nameField != null) nameField.SetValue(barUI, nameTmp);

            bossUIObj.SetActive(false);
        }

        // 2. Tạo ItemUI nếu chưa có
        if (Object.FindAnyObjectByType<ItemUI>() == null)
        {
            GameObject itemUIObj = new GameObject("ItemUI_Manager");
            itemUIObj.transform.SetParent(canvas.transform, false);

            GameObject rackObj = new GameObject("ItemRack");
            rackObj.transform.SetParent(itemUIObj.transform, false);
            RectTransform rackRt = rackObj.AddComponent<RectTransform>();
            rackRt.anchorMin = new Vector2(0.02f, 0.02f);
            rackRt.anchorMax = new Vector2(0.6f, 0.12f);
            rackRt.offsetMin = Vector2.zero;
            rackRt.offsetMax = Vector2.zero;

            HorizontalLayoutGroup hlg = rackObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            GameObject notifObj = new GameObject("PickupNotification");
            notifObj.transform.SetParent(itemUIObj.transform, false);
            RectTransform notifRt = notifObj.AddComponent<RectTransform>();
            notifRt.anchorMin = new Vector2(0.3f, 0.2f);
            notifRt.anchorMax = new Vector2(0.7f, 0.3f);
            notifRt.offsetMin = Vector2.zero;
            notifRt.offsetMax = Vector2.zero;

            Image notifBg = notifObj.AddComponent<Image>();
            notifBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(notifObj.transform, false);
            RectTransform tRt = titleObj.AddComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.05f, 0.5f);
            tRt.anchorMax = new Vector2(0.95f, 0.95f);
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.fontSize = 16;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;

            GameObject descObj = new GameObject("Desc");
            descObj.transform.SetParent(notifObj.transform, false);
            RectTransform dRt = descObj.AddComponent<RectTransform>();
            dRt.anchorMin = new Vector2(0.05f, 0.05f);
            dRt.anchorMax = new Vector2(0.95f, 0.5f);
            dRt.offsetMin = Vector2.zero;
            dRt.offsetMax = Vector2.zero;
            TextMeshProUGUI descTmp = descObj.AddComponent<TextMeshProUGUI>();
            descTmp.fontSize = 12;
            descTmp.alignment = TextAlignmentOptions.Center;
            descTmp.color = Color.white;

            ItemUI itemUI = itemUIObj.AddComponent<ItemUI>();
            var rackField = typeof(ItemUI).GetField("itemRackContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rackField != null) rackField.SetValue(itemUI, rackObj.transform);
            var panelField = typeof(ItemUI).GetField("notificationPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (panelField != null) panelField.SetValue(itemUI, notifObj);
            var titleField = typeof(ItemUI).GetField("notificationTitle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (titleField != null) titleField.SetValue(itemUI, titleTmp);
            var descField = typeof(ItemUI).GetField("notificationDesc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (descField != null) descField.SetValue(itemUI, descTmp);

            notifObj.SetActive(false);
        }
    }

    private void SetupTeleporter()
    {
        if (Object.FindAnyObjectByType<TeleportPortal>() != null) return;

        GameObject portalObj = new GameObject("TeleportPortal");
        portalObj.transform.position = new Vector3(teleporterLocation.x, teleporterLocation.y, 0);

        // Vòng tròn năng lượng phát sáng
        GameObject visual = new GameObject("PortalVisual");
        visual.transform.SetParent(portalObj.transform, false);
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();

        Texture2D tex = CreateCircleTexture(64, (levelIndex == 1) ? new Color(0.1f, 0.85f, 1f, 0.9f) : new Color(0.85f, 0.2f, 1f, 0.9f));
        Sprite circleSprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 32);
        sr.sprite = circleSprite;
        sr.sortingOrder = 8; // Luôn hiển thị trên nền đất
        visual.transform.localScale = new Vector3(3.2f, 3.2f, 1f);

        // Đèn phát sáng rực rỡ để người chơi nhìn thấy từ xa
        Light2D pLight = portalObj.AddComponent<Light2D>();
        pLight.lightType = Light2D.LightType.Point;
        pLight.pointLightOuterRadius = 6.0f;
        pLight.pointLightInnerRadius = 1.5f;
        pLight.color = (levelIndex == 1) ? new Color(0f, 0.9f, 1f) : new Color(0.8f, 0.1f, 1f);
        pLight.intensity = 2.0f;

        // Text gợi ý bấm E
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(portalObj.transform, false);
        textObj.transform.localPosition = new Vector3(0, 2.2f, 0);
        TextMeshPro tmpPrompt = textObj.AddComponent<TextMeshPro>();
        tmpPrompt.fontSize = 5;
        tmpPrompt.alignment = TextAlignmentOptions.Center;
        tmpPrompt.color = Color.yellow;
        tmpPrompt.fontStyle = FontStyles.Bold;
        tmpPrompt.text = "[E] Kích hoạt Cổng Dịch Chuyển";
        tmpPrompt.sortingOrder = 15;

        // Text trạng thái sạc
        GameObject statusObj = new GameObject("StatusText");
        statusObj.transform.SetParent(portalObj.transform, false);
        statusObj.transform.localPosition = new Vector3(0, 3.2f, 0);
        TextMeshPro tmpStatus = statusObj.AddComponent<TextMeshPro>();
        tmpStatus.fontSize = 4.2f;
        tmpStatus.alignment = TextAlignmentOptions.Center;
        tmpStatus.color = Color.cyan;
        tmpStatus.fontStyle = FontStyles.Bold;
        tmpStatus.text = "";
        tmpStatus.sortingOrder = 15;

        TeleportPortal portal = portalObj.AddComponent<TeleportPortal>();
        portal.chargeDuration = 35f;
        portal.nextSceneName = (levelIndex == 1) ? "SecondLevel" : "FirstLevel";
        portal.isFinalLevel = (levelIndex == 2);

        GameObject bossPrefab = CreateBossPrefab(levelIndex);
        portal.bossPrefab = bossPrefab;

        var pField = typeof(TeleportPortal).GetField("promptText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pField != null) pField.SetValue(portal, tmpPrompt);
        var sField = typeof(TeleportPortal).GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (sField != null) sField.SetValue(portal, tmpStatus);
        var gField = typeof(TeleportPortal).GetField("portalGlow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (gField != null) gField.SetValue(portal, sr);
    }

    private GameObject CreateBossPrefab(int lvl)
    {
        GameObject originalEnemy = Resources.Load<GameObject>("Enemy");
        if (originalEnemy == null)
        {
            EnemyScript es = Object.FindAnyObjectByType<EnemyScript>();
            if (es != null) originalEnemy = es.gameObject;
        }

        GameObject boss = new GameObject(lvl == 1 ? "Boss_SkeletonKing" : "Boss_VoidColossus");
        SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
        Texture2D bossTex = CreateCircleTexture(64, lvl == 1 ? new Color(1f, 0.1f, 0.1f) : new Color(0.6f, 0f, 0.9f));
        sr.sprite = Sprite.Create(bossTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 32);
        sr.sortingOrder = 10;

        Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        CircleCollider2D col = boss.AddComponent<CircleCollider2D>();
        col.radius = 1.0f;

        EnemyScript esScript = boss.AddComponent<EnemyScript>();
        esScript.isBoss = true;

        if (lvl == 1)
        {
            boss.AddComponent<BossSkeletonKing>();
        }
        else
        {
            boss.AddComponent<BossVoidColossus>();
        }

        boss.SetActive(false);
        return boss;
    }

    private void SetupChests()
    {
        if (Object.FindAnyObjectByType<ChestInteractable>() != null) return;

        // Vị trí các rương: 2 rương đặt ngay tầm mắt người chơi ở đầu game!
        Vector2[] chestLocations = new Vector2[]
        {
            new Vector2(-3.5f, 1.5f),  // Rương thường ngay bên trái
            new Vector2(3.5f, 2.0f),   // Rương vàng ngay bên phải
            new Vector2(-7.0f, -4.0f), // Rương góc dưới trái
            new Vector2(8.0f, -4.5f),  // Rương góc dưới phải
            new Vector2(0f, 5.5f)      // Rương phía trên
        };

        for (int i = 0; i < Mathf.Min(chestCount, chestLocations.Length); i++)
        {
            GameObject chestObj = new GameObject($"TreasureChest_{i + 1}");
            chestObj.transform.position = chestLocations[i];

            SpriteRenderer sr = chestObj.AddComponent<SpriteRenderer>();
            Texture2D chestTex = CreateChestTexture(i == 1 || i == 3); // 2 rương hiếm
            sr.sprite = Sprite.Create(chestTex, new Rect(0, 0, 32, 24), new Vector2(0.5f, 0.5f), 16);
            sr.sortingOrder = 9; // Luôn hiển thị trên mặt đất
            chestObj.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

            // Đèn phát sáng rực rỡ quanh rương
            Light2D cLight = chestObj.AddComponent<Light2D>();
            cLight.lightType = Light2D.LightType.Point;
            cLight.pointLightOuterRadius = 3.5f;
            cLight.pointLightInnerRadius = 0.6f;
            cLight.color = (i == 1 || i == 3) ? new Color(1f, 0.85f, 0.1f) : new Color(0.4f, 0.8f, 1f);
            cLight.intensity = 1.5f;

            GameObject textObj = new GameObject("PromptText");
            textObj.transform.SetParent(chestObj.transform, false);
            textObj.transform.localPosition = new Vector3(0, 1.3f, 0);
            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.fontSize = 4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.yellow;
            tmp.fontStyle = FontStyles.Bold;
            tmp.text = (i == 1 || i == 3) ? "[E] Mở Rương Vàng" : "[E] Mở Rương";
            tmp.sortingOrder = 15;

            ChestInteractable ci = chestObj.AddComponent<ChestInteractable>();
            ci.isRareChest = (i == 1 || i == 3);

            var pField = typeof(ChestInteractable).GetField("promptText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pField != null) pField.SetValue(ci, tmp);
            var srField = typeof(ChestInteractable).GetField("spriteRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (srField != null) srField.SetValue(ci, sr);
        }
    }

    private void SetupTorchesAndParticles()
    {
        Vector2[] torchPositions = new Vector2[]
        {
            new Vector2(-15f, 10f),
            new Vector2(15f, 10f),
            new Vector2(-15f, -10f),
            new Vector2(15f, -10f),
            new Vector2(0f, 12f),
            new Vector2(0f, -12f),
            new Vector2(-8f, 0f),
            new Vector2(12f, 0f)
        };

        foreach (var pos in torchPositions)
        {
            GameObject torchObj = new GameObject("DungeonTorch");
            torchObj.transform.position = pos;

            Light2D light = torchObj.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.pointLightOuterRadius = 5.5f;
            light.pointLightInnerRadius = 1.0f;
            light.color = (levelIndex == 1) ? new Color(1f, 0.65f, 0.25f) : new Color(0.3f, 0.9f, 0.7f);
            light.intensity = 1.6f;

            torchObj.AddComponent<TorchFlicker>();
        }
    }

    private Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    float alpha = 1f - (dist / radius);
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return tex;
    }

    private Texture2D CreateChestTexture(bool isRare)
    {
        Texture2D tex = new Texture2D(32, 24, TextureFormat.RGBA32, false);
        Color baseCol = isRare ? new Color(0.95f, 0.75f, 0.15f) : new Color(0.65f, 0.40f, 0.20f);
        Color trimCol = isRare ? Color.cyan : Color.yellow;

        for (int y = 0; y < 24; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                if (x <= 1 || x >= 30 || y <= 1 || y >= 22 || y == 12)
                    tex.SetPixel(x, y, trimCol);
                else
                    tex.SetPixel(x, y, baseCol);
            }
        }
        tex.Apply();
        return tex;
    }
}
