using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    [SerializeField] private GameObject deathMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject victoryMenu;

    [SerializeField] private GameObject deathObj;
    private TextMeshProUGUI deathText;
    [SerializeField] private GameObject scoreObj;
    private TextMeshProUGUI scoreText;

    [SerializeField] private PlayerStatsScript playerStatsScript;
    public Color deathColor;
    public static bool isPaused = false;

    // Lưu trữ túi đồ và chỉ số qua các Scene
    public static List<SavedItemEntry> savedInventory = new List<SavedItemEntry>();

    private void Awake()
    {
        if (GetComponent<DungeonLevelSetup>() == null)
        {
            gameObject.AddComponent<DungeonLevelSetup>();
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        if (victoryMenu != null) victoryMenu.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playerStatsScript != null && playerStatsScript.playerHealth > 0)
            {
                if (isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        if (scoreObj != null) scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        if (deathObj != null) deathText = deathObj.GetComponent<TextMeshProUGUI>();
        if (deathMenu != null) deathMenu.SetActive(true);
    }

    public void ShowVictory()
    {
        Time.timeScale = 0f;
        if (victoryMenu != null)
        {
            victoryMenu.SetActive(true);
        }
        else if (deathMenu != null)
        {
            deathMenu.SetActive(true);
            if (deathObj != null)
            {
                TextMeshProUGUI txt = deathObj.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = "<color=#00FF88>VICTORY!</color>\nBẠN ĐÃ THOÁT KHỎI HẦM NGỤC!";
                }
            }
        }
    }

    public void RestartGame()
    {
        savedInventory.Clear();
        Time.timeScale = 1f;
        SceneManager.LoadScene(1); // Load lại màn 1
        if (deathMenu != null) deathMenu.SetActive(false);
    }

    public void Score(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score\n" + score;
        }
    }

    public void MainMenu()
    {
        savedInventory.Clear();
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Main menu
    }

    public void Resume()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Pause()
    {
        if (pauseMenu != null) pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }
}
