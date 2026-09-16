using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public FixedJoystick joystick;
    public Rigidbody2D rb;
    private Vector3 initialPosition;

    public int maxLives = 3;
    [HideInInspector]
    public int currentLives;
    public HealthBar healthBar;
    public SpriteRenderer sprite;
    private int flickerAmount = 6;
    private float flickerDuration = 0.1f;
    public bool canBeHit = true;

    [SerializeField]
    private Button receiveButton, deliverButton;
    public bool carryingOrder = false;

    public Animator animator;
    private Vector2 movement;

    void Start()
    {
        GameData data = SaveSystem.Load();
        moveSpeed = Mathf.Max(5f, data.speed);
        maxLives = Mathf.Clamp(2 + data.healths, 3, 5);

        currentLives = maxLives;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxLives);
        }
        initialPosition = transform.position;
    }

    void Update()
    {
        Vector2 joystickMovement = Vector2.zero;
        if (joystick != null)
        {
            joystickMovement = new Vector2(joystick.Horizontal, joystick.Vertical);
        }

        // Desktop fallback makes the project testable with WASD/arrow keys without
        // removing the original mobile joystick controls.
        Vector2 keyboardMovement = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical"));

        movement = keyboardMovement.sqrMagnitude > 0.01f
            ? keyboardMovement.normalized
            : joystickMovement;

        if (animator != null)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Vehicles") && canBeHit)
        {
            currentLives -= 1;
            if (healthBar != null)
            {
                healthBar.SetHealth(currentLives);
            }

            transform.position = initialPosition;
            StartCoroutine(GetHitFlicker());

            if (currentLives <= 0)
            {
                GamePlayManager manager = FindObjectOfType<GamePlayManager>();
                if (manager != null)
                {
                    manager.gameOver();
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Shop"))
        {
            Shop shop = collision.gameObject.GetComponent<Shop>();
            if (shop != null && shop.havingOrder && receiveButton != null)
            {
                receiveButton.gameObject.SetActive(true);
            }
        }
        else if (collision.gameObject.CompareTag("House") && carryingOrder)
        {
            House house = collision.gameObject.GetComponent<House>();
            if (house != null && house.isDesination && deliverButton != null)
            {
                deliverButton.gameObject.SetActive(true);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (deliverButton != null)
        {
            deliverButton.gameObject.SetActive(false);
        }
        if (receiveButton != null)
        {
            receiveButton.gameObject.SetActive(false);
        }
    }

    IEnumerator GetHitFlicker()
    {
        canBeHit = false;
        for (int i = 0; i < flickerAmount; i++)
        {
            if (sprite != null)
            {
                sprite.color = new Color(1f, 1f, 1f, 0.5f);
            }
            yield return new WaitForSeconds(flickerDuration);

            if (sprite != null)
            {
                sprite.color = Color.white;
            }
            yield return new WaitForSeconds(flickerDuration);
        }
        canBeHit = true;
    }
}
