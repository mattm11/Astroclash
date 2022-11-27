using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class titleScriptsTest : MonoBehaviour
{
    public TMP_InputField input;

    public void quit()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }

    public void start()
    {
        Debug.Log("Starting the game...");

        if (input.text == "")
            playerController.playerName = "Anonymous";
        else
            playerController.playerName = input.text;
        SceneManager.LoadScene("Network Sandbox");
    }
}
