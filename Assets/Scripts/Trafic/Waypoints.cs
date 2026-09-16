using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [Range(0f, 2f)]
    [SerializeField] private float waypointSize = 1f;

    [Header("Path Setting")]
    [SerializeField] private bool canloop = true;
    [SerializeField] private bool isMovingForward = true;

    public int Count
    {
        get { return transform.childCount; }
    }

    private void OnDrawGizmos()
    {
        int count = transform.childCount;
        if (count == 0)
        {
            return;
        }

        foreach (Transform waypoint in transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(waypoint.position, waypointSize);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < count - 1; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }

        if (canloop && count > 1)
        {
            Gizmos.DrawLine(transform.GetChild(count - 1).position, transform.GetChild(0).position);
        }
    }

    public Transform GetNextWaypoint(Transform currentWaypoint)
    {
        int count = transform.childCount;
        if (count == 0)
        {
            return null;
        }

        if (currentWaypoint == null || currentWaypoint.parent != transform)
        {
            return transform.GetChild(0);
        }

        if (count == 1)
        {
            return transform.GetChild(0);
        }

        int currentIndex = currentWaypoint.GetSiblingIndex();
        int nextIndex = currentIndex;

        if (isMovingForward)
        {
            nextIndex++;
            if (nextIndex >= count)
            {
                nextIndex = canloop ? 0 : count - 1;
            }
        }
        else
        {
            nextIndex--;
            if (nextIndex < 0)
            {
                nextIndex = canloop ? count - 1 : 0;
            }
        }

        return transform.GetChild(nextIndex);
    }
}
