using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum Difficulty { Easy, Hard }

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    public Difficulty difficulty = Difficulty.Easy;

    public float customerMinSpawnTime;
    public float customerMaxSpawnTime;
    public float maxFishBite;
    public float minFishBite;
    public float pullLimit;
    public float cookTime;
    public float overcookTime;
    public int moneyLimit;
    public float customerWaitTime;
    
    public TMP_Text difText;
    
    void Awake()
    {
        // Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        ApplyDifficulty();
        DontDestroyOnLoad(gameObject);
    }

    public void SetDifficulty(Difficulty newDifficulty)
    {
        difficulty = newDifficulty;
        PlayerPrefs.SetInt("Difficulty", (int)DifficultyManager.Instance.difficulty);
        PlayerPrefs.Save();
        
        if (PlayerPrefs.HasKey("Difficulty"))
        {
            difficulty = (Difficulty)PlayerPrefs.GetInt("Difficulty");
        }
        
        ApplyDifficulty();
    }

    void ApplyDifficulty()
    {
        if (difficulty == Difficulty.Easy)
        {
            customerMinSpawnTime = 10;
            customerMaxSpawnTime = 40;
            maxFishBite = 20;
            minFishBite = 10;
            pullLimit = 10;
            cookTime = 5;
            overcookTime = 20;
            moneyLimit = 350;
            customerWaitTime = 80;
        }
        else if (difficulty == Difficulty.Hard)
        {
            customerMinSpawnTime = 10;
            customerMaxSpawnTime = 30;
            maxFishBite = 40;
            minFishBite = 15;
            pullLimit = 5;
            cookTime = 15;
            overcookTime = 5;
            moneyLimit = 1000;
            customerWaitTime = 60;
        }
    }
    
    public void ChooseEasy()
    {
        SetDifficulty(Difficulty.Easy);
        difText.text = difficulty.ToString();
    }

    public void ChooseHard()
    {
       SetDifficulty(Difficulty.Hard);
       difText.text = difficulty.ToString();
    }
}
