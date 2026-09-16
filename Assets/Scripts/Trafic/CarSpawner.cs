using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject[] carprefabs;
    [SerializeField] private Waypoints waypoints;

    private void Start()
    {
        if (waypoints == null || waypoints.transform.childCount == 0 ||
            carprefabs == null || carprefabs.Length == 0)
        {
            Debug.LogWarning("El Mandoob: CarSpawner is missing cars or waypoints.", this);
            enabled = false;
            return;
        }

        foreach (Transform waypoint in waypoints.transform)
        {
            GameObject prefab = SelectCarPrefab();
            if (prefab == null)
            {
                continue;
            }

            GameObject car = Instantiate(prefab, transform);
            WaypointMover mover = car.GetComponent<WaypointMover>();
            if (mover == null)
            {
                Debug.LogWarning("El Mandoob: waypoint traffic prefab has no WaypointMover component.", car);
                Destroy(car);
                continue;
            }

            mover.MyWaypoint = waypoints;
            mover.MyCurrentWaypoint = waypoint;
        }
    }

    private GameObject SelectCarPrefab()
    {
        for (int attempt = 0; attempt < carprefabs.Length; attempt++)
        {
            GameObject candidate = carprefabs[Random.Range(0, carprefabs.Length)];
            if (candidate != null)
            {
                return candidate;
            }
        }

        return null;
    }
}
