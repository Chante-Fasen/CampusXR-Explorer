using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNav:MonoBehaviour
{
    public void goToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
