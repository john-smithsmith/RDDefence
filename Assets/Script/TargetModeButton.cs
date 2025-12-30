using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TargetModeButton : MonoBehaviour
{
    public Button myButton;
    public TextMeshProUGUI statusText;

    void Start()
    {
        if (myButton == null) myButton = GetComponent<Button>();
        myButton.onClick.AddListener(() =>
        {
            if (BoardManager.Instance != null)
                BoardManager.Instance.CycleNextTargetMode();
        });

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.OnTargetModeChanged += UpdateUI;
            UpdateUI(BoardManager.Instance.currentTargetMode);
        }
    }

    void UpdateUI(TargetMode mode)
    {
        switch (mode)
        {
            case TargetMode.Closest:
                statusText.text = "Target: Default";
                statusText.color = Color.black;
                break;
            case TargetMode.Front:
                statusText.text = "Target: Head";
                statusText.color = Color.black;
                break;
            case TargetMode.MaxHP:
                statusText.text = "Target: MaxHp";
                statusText.color = Color.black;
                break;
        }
    }

    void OnDestroy()
    {
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.OnTargetModeChanged -= UpdateUI;
        }
    }
}