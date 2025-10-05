using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LivesUi : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI livesTxt;
    [SerializeField] UnityEngine.UI.Text livestxt;
    private void Awake()
    {
        if (!livesTxt)
            livesTxt = GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (!livestxt)
            livestxt = GetComponentInChildren<UnityEngine.UI.Text>(true);
    }
    public void UpdateLives(int currLives)
    {
        string s = "X " + Mathf.Max(currLives, 0);
        if (livesTxt) livesTxt.text  = s;
        if (livestxt) livestxt.text = s;
    }
}
