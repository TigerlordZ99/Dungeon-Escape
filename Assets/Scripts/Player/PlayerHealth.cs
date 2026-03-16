using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        Debug.Log("Player health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player is dead!");
            ShowGameOverScreen();
        }
    }

    void ShowGameOverScreen()
    {
        Time.timeScale = 0f;

        // Canvas
        GameObject canvasObj = new GameObject("GameOverCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Dark panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // You Died text
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

        // Play Again button
        GameObject yesBtn = new GameObject("PlayAgainButton");
        yesBtn.transform.SetParent(panel.transform, false);
        Image yesBtnImage = yesBtn.AddComponent<Image>();
        yesBtnImage.color = new Color(0.2f, 0.8f, 0.2f);
        Button yesButton = yesBtn.AddComponent<Button>();
        RectTransform yesBtnRect = yesBtn.GetComponent<RectTransform>();
        yesBtnRect.anchorMin = new Vector2(0.25f, 0.35f);
        yesBtnRect.anchorMax = new Vector2(0.45f, 0.5f);
        yesBtnRect.offsetMin = Vector2.zero;
        yesBtnRect.offsetMax = Vector2.zero;

        GameObject yesText = new GameObject("YesText");
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

        yesButton.onClick.AddListener(() => {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        // Quit button
        GameObject noBtn = new GameObject("QuitButton");
        noBtn.transform.SetParent(panel.transform, false);
        Image noBtnImage = noBtn.AddComponent<Image>();
        noBtnImage.color = new Color(0.8f, 0.2f, 0.2f);
        Button noButton = noBtn.AddComponent<Button>();
        RectTransform noBtnRect = noBtn.GetComponent<RectTransform>();
        noBtnRect.anchorMin = new Vector2(0.55f, 0.35f);
        noBtnRect.anchorMax = new Vector2(0.75f, 0.5f);
        noBtnRect.offsetMin = Vector2.zero;
        noBtnRect.offsetMax = Vector2.zero;

        GameObject noText = new GameObject("NoText");
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

        noButton.onClick.AddListener(() => {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        });
    }
}