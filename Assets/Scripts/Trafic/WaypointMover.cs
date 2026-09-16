using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    [SerializeField] private float distanceThreshold = 0.1f;
    [SerializeField] private Waypoints waypoints;
    [SerializeField] private float moveSpeed = 5f;
    [Range(0f, 15f)]
    [SerializeField] private float rotateSpeed = 4f;

    public Transform currentWaypoint;

    private Quaternion rotationGoal;
    private Vector2 directionToWaypoint;

    public Transform MyCurrentWaypoint
    {
        get { return currentWaypoint; }
        set { currentWaypoint = value; }
    }

    public Waypoints MyWaypoint
    {
        get { return waypoints; }
        set { waypoints = value; }
    }

    void Start()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("El Mandoob: WaypointMover has no valid waypoint path.", this);
            enabled = false;
            return;
        }

        Transform spawnWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
        if (spawnWaypoint == null)
        {
            enabled = false;
            return;
        }

        transform.position = spawnWaypoint.position;
        currentWaypoint = waypoints.GetNextWaypoint(spawnWaypoint);

        if (currentWaypoint != null)
        {
            transform.up = currentWaypoint.position - transform.position;
        }
    }

    void Update()
    {
        if (currentWaypoint == null || waypoints == null)
        {
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            currentWaypoint.position,
            Mathf.Max(0f, moveSpeed) * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentWaypoint.position) < Mathf.Max(0.01f, distanceThreshold))
        {
            Transform next = waypoints.GetNextWaypoint(currentWaypoint);
            if (next == null)
            {
                enabled = false;
                return;
            }

            currentWaypoint = next;
        }

        RotateTowardsWaypoint();
    }

    private void RotateTowardsWaypoint()
    {
        if (currentWaypoint == null)
        {
            return;
        }

        directionToWaypoint = (currentWaypoint.position - transform.position).normalized;
        if (directionToWaypoint.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(directionToWaypoint.y, directionToWaypoint.x) * Mathf.Rad2Deg;
        rotationGoal = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotationGoal,
            Mathf.Max(0f, rotateSpeed) * Time.deltaTime);
    }
}
