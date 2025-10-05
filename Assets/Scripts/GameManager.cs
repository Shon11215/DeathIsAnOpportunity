using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int lives = 9;
    [SerializeField] private AbilityManager abilityManager;
    [SerializeField] private LivesUi livesUi;
    public int currLives;
    public int CurrDeaths { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);


        SceneManager.sceneLoaded += OnSceneLoaded;

        if (!livesUi) livesUi = FindObjectOfType<LivesUi>(true);
        if (!abilityManager) abilityManager = FindObjectOfType<AbilityManager>(true);

        //var others = FindObjectsOfType<GameManager>(true);
        //if (others.Length > 1 && others[0] != this)
        //{
        //    Destroy(gameObject); return;
        //}
        //if (!livesUi)
        //    livesUi = FindObjectOfType<LivesUi>(true);

        currLives = lives;

    }
    void Start()
    {
        if (!abilityManager)
            abilityManager = FindObjectOfType<AbilityManager>(true);

        livesUi.UpdateLives(currLives);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        livesUi = FindObjectOfType<LivesUi>(true);
        if (livesUi)
        {
            livesUi.UpdateLives(currLives);
        }
    }
    public void OnPlayerDeath(AbilityManager abilityManager)
    {
        abilityManager.HandleDeath();
    }

    public int MakeDeath()
    {
        CurrDeaths++;
        currLives = Mathf.Max(0, currLives - 1);

        if (livesUi)
            livesUi.UpdateLives(currLives);

        return currLives;
        
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
