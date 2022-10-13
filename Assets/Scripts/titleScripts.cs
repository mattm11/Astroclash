using UnityEngine;
using UnityEngine.SceneManagement;

public class titleScripts : MonoBehaviour
{
    public void quit()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }

    public void start()
    {
        Debug.Log("Starting the game...");
        SceneManager.LoadScene(1);
    }
}
