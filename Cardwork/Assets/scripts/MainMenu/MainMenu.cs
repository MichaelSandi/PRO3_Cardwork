using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private List<CardDefinition> starterDeck;
    
    public void OnCreditsButtonClicked()
    {
        SceneManager.LoadScene("CreditsScene"); 
    }

    // Button 2
    public void OnStartButtonClicked()
    {
        RunManager.Instance.StartNewRun(starterDeck);
        SceneManager.LoadScene("MainScene");
    }
    
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Button 3
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        // Damit es im Editor auch "funktioniert"
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
