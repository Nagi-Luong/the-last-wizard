using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsScript : MonoBehaviour
{
    public float playerMaxHealth = 50;
    [SerializeField] private int playerMaxXP = 100;
    public int playerXP = 0;
    private int totalXP = 0;
    public int playerLvl = 0;
    public float playerHealth;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider xpBar;
    [SerializeField] private TextMeshProUGUI lvlText;
    private SpriteRenderer sprite;

    [SerializeField] private GameObject gameManager;
    private GameManagerScript gameManagerScript;
    private void Awake()
    {
        if (GetComponent<PlayerInventory>() == null)
        {
            gameObject.AddComponent<PlayerInventory>();
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        playerHealth = playerMaxHealth;
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (gameManager != null)
        {
            gameManagerScript = gameManager.GetComponent<GameManagerScript>();
        }
        else
        {
            gameManagerScript = Object.FindAnyObjectByType<GameManagerScript>();
        }

        if (healthBar != null)
        {
            healthBar.maxValue = playerMaxHealth;
            healthBar.value = playerMaxHealth; 
        }
        if (xpBar != null)
        {
            xpBar.maxValue = playerMaxXP;
            xpBar.value = playerXP;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerHealth <= 0)
        {
            Die();
        }
    }

    public void GainXP(int xp)
    {
        //set value
        totalXP += xp; //total for end of game
        playerXP += xp;
        xpBar.value = playerXP;
        if (playerXP == playerMaxXP)
        {
            playerLvl++;
            xpBar.value = 0;
            playerXP = 0;
            playerMaxXP += xp *  playerLvl; //or do 100 + xp * playerLvl
            xpBar.maxValue = playerMaxXP;
        }
        Debug.Log(playerMaxXP);
        Debug.Log(playerXP) ;
        lvlText.text = "Level: " + playerLvl;
    }

    public void Heal(float amount)
    {
        playerHealth = Mathf.Min(playerMaxHealth, playerHealth + amount);
        if (healthBar != null) healthBar.value = playerHealth;
    }

    public void UpdateMaxHealthBonus(float bonus)
    {
        float prevMax = playerMaxHealth;
        playerMaxHealth = 50f + bonus;
        if (healthBar != null) healthBar.maxValue = playerMaxHealth;
        playerHealth = Mathf.Min(playerMaxHealth, playerHealth + (playerMaxHealth - prevMax));
        if (healthBar != null) healthBar.value = playerHealth;
    }

    public void TakeDamage(int damage)
    {
        PlayerInventory inv = GetComponent<PlayerInventory>();
        if (inv != null)
        {
            inv.OnPlayerTakeDamage();
        }

        playerHealth -= damage;
        if (healthBar != null) healthBar.value = playerHealth;
        StartCoroutine(DamageEffectSequence(sprite, Color.red, .5f));

        if (playerHealth <= 0)
        {
            if (inv != null && inv.TryConsumeRevive())
            {
                playerHealth = playerMaxHealth * 0.5f;
                if (healthBar != null) healthBar.value = playerHealth;
                Debug.Log("[PlayerStats] Hồi sinh thần kỳ nhờ Rebirth Story!");
                return;
            }
            Die();
        }
    }

    //code from stackOverFlow
    IEnumerator DamageEffectSequence(SpriteRenderer sr, Color dmgColor, float duration)
    {
        if (sr == null) yield break;
        // save origin color
        Color originColor = sr.color;

        // tint the sprite with damage color
        sr.color = dmgColor;

        // lerp animation with given duration in seconds
        for (float t = 0; t < 1.0f; t += Time.deltaTime / duration)
        {
            if (sr == null) yield break;
            sr.color = Color.Lerp(dmgColor, originColor, t);

            yield return null;
        }

        // restore origin color
        if (sr != null) sr.color = originColor;
    }

    private void Die()
    {
        if (gameManagerScript != null)
        {
            gameManagerScript.GameOver(); // death
            gameManagerScript.Score(totalXP); //show total xp
        }
    }

}
