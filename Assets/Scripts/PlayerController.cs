using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using JetBrains.Annotations;
using System;

public class PlayerController : MonoBehaviour
{
    public LayerMask groundLayer;

    public float speed = 1.0f;
    public float slideSpeed = 1.0f;
    private int facing = 1;

    [Header("Dashing")]
    public GameObject dashGhost;
    public Sprite ghostSprite;
    public float ghostDistance = 0.02f;
    public float ghostLife = 0.18f;
    public float ghostAlpha = 0.35f;

    private bool hasDashed = false;
    private bool isDashing = false;
    private Vector2 lastDashDir;
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashEndDrag = 8f;
    //[SerializeField] private float dragRecovery = 0.1f;



    [Header("Wall Jump")]
    public float wallJumpUp = 12f;
    public float wallJumpAway = 10f;
    public float wallJumpLock = 0.15f;
    public float wallJumpLerp = 12f;

    public int wallSlide;

    [Header("jump")]
    public int jumpCounter = 1;
    public float lowJump = 2f;
    public float fallMult = 2.5f;
    public float jumpVelocity = 1f;
    public float collisionRadius = 0.15f;
    public ParticleSystem landingDust;
    public ParticleSystem moveDust;

    [Header("DeathEffect")]
    public float deathFloat = 0.8f;
    public float deathFadeTime = 0.6f;
    [SerializeField] Sprite deathSprite;



    [Header("Offsets")]
    public Vector2 rightOffset;
    public Vector2 leftOffset;
    public Vector2 bottomOffset = new Vector2(0f, -0.6f);
    public Vector2 topOffset;

    [Header("Hit")]
    [SerializeField] private float knockBackForce = 12f;
    [SerializeField] private float knockUpForce = 3f;
    [SerializeField] private float invulnTime = 1.5f;
    [SerializeField] private int hitsBuffer;
    private bool invuln;
    private Coroutine invulnCo;
    [SerializeField] private SpriteRenderer sr;
    private GameManager gameManager;

    [Header("Debug")]
    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public bool onRoof;

    private bool isWallGrabing;
    private float baseGravity = 1f;

    private bool wallJumpLocked;
    private Vector2 wallJumpTargetVel;
    private float wallJumpTimer;
    private bool wasGrounded;
    Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] private AbilityManager abilityManager;
    private bool isDead;
    private bool isRoofCrawling;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseGravity = rb.gravityScale;
        abilityManager = AbilityManager.Instance;
        if (!sr) sr = GetComponent<SpriteRenderer>();

    }
    void Start()
    {
        if (abilityManager == null)
            abilityManager = AbilityManager.Instance;
        gameManager = FindAnyObjectByType<GameManager>();
        hitsBuffer = GetHitPerLife();
    }

    void Update()
    {
        if (isDead) return;
        CheckEnviroment();
        WallSlideBrain();
        WallSlideAnimator();

        Vector2 raw = RawChecks();

        TryDash(raw);


        CheckWallGrab(Input.GetButton("Fire2"));
        CheckRoofGrab(Input.GetButton("Fire2"));

        if (onWall && !onGround && !isWallGrabing)
        {
            WallSlide();
        }


        if (Input.GetButtonDown("Jump"))
        {


            if (onWall && !onGround && wallSlide!=0)
            {
                int dir = -wallSlide;
                rb.velocity *= Vector2.zero;
                wallJumpTargetVel = new Vector2(dir * wallJumpAway, wallJumpUp);

                wallJumpLocked = true;
                wallJumpTimer = wallJumpLock;

                return;


            }

            if (onGround || jumpCounter > 0)
            {
                CreateDust();
                rb.velocity = Vector2.up * jumpVelocity;
                jumpCounter--;
            }
        }
        if (!isDashing)
        {
            if (rb.velocity.y <0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMult - 1) * Time.deltaTime;
            }
            else if (rb.velocity.y >0 && !Input.GetButton("Jump"))
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJump - 1) * Time.deltaTime;
            }
        }

        if (!wasGrounded && onGround)
        {
            Vector3 dustSpawnPos = (Vector2)transform.position +bottomOffset;
            ParticleSystem ps = Instantiate(landingDust, dustSpawnPos, Quaternion.identity);

            ps.Play();

            if (abilityManager != null && abilityManager.IsUnlocked(AbilityType.doubleJump))
                jumpCounter = 2;
            else
                jumpCounter = 1;
        }

        wasGrounded = onGround;



    }

    private void CheckRoofGrab(bool grab)
    {
        if(isDead || isDashing)
        {
            if(isRoofCrawling) ReleaseRoofGrab();
            return;
        }
        if (!isRoofCrawling)
        {
            if(onRoof && grab && abilityManager.IsUnlocked(AbilityType.roofCrawl))
            {
                EnterRoofGrab();
            }
        }
        else
        {
            bool wantRelease = !onRoof ||!grab || Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire1");
            if (wantRelease)
            {
                ReleaseRoofGrab();
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
            }
        }
    }

    private void EnterRoofGrab()
    {
        isRoofCrawling = true;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        animator.SetBool("isRoofCrawl",true);
    }
    private void ReleaseRoofGrab()
    {
        isRoofCrawling = false;
        rb.gravityScale = baseGravity;
        animator.SetBool("isRoofCrawl", false);

    }

    private void CheckWallGrab(bool grab)
    {
        if (onGround || isDashing)
        {
            if (isWallGrabing)
                ReleaseWallGrab();
            return;
        }

        if (!isWallGrabing)
        {
            if (onWall && grab &&abilityManager.IsUnlocked(AbilityType.wallHold))
            {
                EnterWallGrab();
            }
        }
        else
        {
            bool checkIfWantRelease = !onWall || !grab || Input.GetButtonDown("Jump") ||Input.GetButtonDown("Fire1");
            if (checkIfWantRelease)
            {
                ReleaseWallGrab();
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (wallJumpLocked)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, wallJumpTargetVel, wallJumpLerp * Time.fixedDeltaTime);
            wallJumpTimer -= Time.fixedDeltaTime;
            if (wallJumpTimer < 0f) wallJumpLocked = false;
            return;
        }

        if (isDashing) return;
        if (isWallGrabing)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
        if (isRoofCrawling)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }

        Move();

    }



    private void EnterWallGrab()
    {
        isWallGrabing = true;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }
    private void ReleaseWallGrab()
    {
        isWallGrabing = false;
        rb.gravityScale = baseGravity;

    }

    private void TryDash(Vector2 raw)
    {
        if (isWallGrabing) ReleaseWallGrab();
        if(isRoofCrawling) ReleaseRoofGrab();
        if (Input.GetButtonDown("Fire1") && !hasDashed && !isDashing &&(raw.x != 0 || raw.y != 0))
        {
            Debug.Log(abilityManager);
            if (abilityManager != null && abilityManager.IsUnlocked(AbilityType.dash))
            {
                Dash(raw.x, raw.y);

            }
        }
    }

    private void WallSlideAnimator()
    {
        bool wallSlideAnim = onWall && !onGround && !isDashing && !wallJumpLocked;
        animator.SetBool("isWallSlide", wallSlideAnim);
    }

    private void WallSlideBrain()
    {
        wallSlide = onRightWall ? +1 : (onLeftWall ? -1 : 0);
        if (onGround)
            hasDashed = false;
        else if (onWall && abilityManager != null && abilityManager.IsUnlocked(AbilityType.wallReset))
        {
            hasDashed = false;
        }
    }

    private void CheckEnviroment()
    {
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, groundLayer);

        onWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer)
            || Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);

        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);
        onRoof = Physics2D.OverlapCircle((Vector2)transform.position + topOffset, collisionRadius, groundLayer);

    }

    private Vector2 RawChecks()
    {
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        return new Vector2(xRaw, yRaw);
    }

    private void Dash(float x, float y)
    {
        hasDashed = true;
        isDashing = true;

        Vector2 dir = new Vector2(x, y).normalized;

        lastDashDir = dir;

        rb.velocity = Vector2.zero;
        rb.velocity += dir * dashSpeed;

        StartCoroutine(DashRoutine());
        StartCoroutine(GhostTrail());
    }

    private IEnumerator DashRoutine()
    {
        float ogGravity = rb.gravityScale;
        float ogDrag = rb.drag;

        rb.drag = dashEndDrag;
        rb.gravityScale = 0f;

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = ogGravity;
        isDashing = false;
        rb.drag = ogDrag;

        rb.velocity = new Vector2(lastDashDir.x * speed, rb.velocity.y);

    }



    private void WallSlide()
    {
        if (rb.velocity.y < -slideSpeed)
            rb.velocity = new Vector2(rb.velocity.x, -slideSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, collisionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + topOffset, collisionRadius);
    }



    private void Move()
    {
        float x = Input.GetAxis("Horizontal");

        Vector2 v = rb.velocity;
        v.x = x * speed;
        rb.velocity = v;

        int desiredfacing = facing;
        if (x > 0.05f) desiredfacing = 1;
        else if (x < -0.05f) desiredfacing = -1;

        if (desiredfacing != facing)
        {
            facing = desiredfacing;
            transform.localScale = new Vector3(facing, 1, 1);
            if (onGround) CreateDust();
        }


        animator.SetBool("isRunning", x != 0f);

    }

    void CreateDust()
    {
        moveDust.Play();
    }

    void SpawnGhost()
    {
        var ghost = Instantiate(dashGhost, transform.position, Quaternion.identity);
        var sr = ghost.GetComponent<SpriteRenderer>();

        sr.color = new Color(1f, 1f, 1f, ghostAlpha);
        ghost.transform.localScale = transform.localScale;

        StartCoroutine(FadeGhost(sr, ghostLife));

    }

    private IEnumerator GhostTrail()
    {
        float interval = Mathf.Max(0.01f, ghostDistance);
        float time = 0f;
        while (time < dashDuration && isDashing)
        {
            SpawnGhost();
            yield return new WaitForSeconds(interval);
            time += Time.deltaTime;
        }
    }
    private IEnumerator FadeGhost(SpriteRenderer sr, float fadeTime)
    {
        float time = 0f;
        Color start = sr.color;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(start.a, 0f, time/fadeTime);
            sr.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }
        Destroy(sr.gameObject);
    }


    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator) animator.enabled = false;

        StopAllCoroutines();
        invuln = false;
        if (!sr) sr = GetComponent<SpriteRenderer>();
        sr.enabled = true;
        var c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 1f);
        sr.sprite = deathSprite;



        isDashing = false;
        wallJumpLocked = false;


        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;

        var col = GetComponent<Collider2D>();
        col.enabled = false;

        if (isWallGrabing) ReleaseWallGrab();
        if (isRoofCrawling) ReleaseRoofGrab();

        StartCoroutine(DeathFade());


    }

    private IEnumerator DeathFade()
    {
        yield return null;

        if (!sr) sr = GetComponent<SpriteRenderer>();
        sr.enabled = true;

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up*deathFloat;

        Color colorStart = sr.color;

        float time = 0f;
        while (time<deathFadeTime)
        {
            time += Time.deltaTime;
            float p = Mathf.Clamp01(time/deathFadeTime);

            transform.position = Vector3.Lerp(start, end, p);

            sr.color = new Color(colorStart.r, colorStart.g, colorStart.b, Mathf.Lerp(colorStart.a, 0f, p));

            yield return null;
        }
        if (!abilityManager) abilityManager = FindAnyObjectByType<AbilityManager>(FindObjectsInactive.Include);

        abilityManager.HandleDeath();

    }

    private int GetHitPerLife()
    {
        if(abilityManager != null)
        {
            if (abilityManager.IsUnlocked(AbilityType.thirdHit)) return 3;

            else if (abilityManager.IsUnlocked(AbilityType.secondHit)) return 2;
            
        }
        return 1;
    }

    public void TakeHit(Vector2 hitFromWorldPosm, int damage = 1)
    {
        if (isDead || invuln) return;
        if (isWallGrabing) ReleaseWallGrab();
        wallJumpLocked = false;
        isDashing = false;

        Vector2 dir = ((Vector2)transform.position -hitFromWorldPosm).normalized;
        Vector2 impulse = new Vector2(dir.x * knockBackForce, knockUpForce);

        rb.velocity = Vector2.zero;
        rb.AddForce(impulse, ForceMode2D.Impulse);

        if (invulnCo != null) StopCoroutine(invulnCo);
        invulnCo = StartCoroutine(InvulnAbCo());

        if (hitsBuffer > 1)
        {
            hitsBuffer--;
            return;
        }

        Die();



    }

    private IEnumerator InvulnAbCo()
    {
        invuln = true;
        float timeElapsed = 0f;
        const float blink = 0.1f;
        while (timeElapsed<invulnTime)
        {
            if (sr) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blink);
            timeElapsed += blink;
        }
        sr.enabled = true;
        invuln = false;
    }
}
