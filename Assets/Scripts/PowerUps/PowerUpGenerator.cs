using System.Collections;
using UnityEngine;

public class PowerUpGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] powerUps;
    [SerializeField] private Transform[] positions;

    private GameObject spawnedPowerUp;

    void Start()
    {
        if (!HasValidSetup())
        {
            Debug.LogWarning("El Mandoob: PowerUpGenerator is missing power-up prefabs or spawn positions.", this);
            enabled = false;
            return;
        }

        StartCoroutine(GeneratePowerUp());
    }

    private IEnumerator GeneratePowerUp()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(Random.Range(10f, 15f));

            // Keep the street readable: only one generated power-up waits at a time.
            if (spawnedPowerUp != null)
            {
                continue;
            }

            GameObject prefab = PickPowerUp();
            Transform spawnPoint = PickPosition();
            if (prefab == null || spawnPoint == null)
            {
                continue;
            }

            spawnedPowerUp = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        }
    }

    private bool HasValidSetup()
    {
        return powerUps != null && powerUps.Length > 0 &&
               positions != null && positions.Length > 0;
    }

    private GameObject PickPowerUp()
    {
        for (int attempt = 0; attempt < powerUps.Length; attempt++)
        {
            GameObject candidate = powerUps[Random.Range(0, powerUps.Length)];
            if (candidate != null)
            {
                return candidate;
            }
        }

        return null;
    }

    private Transform PickPosition()
    {
        for (int attempt = 0; attempt < positions.Length; attempt++)
        {
            Transform candidate = positions[Random.Range(0, positions.Length)];
            if (candidate != null)
            {
                return candidate;
            }
        }

        return null;
    }
}
