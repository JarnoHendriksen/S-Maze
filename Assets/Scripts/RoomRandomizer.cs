using UnityEngine;
using System.Collections.Generic;

public class RoomRandomizer : MonoBehaviour
{
    [Header("What can spawn?")]
    public List<GameObject> potentialProps;

    [Header("Spawn Settings")]
    public int numberOfItemsToSpawn = 3;

    // Minimum distance between objects to prevent overlapping
    public float minSpawnDistance = 1.5f;

    // Safety limit: how many times we try to find a spot before giving up 
    // (prevents freezing if the room is too full)
    private int maxSpawnAttempts = 10;

    [Header("Area Settings")]
    public Vector2 spawnAreaSize = new Vector2(11f, 4f);
    public Vector3 centerOffset = new Vector3(0f, 0f, 0f);

    void Start()
    {
        SpawnObjectsInArea();
    }

    void SpawnObjectsInArea()
    {
        if (potentialProps == null || potentialProps.Count == 0) return;

        // Keep a list of where we have already spawned things
        List<Vector3> validSpawnPositions = new List<Vector3>();

        for (int i = 0; i < numberOfItemsToSpawn; i++)
        {
            // Try multiple times to find a valid spot for this single object
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                // 1. Generate a random point
                Vector3 proposedPos = GetRandomPointInBounds();

                // 2. Check if this point is valid (far enough from others)
                if (IsPositionValid(proposedPos, validSpawnPositions))
                {
                    // 3. Spawn the object
                    SpawnSingleObject(proposedPos);

                    // 4. Remember this position so future objects avoid it
                    validSpawnPositions.Add(proposedPos);

                    // Break out of the attempt loop and move to the next object
                    break;
                }
            }
        }
    }

    Vector3 GetRandomPointInBounds()
    {
        float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float randomZ = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);

        Vector3 localPos = new Vector3(randomX, randomZ, 0f) + centerOffset;

        // Convert to world space so rotation works
        return transform.TransformPoint(localPos);
    }

    bool IsPositionValid(Vector3 candidatePos, List<Vector3> existingPositions)
    {
        foreach (Vector3 existingPos in existingPositions)
        {
            // If the distance is smaller than our minimum, it's too close!
            if (Vector3.Distance(candidatePos, existingPos) < minSpawnDistance)
            {
                return false;
            }
        }
        return true;
    }

    void SpawnSingleObject(Vector3 position)
    {
        GameObject prefabToSpawn = potentialProps[Random.Range(0, potentialProps.Count)];

        // Random Rotation (Optional: Rotate randomly around Y axis for variety)
        Quaternion randomRot = Quaternion.Euler(0, 0, Random.Range(0, 360));

        Instantiate(prefabToSpawn, position, randomRot, transform);
    }

    // --- VISUALIZATION ---
    // Changed from "OnDrawGizmosSelected" to "OnDrawGizmos" so it is ALWAYS visible
    private void OnDrawGizmos()
    {
        // 1. Check if the Area Size is 0 to prevent invisible boxes
        if (spawnAreaSize.x == 0 || spawnAreaSize.y == 0) return;

        // 2. Make it Solid Red so it's impossible to miss
        Gizmos.color = new Color(1, 0, 0, 0.5f); // Red with 50% opacity

        // 3. Calculate position
        Vector3 worldCenter = transform.TransformPoint(centerOffset);

        // 4. Handle Rotation/Scale
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(worldCenter, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;

        // 5. Draw the Cube
        Gizmos.DrawCube(Vector3.zero, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f));

        // 6. Draw a Wireframe Outline (helps if the floor color blends with the red)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f));
    }
}