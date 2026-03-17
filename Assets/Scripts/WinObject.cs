using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinObject : MonoBehaviour
{
    private GameObject winCanvas;

    void Start()
    {
        CreateWinUI();
        winCanvas.SetActive(false);
    }

    void CreateWinUI()
    {
        // Create Canvas
        winCanvas = new GameObject("WinCanvas");
        Canvas canvas = winCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        winCanvas.AddComponent<CanvasScaler>();
        winCanvas.AddComponent<GraphicRaycaster>();

        // Dark background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(winCanvas.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Win message text
        GameObject textObj = new GameObject("WinText");
        textObj.transform.SetParent(panel.transform, false);
        Text winText = textObj.AddComponent<Text>();
        winText.text = "You Escaped the Dungeon!";
        winText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winText.fontSize = 48;
        winText.fontStyle = FontStyle.Bold;
        winText.color = Color.yellow;
        winText.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.2f, 0.6f);
        textRect.anchorMax = new Vector2(0.8f, 0.8f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // YES button
        GameObject yesBtn = new GameObject("YesButton");
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

        yesButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        // NO button
        GameObject noBtn = new GameObject("NoButton");
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

        noButton.onClick.AddListener(() =>
        {
            // Do nothing — just close the canvas
            winCanvas.SetActive(false);
        });
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.PlayWin();
            winCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1f; // Make sure time resets if object is destroyed
    }
}