using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] private int maxEnemies = 5;

    private List<Vector3> spawnPoints = new List<Vector3>();
    private int currentEnemyCount = 0;
    private bool subscribed = false;

    // No fixed thresholds — 1 enemy at key 1, then +1 every 2 keys after that

    void Update()
    {
        // Keep trying to subscribe until PlayerInventory exists
        if (!subscribed && PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnKeyCountChanged += OnKeyCountChanged;
            subscribed = true;
            Debug.Log("EnemySpawner: Subscribed to PlayerInventory.");
        }
    }

    void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnKeyCountChanged -= OnKeyCountChanged;
    }

    public void RegisterSpawnPoints(List<Vector3> points)
    {
        spawnPoints = points;
        Debug.Log($"EnemySpawner: {spawnPoints.Count} spawn points registered.");
    }

    private void OnKeyCountChanged(int totalKeys)
    {
        Debug.Log($"EnemySpawner: Key count = {totalKeys}");

        // 1 enemy after key 1, +1 every 2 keys after: 1→1, 3→2, 5→3, 7→4, 9→5...
        int targetCount = totalKeys >= 1 ? 1 + (totalKeys - 1) / 2 : 0;
        targetCount = Mathf.Min(targetCount, maxEnemies);

        while (currentEnemyCount < targetCount)
            SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) { Debug.LogWarning("EnemySpawner: No enemyPrefab!"); return; }
        if (spawnPoints.Count == 0) { Debug.LogWarning("EnemySpawner: No spawn points!"); return; }

        Vector3 playerPos = PlayerInventory.Instance != null
            ? PlayerInventory.Instance.transform.position
            : Vector3.zero;

        spawnPoints.Sort((a, b) =>
            Vector3.Distance(b, playerPos).CompareTo(Vector3.Distance(a, playerPos)));

        int pickRange = Mathf.Min(3, spawnPoints.Count);
        Vector3 spawnPos = spawnPoints[Random.Range(0, pickRange)];

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        currentEnemyCount++;
        Debug.Log($"Enemy spawned! Total: {currentEnemyCount}");
    }
}