using UnityEngine;
using System.Collections;
using TMPro;

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
    public int CurrentWave => currentWave;
    private int maxWave = 10;

    [Header("UI Text")]
    public TextMeshProUGUI waveText; // "WAVE 1"
    public TextMeshProUGUI hpText;   // "HP: 5 / 5"

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(StartWaveLoop());
    }

    void Update()
    {
        if (WaveManager.Instance != null && waveText != null)
        {
            waveText.text = $"WAVE {WaveManager.Instance.CurrentWave}";
        }

        if (BoardManager.Instance != null && hpText != null)
        {
            int cur = BoardManager.Instance.currentHp;
            int max = BoardManager.Instance.maxHp;
            hpText.text = $"HP : {cur} / {max}";
        }
    }

    IEnumerator StartWaveLoop()
    {
        while (currentWave <= maxWave)
        {
            Debug.Log("Wave " + currentWave + " 시작");

            if (DataManager.Instance.waveDict.TryGetValue(currentWave, out WaveStat stat))
            {
                int enemyCount = stat.enemyCount;
                float hpBuff = stat.hpMultiplier;
                float interval = stat.spawnInterval; 
                int enemyID = stat.enemyID;          

                for (int i = 0; i < enemyCount; i++)
                {
                    SpawnEnemy(enemyPrefab, hpBuff, enemyID);
                    yield return new WaitForSeconds(interval);
                }
            }
            else
            {
                Debug.LogWarning(currentWave + "웨이브 데이터가 없습니다.");
            }

            yield return new WaitForSeconds(timeBetweenWaves);
            currentWave++;
        }
        Debug.Log("모든 웨이브 클리어!");
    }

    void SpawnEnemy(GameObject prefab, float hpBuff, int enemyID)
    {
        GameObject enemyObj = PoolManager.Instance.Spawn(prefab, waypoints[0].position, Quaternion.identity);
        Enemy enemyScript = enemyObj.GetComponent<Enemy>();
        enemyScript.Init(waypoints, hpBuff, enemyID);
    }
}