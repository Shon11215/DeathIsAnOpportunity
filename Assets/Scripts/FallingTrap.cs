using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingTrap : MonoBehaviour
{


    [SerializeField] private float dropDelay = 1f;
    [SerializeField] private float armDelay = 1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Collider2D solidCollider;
    //[SerializeField] private bool damageWhileFalling = true;
    //[SerializeField] private bool harmless = true;

    [SerializeField] private float fallGravity = 2f;
    [SerializeField] private float dustOffsetY = 0.05f;

    private bool isFalling;
    private bool armed;
    private Rigidbody2D rb;
    private Collider2D[] allCols;
    public ParticleSystem landingDust;

    private void Awake()
    {
        allCols = GetComponentsInChildren<Collider2D>(includeInactive: true);
    }
    void Start()
    {
        if (!solidCollider) solidCollider = GetComponent<Collider2D>();
        if (solidCollider) solidCollider.enabled = false;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;    
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isFalling) return;


        if (!collision.CompareTag("Player")) return;

        StartCoroutine(BeginFall());

    }

    private IEnumerator BeginFall()
    {
        isFalling = true;
        yield return new WaitForSeconds(dropDelay);

        rb.gravityScale = fallGravity;

        
        yield return new WaitForSeconds(armDelay);
        if (solidCollider) solidCollider.enabled = true;

        armed = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!armed) return;

        if(armed && isFalling && collision.collider.CompareTag("Player"))
        {
            var pc = collision.collider.GetComponent<PlayerController>();
            if (pc) pc.TakeHit(transform.position, 1);
            return;
        }



        Debug.Log(collision.collider.tag);    
        if (collision.collider.CompareTag("Ground"))
        {

            Vector3 dustSpawnPos = new Vector3(solidCollider.bounds.center.x,solidCollider.bounds.min.y +dustOffsetY,transform.position.z);
            ParticleSystem ps = Instantiate(landingDust, dustSpawnPos, Quaternion.identity);

            ps.Play();

            rb.velocity = Vector2.zero;
            rb.simulated = false;
            foreach (var c in allCols) if (c) c.enabled = false;
        }
    }


}
