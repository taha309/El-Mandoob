using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player;
    private Vector3 targetPosition;

    [SerializeField] private float minX, maxX, minY, maxY;
    [SerializeField] private float followSharpness = 12f;

    void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("El Mandoob: CameraFollow could not find an object tagged Player.");
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        targetPosition = transform.position;
        targetPosition.x = player.position.x;
        targetPosition.y = player.position.y;

        // Respect scene bounds only when they were actually configured in the Inspector.
        if (maxX > minX)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        }
        if (maxY > minY)
        {
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

        float t = 1f - Mathf.Exp(-Mathf.Max(0.1f, followSharpness) * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPosition, t);
    }
}
