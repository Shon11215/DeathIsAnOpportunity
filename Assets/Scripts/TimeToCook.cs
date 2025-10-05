using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeToCook : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform spawn;
    [SerializeField] private GameObject[] prefabs;

    [SerializeField] private GameObject endPot;
    [SerializeField] private Sprite cooked;


    [SerializeField] private float height = 2f;
    [SerializeField] private float jumpTime = 1f;
    [SerializeField] private float gap = 0.25f;

    private bool shot = true;
    private bool isPlaying;
    private bool canPlay;
    private bool played = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canPlay = true;
        }
    }
    void Update()
    {
        if (played)
        {
            target.GetComponent<SpriteRenderer>().sprite = cooked;
            endPot.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.V) && !isPlaying && canPlay)
        {
            StartCoroutine(Cook());
            if (shot) isPlaying = true;
        }
    }

    private IEnumerator Cook()
    {
        if (!spawn) spawn = transform;

        List<GameObject> spawned = new List<GameObject>();

        foreach (var prefab in prefabs)
        {
            var obj = Instantiate(prefab, spawn.position, Quaternion.identity);
            spawned.Add(obj);
            StartCoroutine(Jump(obj.transform));
            yield return new WaitForSeconds(gap);
        }

        while (spawned.Exists(o => o!=null))
        {
            yield return null;
        }

        played = true;

    }

    private IEnumerator Jump(Transform t)
    {
        Vector3 start = t.position;
        Vector3 end = target.position;
        float time = 0f;

        while (time<jumpTime)
        {
            time += Time.deltaTime;
            float u = Mathf.Clamp01(time/jumpTime);
            float h = Mathf.Sin(u*MathF.PI)*height;
            t.position = Vector3.Lerp(start, end, u) + Vector3.up * h;
            yield return null;

        }

        Destroy(t.gameObject);
    }

}
