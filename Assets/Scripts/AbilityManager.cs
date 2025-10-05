using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    //public static AbilityManager Instance { get; private set; }
    public GameManager gameManager;
    public static AbilityManager Instance { get; private set; }

    [Header("Abilities")]
    public List<AbilityData> abilities = new List<AbilityData>();
    [SerializeField] private int nextIndex = 0;

    private HashSet<AbilityType> unlocked = new HashSet<AbilityType>();
    

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!gameManager)
            gameManager = FindObjectOfType<GameManager>(true);


    }

    public void ContinueAfterDeath()
    {
        if(nextIndex < abilities.Count)
        {
            Unlock();
        }
        gameManager.Respawn();
    }

    public void HandleDeath()
    {
        int remaining = gameManager.MakeDeath();

        if(remaining <= 0)
        {
            Time.timeScale = 1f;
            gameManager.StartNewRun(this);
            return;
        }

        AbilityData abilityDataCurr = null;
        if (nextIndex < abilities.Count)
        {
            abilityDataCurr = abilities[nextIndex];
        }
        var ui = FindObjectOfType<DeathUiController>(true);
        ui.Show(abilityDataCurr,ContinueAfterDeath);
        

    }

    private void Unlock()
    {
        if (nextIndex >= abilities.Count) return;

        var data = abilities[(nextIndex)];
        unlocked.Add(data.type);



        nextIndex++;
        Debug.Log("Unlocked ability: " + data.type);

    }

    public void ResetRun()
    {
        nextIndex = 0;
        unlocked.Clear();
    }

    public bool IsUnlocked(AbilityType type)
    {
        bool resault = unlocked.Contains(type);
        Debug.Log("IsUnlocked check for " + type + " = " + resault);
        return unlocked.Contains(type);
    }



}
