using UnityEngine;

public class HorizontalVehicle : MonoBehaviour
{
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public string direction = "left";

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

        float signedSpeed = direction == "right" ? -Mathf.Abs(speed) : Mathf.Abs(speed);
        myBody.velocity = new Vector2(signedSpeed, myBody.velocity.y);

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

        myBody.velocity = new Vector2(0f, myBody.velocity.y);
        currentStopTime += Time.deltaTime;

        if (currentStopTime >= StopTimeout)
        {
            currentStopTime = 0f;
            ignoreTimer = IgnoreDuration;
        }
    }

    private bool HasVehicleAhead()
    {
        frontPosition = transform.position;
        float directionSign = direction == "right" ? -1f : 1f;
        frontPosition.x += 2f * directionSign;

        for (int i = 0; i < 2; i++)
        {
            frontPosition.x += directionSign;
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
