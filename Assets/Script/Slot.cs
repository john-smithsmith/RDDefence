using UnityEngine;

public class Slot : MonoBehaviour
{
    public int x;
    public int y;
    public Dice currentDice;

    public void Init(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public void SetDice(Dice dice)
    {
        currentDice = dice;
        dice.transform.SetParent(transform);
        dice.transform.localPosition = Vector3.zero;
        dice.currentSlot = this;
    }

    public void RemoveDice()
    {
        currentDice = null;
    }

    public bool IsEmpty()
    {
        return currentDice == null;
    }
}