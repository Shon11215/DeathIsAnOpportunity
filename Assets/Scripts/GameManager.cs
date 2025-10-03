using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int lives = 9;
    public int currLives;
    public int CurrDeaths { get; private set; }
    void Awake()
    {
        var others = FindObjectsOfType<GameManager>(true);
        if (others.Length > 1 && others[0] != this)
        {
            Destroy(gameObject); return;
        }
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        currLives = lives;
    }

    public void OnPlayerDeath(AbilityManager abilityManager)
    {
        abilityManager.HandleDeath();
    }

    public void MakeDeath()
    {
        CurrDeaths++;
        currLives = Mathf.Max(0, currLives-1);
    }

    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
    }

    public void StartNewRun(AbilityManager abilityManager)
    {
        CurrDeaths = 0;
        currLives = lives;
        abilityManager.ResetRun();
        Respawn();
    }

}
