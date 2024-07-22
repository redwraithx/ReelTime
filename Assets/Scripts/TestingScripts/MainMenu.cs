using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // add the scene to the build scene queue index
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        PrintDebug();

        // this wont work in editor
        Application.Quit();
    }




    // DELETE THIS AND ALL CALLS TO IT BEFORE RELEASE, TO KEEP GARBAGE COLLECTION DOWN
    private void PrintDebug(string msgText = "Quiting")
    {
        Debug.Log(msgText);
    }
}
