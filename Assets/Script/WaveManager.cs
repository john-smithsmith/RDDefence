using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    [Header("Settings")]
    public Transform[] waypoints;
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public float timeBetweenWaves = 5f; // 웨이브 휴식 시간
    public float spawnInterval = 1f;    // 적 간격
    private int currentWave = 1;
    private int maxWave = 10;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(StartWaveLoop());
    }

    IEnumerator StartWaveLoop()
    {
        while (currentWave <= maxWave)
        {
            Debug.Log("Wave " + currentWave + " 시작");
            int enemyCount = 5 + (currentWave * 2); // 웨이브당 적 숫자 증가
            float hpBuff = 1 + (currentWave * 0.2f); // 체력 증가 배율
            if (currentWave == maxWave)
            {
                SpawnEnemy(bossPrefab, hpBuff * 10);
            }
            else
            {
                for (int i = 0; i < enemyCount; i++)
                {
                    SpawnEnemy(enemyPrefab, hpBuff);
                    yield return new WaitForSeconds(spawnInterval);
                }
            }         
            yield return new WaitForSeconds(timeBetweenWaves);
            currentWave++;
        }

        Debug.Log("모든 웨이브 클리어!");
    }

    void SpawnEnemy(GameObject prefab, float hpBuff)
    {
        GameObject enemyObj = PoolManager.Instance.Spawn(prefab, waypoints[0].position, Quaternion.identity);
        Enemy enemyScript = enemyObj.GetComponent<Enemy>();
        enemyScript.Init(waypoints, hpBuff);
    }
}