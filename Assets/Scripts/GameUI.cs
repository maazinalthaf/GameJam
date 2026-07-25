using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives all HUD elements. Drag the matching UI objects in from the Canvas
/// (see SETUP.md for the exact hierarchy this expects).
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("Bars (Image, Fill Method = Horizontal)")]
    public Image waterBar;
    public Image oxygenBar;

    [Header("Text")]
    public Text timerText;
    public Text powerStatusText;

    [Header("Warnings")]
    public GameObject leakWarningBanner;
    public GameObject powerWarningBanner;

    [Header("End screen")]
    public GameObject endPanel;
    public Text endTitleText;
    public Text endMessageText;

    public void Refresh(float water, float oxygen, float elapsed, float duration, bool powerOn, bool leakActive)
    {
        if (waterBar) waterBar.fillAmount = water / 100f;
        if (oxygenBar) oxygenBar.fillAmount = oxygen / 100f;

        if (timerText)
        {
            float remaining = Mathf.Max(0f, duration - elapsed);
            int m = Mathf.FloorToInt(remaining / 60f);
            int s = Mathf.FloorToInt(remaining % 60f);
            timerText.text = $"{m:00}:{s:00}";
        }

        if (powerStatusText)
            powerStatusText.text = powerOn ? "POWER: ONLINE" : "POWER: OFFLINE - FIX PANEL";

        if (leakWarningBanner) leakWarningBanner.SetActive(leakActive);
        if (powerWarningBanner) powerWarningBanner.SetActive(!powerOn);
    }

    public void ShowEnd(bool won, string message)
    {
        if (endPanel) endPanel.SetActive(true);
        if (endTitleText) endTitleText.text = won ? "YOU SURVIVED" : "SUBMARINE LOST";
        if (endMessageText) endMessageText.text = message;
        Time.timeScale = 0f;
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        GameManager.Instance.RestartLevel();
    }
}
