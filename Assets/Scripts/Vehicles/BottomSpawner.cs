using System.Collections;
using UnityEngine;

public class BottomSpawner : VehicleSpawner
{
    [SerializeField] private GameObject[] vehiclesReference;
    [SerializeField] private Transform pos;

    void Start()
    {
        if (pos == null || vehiclesReference == null || vehiclesReference.Length == 0)
        {
            Debug.LogWarning("El Mandoob: BottomSpawner is missing traffic references.", this);
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
                VerticalVehicle vehicle = spawnedVehicle.GetComponent<VerticalVehicle>();
                if (vehicle == null)
                {
                    Debug.LogWarning("El Mandoob: bottom traffic prefab has no VerticalVehicle component.", spawnedVehicle);
                    Destroy(spawnedVehicle);
                    continue;
                }

                vehicle.direction = "bottom";
                vehicle.speed = Mathf.Abs(carSpeed);
                spawnedVehicle.transform.localScale = new Vector3(1f, -1f, 1f);

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
