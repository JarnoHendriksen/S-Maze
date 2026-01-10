using UnityEngine;
using System.Collections.Generic;

public class RoomRandomizer : MonoBehaviour
{
    // Make this private so you don't accidentally fill it in the Inspector again!
    private List<Transform> spawnPoints;

    [Header("What can spawn?")]
    public List<GameObject> potentialProps;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float spawnChance = 0.5f;

    void Start()
    {
        spawnPoints = new List<Transform>();

        // FIXED: Changed spelling to "Spawnpoints" (lowercase 'p') to match your image
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

                    // FIXED: This spawns the prop as a Child of the room immediately.
                    // It uses the LIVE position of the spawn point.
                    Instantiate(prefabToSpawn, point.position, point.rotation, transform);
                }
            }
        }
    }
}