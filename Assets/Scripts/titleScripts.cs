using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class titleScripts : MonoBehaviour
{
    public TMP_InputField input;

    public void quit()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }

    public void changeScene(string _sceneName)
    {
        Debug.Log("Loading Scene: " + _sceneName);

        if (input == null || input.text == "")
            playerController.playerName = "Anonymous";
        else
            playerController.playerName = input.text;
        SceneManager.LoadScene(_sceneName);
    }
}
