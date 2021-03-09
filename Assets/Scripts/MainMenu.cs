using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void playGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void quitGame()
    {
        Debug.Log("Quit game.");
        Application.Quit();
    }
}
