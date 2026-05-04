using UnityEngine;

public class CreditsUI : MonoBehaviour
{
    public void GoBackToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
