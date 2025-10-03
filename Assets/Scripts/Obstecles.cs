using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstecles : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Kill(collision);
    }

    private void Kill(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        var player = collision.GetComponent<PlayerController>();
        
        player.TakeHit((Vector2)transform.position, 1);
    }
}
