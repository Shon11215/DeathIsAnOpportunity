using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    [SerializeField] private string overrideScene = "";
    [SerializeField] private float loadDelay = 0.15f;

    [SerializeField] private float howHigh = 0.3f;
    [SerializeField] private float speed = 1f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float newY = startPos.y + MathF.Sin(Time.time * speed) * howHigh;
        transform.position = new Vector3(startPos.x , newY,startPos.z);
    }


    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (!col) col = gameObject.AddComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        StartCoroutine(LoadNext());
    }

    IEnumerator LoadNext()
    {
        Time.timeScale = 1f;
        if(SceneManager.GetActiveScene().buildIndex == 4)
        {
            var gm = GameManager.Instance;
            var ui = FindObjectOfType<LivesUi>(true);

            int shown = gm.currLives;
            float elapsed = 0f;
            float step = 0.02f;

            while (elapsed<loadDelay)
            {
                elapsed +=step;
                shown += 3;
                ui.UpdateLives(shown);
                yield return new WaitForSeconds(step);

            }



        }
        else
        {
            yield return new WaitForSeconds(loadDelay);

        }


        if (!string.IsNullOrEmpty(overrideScene))
        {
            SceneManager.LoadScene(overrideScene);
            yield break;
        }
        int next = SceneManager.GetActiveScene().buildIndex +1;
        SceneManager.LoadScene(next);


    }
}
