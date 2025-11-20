using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DiffiultyDisplay : MonoBehaviour
{
    public TMP_Text text;
    public Button easyButton;
    public Button hardButton;

    public void Start()
    {
        DifficultyManager.Instance.difText = text;
        // Assign button listeners at runtime
        if (easyButton != null)
            easyButton.onClick.AddListener(DifficultyManager.Instance.ChooseEasy);

        if (hardButton != null)
            hardButton.onClick.AddListener(DifficultyManager.Instance.ChooseHard);
    }
}