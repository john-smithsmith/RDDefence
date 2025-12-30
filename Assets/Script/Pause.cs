using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Pause : MonoBehaviour
{
    [Header("UI Components")]
    public Button PButton;
    public TextMeshProUGUI PText;

    private bool isPaused = false;

    void Start()
    {
        if (PButton == null) PButton = GetComponent<Button>();
        PButton.onClick.AddListener(TogglePause);
        UpdateUI();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if (PText != null)
        {
            if (isPaused)
                PText.text = "Run";
            else
                PText.text = "Pause";
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}