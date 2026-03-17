using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    private Image healthBarFill;
    private Text healthText;
    private RectTransform bgRect;
    private GameObject healthBarCanvas;

    // The full anchor extents — bar spans 0.25 to 0.75 horizontally
    private const float barLeft = 0.25f;
    private const float barRight = 0.75f;

    void Start()
    {
        currentHealth = maxHealth;
        CreateHealthBar();
        UpdateHealthBar();
    }

    void CreateHealthBar()
    {
        healthBarCanvas = new GameObject("HealthBarCanvas");
        Canvas canvas = healthBarCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        healthBarCanvas.AddComponent<CanvasScaler>();
        healthBarCanvas.AddComponent<GraphicRaycaster>();

        // Background — anchors will shrink inward as health drops
        GameObject bg = new GameObject("HealthBarBG");
        bg.transform.SetParent(healthBarCanvas.transform, false);
        bg.AddComponent<Image>().color = new Color(0.2f, 0f, 0f);
        bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(barLeft, 0.93f);
        bgRect.anchorMax = new Vector2(barRight, 0.98f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill — always covers the full background
        GameObject fill = new GameObject("HealthBarFill");
        fill.transform.SetParent(bg.transform, false);
        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = new Color(0.1f, 0.9f, 0.1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // HP text
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(bg.transform, false);
        healthText = textObj.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 22;
        healthText.fontStyle = FontStyle.Bold;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    void UpdateHealthBar()
    {
        if (bgRect == null) return;

        float pct = currentHealth / maxHealth;

        // Shrink inward from both sides symmetrically
        float half = (barRight - barLeft) / 2f;
        float center = (barLeft + barRight) / 2f;
        float currentHalf = half * pct;

        bgRect.anchorMin = new Vector2(center - currentHalf, 0.93f);
        bgRect.anchorMax = new Vector2(center + currentHalf, 0.98f);

        // Color shifts green → red
        if (healthBarFill != null)
            healthBarFill.color = Color.Lerp(new Color(0.9f, 0.1f, 0.1f), new Color(0.1f, 0.9f, 0.1f), pct);

        if (healthText != null)
            healthText.text = $"HP {Mathf.CeilToInt(currentHealth)}/100";
    }

    public void TakeDamage(float dmg)
    {
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        AudioManager.PlayTakeDamage();
        Debug.Log("Player health: " + currentHealth);
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            AudioManager.PlayDeath();
            ShowGameOverScreen();
        }
    }

    void ShowGameOverScreen()
    {
        // Hide the health bar before showing game over
        if (healthBarCanvas != null)
            healthBarCanvas.SetActive(false);

        Time.timeScale = 0f;

        GameObject canvasObj = new GameObject("GameOverCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);
        panel.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("GameOverText");
        textObj.transform.SetParent(panel.transform, false);
        Text gameOverText = textObj.AddComponent<Text>();
        gameOverText.text = "You Died!";
        gameOverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gameOverText.fontSize = 48;
        gameOverText.fontStyle = FontStyle.Bold;
        gameOverText.color = Color.red;
        gameOverText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.2f, 0.6f);
        textRect.anchorMax = new Vector2(0.8f, 0.8f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        GameObject yesBtn = new GameObject("PlayAgainButton");
        yesBtn.transform.SetParent(panel.transform, false);
        yesBtn.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f);
        Button yesButton = yesBtn.AddComponent<Button>();
        RectTransform yesBtnRect = yesBtn.GetComponent<RectTransform>();
        yesBtnRect.anchorMin = new Vector2(0.25f, 0.35f);
        yesBtnRect.anchorMax = new Vector2(0.45f, 0.5f);
        yesBtnRect.offsetMin = Vector2.zero;
        yesBtnRect.offsetMax = Vector2.zero;
        GameObject yesText = new GameObject("Text");
        yesText.transform.SetParent(yesBtn.transform, false);
        Text yesLabel = yesText.AddComponent<Text>();
        yesLabel.text = "Play Again";
        yesLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        yesLabel.fontSize = 28;
        yesLabel.fontStyle = FontStyle.Bold;
        yesLabel.color = Color.white;
        yesLabel.alignment = TextAnchor.MiddleCenter;
        RectTransform yesTextRect = yesText.GetComponent<RectTransform>();
        yesTextRect.anchorMin = Vector2.zero;
        yesTextRect.anchorMax = Vector2.one;
        yesTextRect.offsetMin = Vector2.zero;
        yesTextRect.offsetMax = Vector2.zero;
        yesButton.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); });

        GameObject noBtn = new GameObject("QuitButton");
        noBtn.transform.SetParent(panel.transform, false);
        noBtn.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);
        Button noButton = noBtn.AddComponent<Button>();
        RectTransform noBtnRect = noBtn.GetComponent<RectTransform>();
        noBtnRect.anchorMin = new Vector2(0.55f, 0.35f);
        noBtnRect.anchorMax = new Vector2(0.75f, 0.5f);
        noBtnRect.offsetMin = Vector2.zero;
        noBtnRect.offsetMax = Vector2.zero;
        GameObject noText = new GameObject("Text");
        noText.transform.SetParent(noBtn.transform, false);
        Text noLabel = noText.AddComponent<Text>();
        noLabel.text = "Quit";
        noLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        noLabel.fontSize = 28;
        noLabel.fontStyle = FontStyle.Bold;
        noLabel.color = Color.white;
        noLabel.alignment = TextAnchor.MiddleCenter;
        RectTransform noTextRect = noText.GetComponent<RectTransform>();
        noTextRect.anchorMin = Vector2.zero;
        noTextRect.anchorMax = Vector2.one;
        noTextRect.offsetMin = Vector2.zero;
        noTextRect.offsetMax = Vector2.zero;
        noButton.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); });
    }
}