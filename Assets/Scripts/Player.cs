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
    [HideInInspector] public int currentLives;
    public HealthBar healthBar;
    public SpriteRenderer sprite;
    private int flickerAmount = 6;
    private float flickerDuration = 0.1f;
    public bool canBeHit = true;
    private bool hitRecoveryActive;
    private int shieldLocks;

    [SerializeField] private Button receiveButton, deliverButton;
    public bool carryingOrder = false;

    public Animator animator;
    private Vector2 movement;
    private Shop nearbyShop;
    private House nearbyHouse;
    private bool currentDeliveryHadHit;

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
        currentDeliveryHadHit = false;
        RefreshInvulnerability();
    }

    void Update()
    {
        Vector2 joystickMovement = Vector2.zero;
        if (joystick != null)
        {
            joystickMovement = Vector2.ClampMagnitude(
                new Vector2(joystick.Horizontal, joystick.Vertical),
                1f);
        }

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
        if (!collision.CompareTag("Vehicles") || !canBeHit)
        {
            return;
        }

        if (carryingOrder)
        {
            currentDeliveryHadHit = true;
        }

        currentLives = Mathf.Max(0, currentLives - 1);
        if (healthBar != null)
        {
            healthBar.SetHealth(currentLives);
        }

        nearbyShop = null;
        nearbyHouse = null;
        HideInteractionButtons();

        if (rb != null)
        {
            rb.position = initialPosition;
            rb.velocity = Vector2.zero;
        }
        else
        {
            transform.position = initialPosition;
        }

        ElMandoobHUD hud = FindObjectOfType<ElMandoobHUD>();
        if (hud != null && currentLives > 0)
        {
            hud.ShowMessage("خلي بالك من العربيات! فاضلك " + currentLives + " تحمل.", 2.5f);
        }

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Shop"))
        {
            nearbyShop = collision.gameObject.GetComponent<Shop>();
        }
        else if (collision.gameObject.CompareTag("House"))
        {
            nearbyHouse = collision.gameObject.GetComponent<House>();
        }

        RefreshInteractionButtons();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Shop"))
        {
            Shop exitingShop = collision.gameObject.GetComponent<Shop>();
            if (nearbyShop == exitingShop)
            {
                nearbyShop = null;
            }
        }
        else if (collision.gameObject.CompareTag("House"))
        {
            House exitingHouse = collision.gameObject.GetComponent<House>();
            if (nearbyHouse == exitingHouse)
            {
                nearbyHouse = null;
            }
        }

        RefreshInteractionButtons();
    }

    public void RefreshInteractionButtons()
    {
        if (receiveButton != null)
        {
            receiveButton.gameObject.SetActive(
                nearbyShop != null && nearbyShop.havingOrder && !carryingOrder);
        }

        if (deliverButton != null)
        {
            deliverButton.gameObject.SetActive(
                nearbyHouse != null && nearbyHouse.isDesination && carryingOrder);
        }
    }

    public void HideInteractionButtons()
    {
        if (receiveButton != null) receiveButton.gameObject.SetActive(false);
        if (deliverButton != null) deliverButton.gameObject.SetActive(false);
    }

    public void BeginDelivery()
    {
        currentDeliveryHadHit = false;
    }

    public bool CompleteDeliveryWasSafe()
    {
        bool wasSafe = !currentDeliveryHadHit;
        currentDeliveryHadHit = false;
        return wasSafe;
    }

    public void SetShieldActive(bool active)
    {
        if (active)
        {
            shieldLocks++;
        }
        else
        {
            shieldLocks = Mathf.Max(0, shieldLocks - 1);
        }

        RefreshInvulnerability();
    }

    private void RefreshInvulnerability()
    {
        canBeHit = !hitRecoveryActive && shieldLocks == 0;
    }

    private IEnumerator GetHitFlicker()
    {
        hitRecoveryActive = true;
        RefreshInvulnerability();

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

        hitRecoveryActive = false;
        RefreshInvulnerability();
    }
}
