using UnityEngine;
using TMPro;

public class Crab : MonoBehaviour
{
    private string message = "BRING ME {0} DOLLARS BEFORE 22:00\n\nOR YOU'RE DONE";
    public TMP_Text text;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int moneyLimit = DifficultyManager.Instance.moneyLimit;
        text.text = string.Format(message, moneyLimit);
    }
    
}
