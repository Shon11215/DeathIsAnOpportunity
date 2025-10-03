using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AbilityType
{
    dash,doubleJump,wallHold,stageCheckpoint,wallReset,secondHit,roofCrawl,sameStage,thirdHit
}

[CreateAssetMenu(fileName = "AbilityData",menuName = "Game/Ability Data")]
public class AbilityData : ScriptableObject
{
    public AbilityType type;
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;
}
