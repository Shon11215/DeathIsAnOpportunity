using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{

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
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
