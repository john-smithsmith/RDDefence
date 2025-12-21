using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class UpgradeStat
{
    public DiceType type;
    public int baseDamage = 10;      // 1레벨 기본 데미지
    public int damagePerLevel = 5;   // 레벨업당 오르는 데미지
    public int startCost = 100;      // 첫 강화 비용
    public int costIncrease = 100;   // 강화할 때마다 늘어나는 비용
}

public class DiceUpgradeManager : MonoBehaviour
{
    public static DiceUpgradeManager Instance;
    

    [Header("Settings")]
    public List<UpgradeStat> upgradeStats = new List<UpgradeStat>();

    private Dictionary<DiceType, UpgradeStat> statDict = new Dictionary<DiceType, UpgradeStat>();
    private Dictionary<DiceType, int> currentLevels = new Dictionary<DiceType, int>();

    void Awake()
    {
        Instance = this;
        LoadCSVData();
        Initialize();
    }

    void LoadCSVData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("DiceUpgradeData");

        if (csvData == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없음");
            return;
        }

        upgradeStats.Clear();

        string[] lines = csvData.text.Split(new char[] { '\n' });

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Split(',');

            UpgradeStat newStat = new UpgradeStat();

            try
            {
                newStat.type = (DiceType)Enum.Parse(typeof(DiceType), row[0].Trim());

                newStat.baseDamage = int.Parse(row[1]);
                newStat.damagePerLevel = int.Parse(row[2]);
                newStat.startCost = int.Parse(row[3]);
                newStat.costIncrease = int.Parse(row[4]);

                upgradeStats.Add(newStat);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{i}번째 줄 파싱 에러: {e.Message}");
            }
        }

        Debug.Log("CSV 데이터 로드");
    }

    void Initialize()
    {

        foreach (var stat in upgradeStats)
        {
            if (!statDict.ContainsKey(stat.type))
            {
                statDict.Add(stat.type, stat);
                currentLevels.Add(stat.type, 1);
            }
        }
    }

    public int GetTotalDamage(DiceType type)
    {
        if (!statDict.ContainsKey(type)) return 10;

        int level = currentLevels[type];
        UpgradeStat stat = statDict[type];

        return stat.baseDamage + ((level - 1) * stat.damagePerLevel);
    }

    public int GetUpgradeCost(DiceType type)
    {
        if (!statDict.ContainsKey(type)) return 0;

        int level = currentLevels[type];
        UpgradeStat stat = statDict[type];

        return stat.startCost + ((level - 1) * stat.costIncrease);
    }

    public int GetLevel(DiceType type)
    {
        if (currentLevels.ContainsKey(type)) return currentLevels[type];
        return 1;
    }

    public void TryUpgrade(int typeIndex)
    {
        DiceType type = (DiceType)typeIndex;
        TryUpgrade(type);
    }

    public void TryUpgrade(DiceType type)
    {
        if (!statDict.ContainsKey(type)) return;

        int cost = GetUpgradeCost(type);

        if (BoardManager.Instance.sp >= cost)
        {
            BoardManager.Instance.sp -= cost;

            currentLevels[type]++;

            Debug.Log($"{type} 주사위 강화! Lv.{currentLevels[type]}");

        }
        else
        {
            Debug.Log("SP가 부족");
        }
    }
}
