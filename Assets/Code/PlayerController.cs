using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {
    public float movementSpeed;
    public float gravity;
    public bool canClimb = false;
    public bool climbing = false;
    public float climbingSpeed;

    public int ammoCount = 10;
    public int projectileType = 0;

    [Tooltip("Adjust starting height of spawned projectiles.")]
    public float projectileOffset;
    Rigidbody2D rb;
    TextMeshPro ammoText;
    public GameObject grapplePrefab;
    private int health = 3;
    private bool playerHit = false;
    
    void Start() {
        SemisolidPlatform.playerObjects.Add(this.gameObject);
        rb = GetComponent<Rigidbody2D>();
        ammoText = GetComponentInChildren<TextMeshPro>();
        ammoText.text = ammoCount.ToString();
    }

    void Update() {
        if ((Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")) && ammoCount > 0) {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void FixedUpdate() {
        Vector3 movement = Walk(Input.GetAxisRaw("Horizontal"));
        if (canClimb && Input.GetAxisRaw("Vertical") != 0) {
            climbing = true;
        }
        if (climbing) {
            movement += Climb(Input.GetAxisRaw("Vertical"));
        }
        else {
            movement += new Vector3(0f, -gravity, 0f);
        }

        rb.MovePosition(transform.position + movement * Time.deltaTime);
    }

    Vector3 Walk(float walkingDirection) {
        Vector3 walk = new Vector3(walkingDirection * movementSpeed, 0f, 0f);
        return walk;
    }

    Vector3 Climb(float climbingDirection) {
        Vector3 climb = new Vector3(0, climbingDirection * climbingSpeed, 0);
        return climb;
    }

    void Attack() {
        ammoCount--;
        ammoText.text = ammoCount.ToString();
        switch (projectileType) {
            case 0:
                GameObject grappleObject = Instantiate(grapplePrefab, new Vector3(transform.position.x, transform.position.y + projectileOffset, 0f), Quaternion.identity) as GameObject;
                break;
            default:
                Debug.Log("Invalid projectile type");
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D col) {

        if (col.gameObject.tag == "Ball" && !playerHit) {
            HitPlayer();
        }
        
        if (col.gameObject.tag == "Ladder") {
            canClimb = true;
        }

    }
    
    void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.tag == "Ladder") {
            canClimb = false;
            climbing = false;
        }
    }

    void HitPlayer() {
        Debug.Log("Player hit");
        Debug.Log("Health: " + health);
        health--;

        playerHit = true;
        rb.gravityScale = 1f;
        rb.AddForce(new Vector2(50, 50));

        // Player dead:
        if (health <= 0) {
            Debug.Log("Player dead");
            GetComponentInChildren<SpriteRenderer>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }
    }
}
