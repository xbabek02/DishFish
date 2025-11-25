using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Clock : MonoBehaviour
{
    public TMP_Text timeText;

    private int hours = 10;
    private int minutes = 0;
    private float timer;

    public FirstPersonController fpsController;

    public GameObject WinScreen;
    public GameObject LoseScreen;
    public GameObject UIScreen;
    
    public int moneyGoal;
    
    public TMP_Text highScoreText;
    public int highScore = 0;
    
    void Start()
    {
        moneyGoal = DifficultyManager.Instance.moneyLimit;
    }
    
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 2.6f)
        {
            timer = 0f;
            AddMinutes(5);  
            DisplayTime();
        }

        if (hours >= 20)
        {
            EndGame();
        }
    }

    void AddMinutes(int amount)
    {
        minutes += amount;

        if (minutes >= 60)
        {
            minutes -= 60;
            hours++;

            if (hours >= 24)
                hours = 0;
        }
    }

    public void DisplayTime()
    {
        timeText.text = $"{hours:00}:{minutes:00}";
    }

    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore-"+(int)DifficultyManager.Instance.difficulty, 0);
    }
    
    public void EndGame()
    {
        var player = fpsController.gameObject.GetComponent<Player>();
        
        LoadHighScore();
        SaveHighScore(player.Money);
        highScoreText.text = "Highscore: " + highScore.ToString();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        fpsController.enabled = false;
        UIScreen.SetActive(false);
        
        if (player.Money < moneyGoal)
        {
            Lose();
        }

        if (player.Money >= moneyGoal)
        {
            Win();
        }
    }

    private void Win()
    {
       WinScreen.SetActive(true);
    }

    private void Lose()
    {
        LoseScreen.SetActive(true);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void SaveHighScore(int score)
    {
        // Check if new score is higher
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore-"+(int)DifficultyManager.Instance.difficulty, highScore);
            PlayerPrefs.Save(); 
            LoadHighScore();
        }
    }

}