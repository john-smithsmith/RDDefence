using UnityEngine;

[CreateAssetMenu(fileName = "DiceData", menuName = "Scriptable Objects/DiceData")]
public class DiceData : ScriptableObject
{
    public string diceName;
    public Sprite icon;
    public Color color;
    public float attackSpeed;
    public int damage;
    public string specialEffect;
}