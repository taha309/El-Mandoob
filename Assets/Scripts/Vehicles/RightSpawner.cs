using System.Collections;
using UnityEngine;

public class RightSpawner : VehicleSpawner
{
    [SerializeField] private GameObject[] vehiclesReference;
    [SerializeField] private Transform pos;

    void Start()
    {
        if (pos == null || vehiclesReference == null || vehiclesReference.Length == 0)
        {
            Debug.LogWarning("El Mandoob: RightSpawner is missing traffic references.", this);
            enabled = false;
            return;
        }

        StartCoroutine(SpawnVehicles());
    }

    private IEnumerator SpawnVehicles()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(Random.Range(7f, 10f));

            int batchSize = Mathf.Clamp(carsPerSpawn, 1, 3);
            for (int i = 0; i < batchSize; i++)
            {
                GameObject prefab = PickVehicle();
                if (prefab == null)
                {
                    continue;
                }

                GameObject spawnedVehicle = Instantiate(prefab, pos.position, Quaternion.identity);
                HorizontalVehicle vehicle = spawnedVehicle.GetComponent<HorizontalVehicle>();
                if (vehicle == null)
                {
                    Debug.LogWarning("El Mandoob: right traffic prefab has no HorizontalVehicle component.", spawnedVehicle);
                    Destroy(spawnedVehicle);
                    continue;
                }

                vehicle.direction = "right";
                vehicle.speed = -Mathf.Abs(carSpeed);

                yield return new WaitForSeconds(2f);
            }
        }
    }

    private GameObject PickVehicle()
    {
        for (int attempt = 0; attempt < vehiclesReference.Length; attempt++)
        {
            GameObject candidate = vehiclesReference[Random.Range(0, vehiclesReference.Length)];
            if (candidate != null)
            {
                return candidate;
            }
        }
        return null;
    }
}
