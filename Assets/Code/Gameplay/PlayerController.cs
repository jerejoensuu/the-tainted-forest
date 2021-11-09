using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour {
    public float movementSpeed;
    [SerializeField] private bool canClimb = false;
    [SerializeField] private bool canClimbDown = false;
    private float currentLadderY = 0;
    public float climbingSpeed;

    [HideInInspector] public int ammoCount;
    public int score = 0;
    public int projectileType = 0;
    float movementX = 0;
    float movementY = 0;
    bool collisionCooldown = false;
    bool shieldActive = false;

    [Tooltip("Adjust starting height of spawned projectiles.")] public float projectileOffset;
    Rigidbody2D rb;
    BoxCollider2D bc;
    SpriteRenderer[] spriteRenderers;
    [SerializeField] private LayerMask layerMask;
    public GameObject grapplePrefab;
    private int health = 3;
    [SerializeField] public bool playerHit {get; set;}
    // prevent gravityScale from turning back too soon:
    [SerializeField] private bool hitOffGroundOffset = false;
    [SerializeField] private float invincibilityDurationSeconds;
    [SerializeField] private float invincibilityDeltaTime = 0.15f;

    GameObject hud;
    
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        hud = GameObject.Find("PlayerUI");
        hud.GetComponent<PlayerUI>().SetAmmo(ammoCount);
    }

    void Update() {
        if (!UIController.paused && health >= 1) {
            if ((Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")) && ammoCount > 0 && IsGrounded()) {
                Attack();
            }

            if (playerHit) {
                canClimb = false;
                canClimbDown = false;
            }
        }
    }

    void FixedUpdate() {
        if (!UIController.paused && health >= 1) {
            // Horizontal movement:
            if (Input.GetAxisRaw("Horizontal") != 0) {
                Walk();
            }

            // Can player climb and are they trying to climb:
            if (canClimb && Input.GetAxisRaw("Vertical") != 0) {
                // Can player climb down, is the ladder below them and are they attempting to climb down:
                if (canClimbDown && currentLadderY < transform.localPosition.y && Input.GetAxisRaw("Vertical") < 0) {
                    // Turn player into a semisolid able to go through platforms:
                    gameObject.layer = LayerMask.NameToLayer("SemisolidPlayer");
                // Is the player on the ground or is the ladder they're climbing above them:
                } else if (IsGrounded() || currentLadderY > transform.localPosition.y) {
                    // Turn player back into a solid object and disable canClimbDown:
                    canClimbDown = false;
                    gameObject.layer = LayerMask.NameToLayer("Player");
                }
                rb.velocity = new Vector2(0, 0);
                rb.gravityScale = 0;
                Climb();
            // If the player is unable to climb anymore, turn it's gravityScale back on:
            } else if (!canClimb && !playerHit || (IsGrounded() && !hitOffGroundOffset)) {
                rb.gravityScale = 1;
            }
        }

        Flip();
    }

    void Walk() {
        if (!playerHit || !hitOffGroundOffset) {
            movementX = Input.GetAxisRaw("Horizontal") * movementSpeed;
            transform.position += new Vector3(movementX, 0, 0) * Time.deltaTime;
        }
    }

    void Climb() {
        movementY = Input.GetAxisRaw("Vertical") * climbingSpeed;
        transform.position += new Vector3(0, movementY, 0) * Time.deltaTime;
    }

    void Attack() {
        
        switch (projectileType) {
            case 0:
                if (transform.parent.GetComponent<LevelManager>().CountVines() < 1) {
                    ChangeAmmoCount(-1);
                    GameObject grappleObject = Instantiate(grapplePrefab, new Vector3(transform.position.x, transform.position.y - (grapplePrefab.transform.localScale.y/2) + projectileOffset, 0f), Quaternion.identity) as GameObject;
                    grappleObject.transform.parent = transform.parent;
                }
                break;
            default:
                Debug.Log("Invalid projectile type");
                break;
        }
    }

    void ChangeAmmoCount(int amount) {
        ammoCount += amount;
        hud.GetComponent<PlayerUI>().SetAmmo(ammoCount);
    }

    void ChangeScore(int amount) {
        score += amount;
        hud.GetComponent<PlayerUI>().SetScore(score);
    }

    void OnTriggerEnter2D(Collider2D col) {

        if (col.gameObject.tag == "Ladder" && !playerHit) {
            canClimb = true;
            canClimbDown = col.gameObject.transform.localPosition.y < transform.localPosition.y;
            currentLadderY = col.gameObject.transform.localPosition.y;
        }

        // Avoid double collisions:
        if (collisionCooldown) {
            return;
        }

        if (col.gameObject.tag == "Ball" && !playerHit) {
            HitPlayer(col.gameObject.transform.localPosition.x);
        }

        // Drops:
        if (col.gameObject.layer == 11 && !playerHit) {
            HandleDrops(col.gameObject);
            Destroy(col.gameObject);
        }

        StartCoroutine(StartCollisionCooldown());

    }

    void HandleDrops(GameObject gameObject) {
        switch (gameObject.tag) {
            case "AmmoDrop":    ChangeAmmoCount(Random.Range(1, 4));
                                break;
            case "TimeFreeze":  StartCoroutine(transform.parent.GetComponent<LevelManager>().FreezeBubbles());
                                break;
            case "DamageAll":   transform.parent.GetComponent<LevelManager>().DamageAllBubbles();
                                break;
            case "Shield":      shieldActive = true;;
                                break;
        }
    }

    IEnumerator StartCollisionCooldown() {
        collisionCooldown = true;
        yield return new WaitForSeconds(0.1f);
        collisionCooldown = false;
    }
    
    void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.tag == "Ladder") {
            canClimb = false;
            canClimbDown = false;
        }
    }

    public void HitPlayer(float enemyX) {
        if (playerHit) {
            return;
        }

        if (shieldActive) {
            shieldActive = false;
            StartCoroutine(CreateIFrames());
            return;
        }
        
        health--;
        int dir = enemyX < transform.localPosition.x ? 1 : -1;
        rb.gravityScale = 0.5f;
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(2.5f * dir, 3.25f), ForceMode2D.Impulse);


        // Player dead:
        if (health <= 0) {
            Debug.Log("Player dead");
            GameObject.Find("LevelManager").GetComponent<LevelManager>().LevelLose();
            //GetComponentInChildren<SpriteRenderer>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        StartCoroutine(CreateIFrames());
    }

    bool IsGrounded() {
        float extraHeight = 0.1f;
        RaycastHit2D raycastHit = Physics2D.BoxCast(bc.bounds.center - bc.bounds.extents * 1.3f, bc.bounds.size * 0.05f, 0f, Vector2.down, extraHeight, layerMask);

        /* DEBUG:
        Color rayColor;
        if (raycastHit.collider != null) {
            rayColor = Color.green;
        } else {
            rayColor = Color.red;
        }
        
        Debug.DrawRay(bc.bounds.center + new Vector3(bc.bounds.extents.x, 0), Vector2.down * (bc.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(bc.bounds.center - new Vector3(bc.bounds.extents.x, 0), Vector2.down * (bc.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(bc.bounds.center - new Vector3(bc.bounds.extents.x, bc.bounds.extents.y + extraHeight), Vector2.right * (bc.bounds.extents.x * 2), rayColor);
        */
        

        return raycastHit.collider != null;
    }

    private IEnumerator CreateIFrames() {
        playerHit = true;
        bool flash = false;
        hitOffGroundOffset = true;

        for (float i = 0; i < invincibilityDurationSeconds; i += invincibilityDeltaTime) {
            TurnInvisible(flash);
            flash = !flash;
            yield return new WaitForSeconds(invincibilityDeltaTime);
            if (IsGrounded()) {
                hitOffGroundOffset = false;
            }
        }

        TurnInvisible(true);
        playerHit = false;
    }

    private void TurnInvisible(bool boolean) {
        foreach (SpriteRenderer child_sr in spriteRenderers) {
            child_sr.enabled = boolean;
        }
    }

    void Flip () {
        if (movementX < 0 || rb.velocity.x < 0) {
            transform.localScale = new Vector2(1, 1);
        } else if (movementX > 0 || rb.velocity.x > 0) {
            transform.localScale = new Vector2(-1, 1);
        }
    }
}
