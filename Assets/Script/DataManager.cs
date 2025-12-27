using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class DiceStat
{
    public DiceType type;
    public int baseDamage;
    public float attackSpeed;
    public float range;
}

[System.Serializable]
public class WaveStat
{
    public int wave;
    public int enemyCount;
    public float hpMultiplier;
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public Dictionary<DiceType, DiceStat> diceDict = new Dictionary<DiceType, DiceStat>();
    public Dictionary<int, WaveStat> waveDict = new Dictionary<int, WaveStat>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadAllData();
    }

    void LoadAllData()
    {
        LoadDiceData();
        LoadWaveData();
    }

    void LoadDiceData()
    {
        List<Dictionary<string, object>> data = CSVReader.Read("DiceData");

        foreach (var row in data)
        {
            try
            {
                // 빈 데이터 건너뛰기
                if (!row.ContainsKey("Type")) continue;

                DiceStat stat = new DiceStat();

                // Enum 및 숫자 변환 (CSV 헤더 이름과 일치해야 함)
                stat.type = (DiceType)Enum.Parse(typeof(DiceType), row["Type"].ToString());

                if (row.ContainsKey("BaseDamage")) stat.baseDamage = int.Parse(row["BaseDamage"].ToString());
                if (row.ContainsKey("AttackSpeed")) stat.attackSpeed = float.Parse(row["AttackSpeed"].ToString());
                if (row.ContainsKey("Range")) stat.range = float.Parse(row["Range"].ToString());

                if (!diceDict.ContainsKey(stat.type))
                    diceDict.Add(stat.type, stat);
            }
            catch (Exception e)
            {
                Debug.LogError("DiceData 파싱 에러: " + e.Message);
            }
        }
        Debug.Log($"주사위 데이터 로드 완료: {diceDict.Count}개");
    }

    void LoadWaveData()
    {
        List<Dictionary<string, object>> data = CSVReader.Read("WaveData");

        foreach (var row in data)
        {
            try
            {
                if (!row.ContainsKey("Wave")) continue;

                WaveStat stat = new WaveStat();
                stat.wave = int.Parse(row["Wave"].ToString());

                if (row.ContainsKey("EnemyCount")) stat.enemyCount = int.Parse(row["EnemyCount"].ToString());
                if (row.ContainsKey("HpMultiplier")) stat.hpMultiplier = float.Parse(row["HpMultiplier"].ToString());

                if (!waveDict.ContainsKey(stat.wave))
                    waveDict.Add(stat.wave, stat);
            }
            catch (Exception e)
            {
                Debug.LogError("WaveData 파싱 에러: " + e.Message);
            }
        }
        Debug.Log($"웨이브 데이터 로드 완료: {waveDict.Count}개");
    }
}