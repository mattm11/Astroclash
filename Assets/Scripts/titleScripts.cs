using UnityEngine;
using UnityEngine.SceneManagement;

public class titleScripts : MonoBehaviour
{
    public void quit()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }

    public void changeScene(string _sceneName)
    {
        Debug.Log("Loading Scene: " + _sceneName);
        SceneManager.LoadScene(_sceneName);
    }
}
