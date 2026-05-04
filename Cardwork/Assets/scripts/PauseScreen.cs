using UnityEngine;
using UnityEngine.InputSystem;

public class PauseScreen : MonoBehaviour
{
    [SerializeField] private GameObject backGround;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (backGround != null)
            backGround.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (backGround != null)
                    backGround.SetActive(!backGround.activeSelf);
            }
        }
    }
    
    public void ResumeGame()
    {
        if (backGround != null)
            backGround.SetActive(false);
    }

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        // Damit es im Editor auch "funktioniert"
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}