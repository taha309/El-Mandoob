using UnityEngine;

public class VerticalVehicle : MonoBehaviour
{
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public string direction = "top";

    private Rigidbody2D myBody;
    private Vector2 frontPosition;

    private const float StopTimeout = 2f;
    private const float IgnoreDuration = 0.75f;
    private float currentStopTime;
    private float ignoreTimer;

    void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (myBody == null)
        {
            return;
        }

        if (ignoreTimer > 0f)
        {
            ignoreTimer -= Time.deltaTime;
        }

        float signedSpeed = direction == "bottom" ? Mathf.Abs(speed) : -Mathf.Abs(speed);
        myBody.velocity = new Vector2(myBody.velocity.x, signedSpeed);

        if (ignoreTimer > 0f)
        {
            currentStopTime = 0f;
            return;
        }

        bool blocked = HasVehicleAhead();
        if (!blocked)
        {
            currentStopTime = 0f;
            return;
        }

        myBody.velocity = new Vector2(myBody.velocity.x, 0f);
        currentStopTime += Time.deltaTime;

        // At intersections two vehicles can otherwise stare at each other forever.
        // Briefly yield the collision check, then automatically restore it.
        if (currentStopTime >= StopTimeout)
        {
            currentStopTime = 0f;
            ignoreTimer = IgnoreDuration;
        }
    }

    private bool HasVehicleAhead()
    {
        frontPosition = transform.position;
        float directionSign = direction == "bottom" ? 1f : -1f;
        frontPosition.y += 1.5f * directionSign;

        for (int i = 0; i < 2; i++)
        {
            frontPosition.y += directionSign;
            if (IsVehicleHere(frontPosition))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsVehicleHere(Vector2 position)
    {
        Collider2D intersecting = Physics2D.OverlapCircle(position, 0.08f);
        return intersecting != null &&
               intersecting.gameObject != gameObject &&
               intersecting.CompareTag("Vehicles");
    }
}
