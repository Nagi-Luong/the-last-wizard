using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    public static BossHealthBarUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject container;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI hpRatioText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (container != null) container.SetActive(false);
    }

    public void Show(string bossName, float maxHealth)
    {
        if (container != null) container.SetActive(true);
        if (bossNameText != null) bossNameText.text = bossName;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
        if (hpRatioText != null)
        {
            hpRatioText.text = $"{(int)maxHealth} / {(int)maxHealth}";
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Max(0, currentHealth);
        }
        if (hpRatioText != null)
        {
            hpRatioText.text = $"{Mathf.Max(0, (int)currentHealth)} / {(int)maxHealth}";
        }
    }

    public void Hide()
    {
        if (container != null) container.SetActive(false);
    }
}
