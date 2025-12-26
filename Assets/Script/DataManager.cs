using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class DiceStat
{
    public DiceType type;
    
}

[System.Serializable]
public class WaveStat
{
   
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public Dictionary<DiceType, DiceStat> diceDict = new Dictionary<DiceType, DiceStat>();
    public Dictionary<int, WaveStat> waveDict = new Dictionary<int, WaveStat>();

    void Awake()
    {
        Instance = this;
        LoadAllData();
    }

    void LoadAllData()
    {
        LoadDiceData();
        LoadWaveData();
    }

    void LoadDiceData()
    {
        List<Dictionary<string, object>> data = CSVReader.Read("DiceData"); // 아래 CSVReader 참고

        foreach (var row in data)
        {
            DiceStat stat = new DiceStat();

           
        }
        Debug.Log("주사위 데이터 로드");
    }

    void LoadWaveData()
    {
        List<Dictionary<string, object>> data = CSVReader.Read("WaveData");

        foreach (var row in data)
        {

        }
        Debug.Log("웨이브 데이터 로드");
    }
}