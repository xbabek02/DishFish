using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;
    public FirstPersonController player;
    public string titleSceneName;
    public GameObject uiCanvas;
    
    public bool canBePaused = true;

    void Awake()
    {
        Resume();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        player.enabled = true;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        uiCanvas.SetActive(true);
    }

    void Pause()
    {
        if (canBePaused)
        {
            player.enabled = false;
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
            Cursor.lockState = CursorLockMode.None; // Unlocks cursor
            Cursor.visible = true;
            uiCanvas.SetActive(false);
        }
    }

    public void QuitGame()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}