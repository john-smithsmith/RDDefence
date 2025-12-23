using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [Header("Settings")]
    public DiceType type; 

    [Header("UI Components")]
    public TextMeshProUGUI levelText; 
    public TextMeshProUGUI costText;  
    public Button myButton;          

    void Start()
    {
        myButton.onClick.AddListener(() => DiceUpgradeManager.Instance.TryUpgrade(type));
        DiceUpgradeManager.Instance.OnUpgradeSuccess += UpdateUI;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (DiceUpgradeManager.Instance == null) return;
        int lv = DiceUpgradeManager.Instance.GetLevel(type);
        levelText.text = $"Lv.{lv}";
        int cost = DiceUpgradeManager.Instance.GetUpgradeCost(type);
        costText.text = $"{cost} SP";
    }

    void OnDestroy()
    {
        if (DiceUpgradeManager.Instance != null)
        {
            DiceUpgradeManager.Instance.OnUpgradeSuccess -= UpdateUI;
        }
    }
}