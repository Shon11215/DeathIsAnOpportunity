using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        GameManager gm = FindObjectOfType<GameManager>(true);
        AbilityManager am = FindObjectOfType<AbilityManager>(true);

        gm.StartNewRun(am);
        SceneManager.LoadScene("Lvl1");

    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
