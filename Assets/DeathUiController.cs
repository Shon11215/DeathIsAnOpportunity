using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathUiController : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [Header("Ability things")]
    [SerializeField] Image showImage;
    [SerializeField] private TMP_Text abName;
    [SerializeField] private TMP_Text desc;
    [SerializeField] private Button nextButton;

    [SerializeField] private AbilityManager am;
    public void Show(AbilityData next)
    {
        panel.SetActive(true);
        Time.timeScale = 0f;

        if (next != null)
        {
            showImage.sprite = next.icon;
            abName.text = next.abilityName;
            desc.text = next.description;
        }

        


    }
    public void Hide()
    {
        panel.SetActive(false);
    }

    public void ContinueButton()
    {
        Hide();
        Time.timeScale = 1.0f;

        if (!am) am = FindObjectOfType<AbilityManager>(true);
        am.ContinueAfterDeath();
    }

}
