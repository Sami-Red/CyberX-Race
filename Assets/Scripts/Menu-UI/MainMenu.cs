using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("PlayerTesting");
    }

    public void GoToTesting()
    {
        SceneManager.LoadScene("singlePlayerTesting");
    }


    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit(); 
    }
}
