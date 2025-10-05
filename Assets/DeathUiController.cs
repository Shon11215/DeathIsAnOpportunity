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
    private bool isVisable;
    private Action onContinue;


    private void Update()
    {
        if(isVisable &&Input.GetKeyDown(KeyCode.V))
        {
            ContinueButton();
        }
    }
    public void Show(AbilityData next,Action onContinue)
    {
        panel.SetActive(true);
        isVisable = true;
        Time.timeScale = 0f;

        if (next != null)
        {
            this.onContinue = onContinue;
            showImage.sprite = next.icon;
            abName.text = next.abilityName;
            desc.text = next.description;
        }

        


    }
    public void Hide()
    {
        panel.SetActive(false);
        isVisable = false;

    }

    public void ContinueButton()
    {
        Hide();
        Time.timeScale = 1.0f;

        onContinue?.Invoke();
    }

}
