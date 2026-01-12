using UnityEngine;
using System.Collections.Generic;

public class RoomRandomizer : MonoBehaviour
{
    private List<Transform> spawnPoints;

    [Header("What can spawn?")]
    public List<GameObject> potentialProps;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float spawnChance = 0.5f;

    void Start()
    {
        spawnPoints = new List<Transform>();

        Transform container = transform.Find("Spawnpoints");

        if (container != null)
        {
            foreach (Transform child in container)
            {
                spawnPoints.Add(child);
            }
        }
        else
        {
            Debug.LogError($"Could not find 'Spawnpoints' object in Room {name}!");
        }

        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            if (Random.value < spawnChance)
            {
                if (potentialProps != null && potentialProps.Count > 0)
                {
                    GameObject prefabToSpawn = potentialProps[Random.Range(0, potentialProps.Count)];

                    Instantiate(prefabToSpawn, point.position, point.rotation, transform);
                }
            }
        }
    }
}