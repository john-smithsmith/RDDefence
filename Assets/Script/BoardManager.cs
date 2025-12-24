using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Settings")]
    public GameObject dicePrefab; 
    public List<Slot> allSlots;   
    public Slot[,] slotGrid = new Slot[5, 3];

    [Header("Resources")]
    public int sp = 100;
    public int spawnCost = 10;

    [Header("Global FSM")]
    public TargetMode currentTargetMode = TargetMode.Closest;
    public event Action<TargetMode> OnTargetModeChanged;

    void Awake()
    {
        Instance = this;
        InitGrid();
    }

    void Start()
    {
        ChangeTargetState(TargetMode.Closest);
    }

    void InitGrid()
    {
        for (int i = 0; i < allSlots.Count; i++)
        {
            int x = i % 5;
            int y = i / 5;

            Slot slot = allSlots[i];
            slot.Init(x, y);
            slotGrid[x, y] = slot;
        }
    }

    public void OnSpawnButtonClick()
    {
        if (sp >= spawnCost)
        {
            if (SpawnRandomDice())
            {
                sp -= spawnCost;
                spawnCost += 5;
                Debug.Log("다이스 소환 남은 SP: " + sp);
            }
        }
    }

    public void CycleNextTargetMode()
    {
        int current = (int)currentTargetMode;
        int next = (current + 1) % 3;
        ChangeTargetState((TargetMode)next);
    }

    public void ChangeTargetState(TargetMode newMode)
    {
        currentTargetMode = newMode; // 상태 변경

        // 상태별 특수 로직 (나중에 확장 가능)
        switch (currentTargetMode)
        {
            case TargetMode.Closest:
                Debug.Log("상태 : 가까운 적");
                break;
            case TargetMode.Front:
                Debug.Log("상태 : 선두");
                break;
            case TargetMode.MaxHP:
                Debug.Log("상태 : 체력");
                break;
        }

        NotifyAllDice();
        OnTargetModeChanged?.Invoke(currentTargetMode);
    }

    void NotifyAllDice()
    {
        foreach (Slot slot in allSlots)
        {
            if (!slot.IsEmpty())
            {
                slot.currentDice.SetTargetMode(currentTargetMode);
            }
        }
    }

    bool SpawnRandomDice()
    {
        List<Slot> emptySlots = new List<Slot>();
        foreach (Slot slot in allSlots)
        {
            if (slot.IsEmpty()) emptySlots.Add(slot);
        }

        if (emptySlots.Count == 0) return false;
        Slot targetSlot = emptySlots[UnityEngine.Random.Range(0, emptySlots.Count)];
        GameObject newDiceObj = PoolManager.Instance.Spawn(dicePrefab, targetSlot.transform.position, Quaternion.identity);
        Dice newDice = newDiceObj.GetComponent<Dice>();
        DiceType randomType = (DiceType)UnityEngine.Random.Range(0, 5);
        newDice.Init(randomType);
        newDice.SetTargetMode(currentTargetMode);
        targetSlot.SetDice(newDice);
        return true;
    }

    public void SpawnMergedDiceRandom(int newDotCount)
    {
        List<Slot> emptySlots = new List<Slot>();
        foreach (Slot slot in allSlots)
        {
            if (slot.IsEmpty()) emptySlots.Add(slot);
        }
        if (emptySlots.Count > 0)
        {
            Slot targetSlot = emptySlots[UnityEngine.Random.Range(0, emptySlots.Count)];
            GameObject newDiceObj = PoolManager.Instance.Spawn(dicePrefab, targetSlot.transform.position, Quaternion.identity);
            Dice newDice = newDiceObj.GetComponent<Dice>();
            DiceType randomType = (DiceType)UnityEngine.Random.Range(0, 5);
            newDice.Init(randomType);
            newDice.SetDotCount(newDotCount);
            newDice.SetTargetMode(currentTargetMode);
            targetSlot.SetDice(newDice);
        }
    }

    public void AddSP(int amount)
    {
        sp += amount;
        Debug.Log("SP획득 현재 SP: " + sp);
    }
}