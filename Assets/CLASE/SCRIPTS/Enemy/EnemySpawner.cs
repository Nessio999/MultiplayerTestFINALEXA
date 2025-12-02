using UnityEngine;
using Fusion;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Enemy Spawner Settings")]
    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3f;

    private float timer = 0f;

    private void FixedUpdate()
    {
        if (Runner.IsServer)
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnEnemy();
                timer = 0f;
            }
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        Runner.Spawn(enemyPrefab, spawnPoint.position + new Vector3(0, 1, 0), spawnPoint.rotation);

        Debug.Log("Enemy spawned at: " + spawnPoint.position);
    }
}
