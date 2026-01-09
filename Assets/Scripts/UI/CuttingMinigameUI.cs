using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CuttingMinigameUI : MonoBehaviour
{
    [Header("References")]
    public CuttingMinigame minigame;

    [Header("UI Elements")]
    public GameObject minigamePanel;
    public RectTransform timingBar;

    // Two zones - top and bottom
    public RectTransform topGreenZone;
    public RectTransform topPerfectZone;
    public RectTransform bottomGreenZone;
    public RectTransform bottomPerfectZone;

    public RectTransform indicator;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI resultText;
    public GameObject interactPrompt;

    [Header("Colors")]
    public Color greenZoneColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    public Color perfectZoneColor = new Color(1f, 0.8f, 0f, 0.7f);
    public Color indicatorColor = Color.white;

    [Header("Result Display")]
    public float resultDisplayDuration = 0.5f;

    float resultTimer;
    float barHeight;

    void Awake()
    {
        SetZoneColors();
        ShowMinigame(false);
        ShowInteractPrompt(false);
    }

    void SetZoneColors()
    {
        if (topGreenZone) SetImageColor(topGreenZone, greenZoneColor);
        if (bottomGreenZone) SetImageColor(bottomGreenZone, greenZoneColor);
        if (topPerfectZone) SetImageColor(topPerfectZone, perfectZoneColor);
        if (bottomPerfectZone) SetImageColor(bottomPerfectZone, perfectZoneColor);
        if (indicator) SetImageColor(indicator, indicatorColor);
    }

    void SetImageColor(RectTransform rect, Color color)
    {
        var img = rect.GetComponent<Image>();
        if (img) img.color = color;
    }

    void Start()
    {
        if (timingBar)
            barHeight = timingBar.rect.height;
    }

    void Update()
    {
        if (!minigame || !minigame.IsActive) return;

        UpdateIndicator();
        UpdateZones();
        UpdateCombo();

        if (resultTimer > 0)
        {
            resultTimer -= Time.deltaTime;
            if (resultTimer <= 0 && resultText)
                resultText.gameObject.SetActive(false);
        }
    }

    void UpdateIndicator()
    {
        if (!indicator || !timingBar) return;

        // Vertical movement: 0 = bottom, 1 = top
        float yPos = minigame.IndicatorPosition * barHeight - barHeight / 2f;
        indicator.anchoredPosition = new Vector2(indicator.anchoredPosition.x, yPos);
    }

    void UpdateZones()
    {
        if (!timingBar) return;

        float zoneHeight = minigame.GreenZoneWidth * barHeight;
        float perfectHeight = minigame.PerfectZoneWidth * barHeight;

        // Bottom zone
        if (bottomGreenZone)
        {
            float yPos = minigame.BottomZoneCenter * barHeight - barHeight / 2f;
            bottomGreenZone.anchoredPosition = new Vector2(0, yPos);
            bottomGreenZone.sizeDelta = new Vector2(bottomGreenZone.sizeDelta.x, zoneHeight);
        }
        if (bottomPerfectZone)
        {
            float yPos = minigame.BottomZoneCenter * barHeight - barHeight / 2f;
            bottomPerfectZone.anchoredPosition = new Vector2(0, yPos);
            bottomPerfectZone.sizeDelta = new Vector2(bottomPerfectZone.sizeDelta.x, perfectHeight);
        }

        // Top zone
        if (topGreenZone)
        {
            float yPos = minigame.TopZoneCenter * barHeight - barHeight / 2f;
            topGreenZone.anchoredPosition = new Vector2(0, yPos);
            topGreenZone.sizeDelta = new Vector2(topGreenZone.sizeDelta.x, zoneHeight);
        }
        if (topPerfectZone)
        {
            float yPos = minigame.TopZoneCenter * barHeight - barHeight / 2f;
            topPerfectZone.anchoredPosition = new Vector2(0, yPos);
            topPerfectZone.sizeDelta = new Vector2(topPerfectZone.sizeDelta.x, perfectHeight);
        }
    }

    void UpdateCombo()
    {
        if (!comboText) return;

        if (minigame.ComboCount > 0)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"x{minigame.ComboCount}";
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    public void ShowMinigame(bool show)
    {
        if (minigamePanel)
            minigamePanel.SetActive(show);

        if (!show && comboText)
            comboText.gameObject.SetActive(false);
    }

    public void ShowInteractPrompt(bool show)
    {
        if (interactPrompt)
            interactPrompt.SetActive(show);
    }

    public void ShowHitResult(CuttingMinigame.HitResult result, int combo)
    {
        if (!resultText) return;

        resultText.gameObject.SetActive(true);
        resultTimer = resultDisplayDuration;

        switch (result)
        {
            case CuttingMinigame.HitResult.Perfect:
                resultText.text = combo > 1 ? $"PERFECT! x{combo}" : "PERFECT!";
                resultText.color = perfectZoneColor;
                break;
            case CuttingMinigame.HitResult.Good:
                resultText.text = "Good";
                resultText.color = greenZoneColor;
                break;
            case CuttingMinigame.HitResult.Miss:
                resultText.text = "Miss";
                resultText.color = Color.red;
                break;
        }
    }
}
