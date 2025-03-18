using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Player playerReference;
    
    private void Awake()
    {
        // Ensure the menu is hidden at startup
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    private void Update()
    {
        // Make sure we have reference to menu and player
        if (pauseMenuUI == null || playerReference == null)
            return;
            
        // Update UI based on player's pause state
        pauseMenuUI.SetActive(playerReference.paused);
    }

    // Call this from the Resume button
    public void ResumeGame()
    {
        if (playerReference != null)
        {
            playerReference.paused = false;
            Time.timeScale = 1f;
            pauseMenuUI.SetActive(false);
        }
    }

    // Call this from the Quit button
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Call this from the Options button
    public void OpenOptions()
    {
        // Implement options menu functionality
        Debug.Log("Options menu requested");
    }
}