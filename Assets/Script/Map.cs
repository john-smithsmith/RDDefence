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

    void Awake()
    {
        Instance = this;
        InitGrid();
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

    bool SpawnRandomDice()
    {
        List<Slot> emptySlots = new List<Slot>();
        foreach (Slot slot in allSlots)
        {
            if (slot.IsEmpty()) emptySlots.Add(slot);
        }

        if (emptySlots.Count == 0) return false;
        Slot targetSlot = emptySlots[Random.Range(0, emptySlots.Count)];
        GameObject newDiceObj = PoolManager.Instance.Spawn(dicePrefab, targetSlot.transform.position, Quaternion.identity);
        Dice newDice = newDiceObj.GetComponent<Dice>();
        DiceType randomType = (DiceType)Random.Range(0, 5);
        newDice.Init(randomType);
        targetSlot.SetDice(newDice);
        return true;
    }

    public void SpawnMergedDiceAtRandom(int newDotCount)
    {
        List<Slot> emptySlots = new List<Slot>();
        foreach (Slot slot in allSlots)
        {
            if (slot.IsEmpty()) emptySlots.Add(slot);
        }
        if (emptySlots.Count > 0)
        {
            Slot targetSlot = emptySlots[Random.Range(0, emptySlots.Count)];
            GameObject newDiceObj = PoolManager.Instance.Spawn(dicePrefab, targetSlot.transform.position, Quaternion.identity);
            Dice newDice = newDiceObj.GetComponent<Dice>();
            DiceType randomType = (DiceType)Random.Range(0, 5);
            newDice.Init(randomType);
            newDice.SetDotCount(newDotCount);
            targetSlot.SetDice(newDice);
        }
    }
}