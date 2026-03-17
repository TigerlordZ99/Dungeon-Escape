using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] private int maxEnemies = 20;

    private List<Vector3> spawnPoints = new List<Vector3>();
    private int currentEnemyCount = 0;
    private bool subscribed = false;
    private Vector3 lastPlayerPos;
    private Vector3 playerMoveDir = Vector3.zero;
    private bool playerTracked = false;

    void Update()
    {
        if (!subscribed && PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnKeyCountChanged += OnKeyCountChanged;
            subscribed = true;
        }

        if (PlayerInventory.Instance != null)
        {
            Vector3 currentPos = PlayerInventory.Instance.transform.position;
            if (playerTracked)
            {
                Vector3 delta = currentPos - lastPlayerPos;
                if (delta.magnitude > 0.01f)
                    playerMoveDir = delta.normalized;
            }
            lastPlayerPos = currentPos;
            playerTracked = true;
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
    }

    private void OnKeyCountChanged(int totalKeys)
    {
        int targetCount;
        if (totalKeys < 5)
            targetCount = totalKeys >= 1 ? 1 + (totalKeys - 1) / 2 : 0;
        else
            targetCount = 3 + (totalKeys - 4);

        targetCount = Mathf.Min(targetCount, maxEnemies);

        while (currentEnemyCount < targetCount)
        {
            currentEnemyCount++;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;
        if (spawnPoints.Count == 0) return;

        Vector3 playerPos = PlayerInventory.Instance != null
            ? PlayerInventory.Instance.transform.position
            : Vector3.zero;

        Vector3 spawnPos;
        if (playerMoveDir != Vector3.zero)
            spawnPos = spawnPoints
                .OrderByDescending(p => Vector3.Dot((p - playerPos).normalized, playerMoveDir))
                .First();
        else
            spawnPos = spawnPoints
                .OrderByDescending(p => Vector3.Distance(p, playerPos))
                .First();

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}