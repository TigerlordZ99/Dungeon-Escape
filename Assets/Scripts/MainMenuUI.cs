using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    void Start()
    {
        CreateMainMenuUI();
    }

    void CreateMainMenuUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.15f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Title
        CreateText(canvasObj, "Title", "DUNGEON ESCAPE",
            new Vector2(0.2f, 0.7f), new Vector2(0.8f, 0.9f),
            60, FontStyle.Bold, Color.yellow);

        // Start Game Button
        CreateButton(canvasObj, "StartButton", "Start Game",
            new Vector2(0.35f, 0.55f), new Vector2(0.65f, 0.65f),
            new Color(0.2f, 0.6f, 0.2f), () =>
            {
                SceneManager.LoadScene("Game");
            });

        // Rules Button
        CreateButton(canvasObj, "RulesButton", "Rules",
            new Vector2(0.35f, 0.40f), new Vector2(0.65f, 0.50f),
            new Color(0.2f, 0.4f, 0.8f), () =>
            {
                ShowRules(canvasObj);
            });

        // Credits Button
        CreateButton(canvasObj, "CreditsButton", "Credits",
            new Vector2(0.35f, 0.25f), new Vector2(0.65f, 0.35f),
            new Color(0.5f, 0.2f, 0.6f), () =>
            {
                ShowCredits(canvasObj);
            });
    }

    void ShowRules(GameObject canvas)
    {
        ShowPopup(canvas, "RULES",
            "- The green room is where you start and the goal is to reach the red room.\n" +
            "- Every other room will have a colored key and 4 doors (top, left, right, bottom) of any color.\n" +
            "- Collect keys to unlock doors to another room.\n" +
            "- Each colored key corresponds to its door. Ex: A red key unlocks a red door.\n" +
            "- You can only use a key once. However, unlocking applies to all the room's 4 doors .\n" +
            "- White colored doors indicate open/unlocked. Any other colored doors indicate locked.\n" +
            "- Find the Master Key to unlock the red room. The master key looks much different that the others.\n" +
            "- Reach the middle of the red room to escape the dungeon.\n" +
            "- Avoid enemies chasing you! Interacting with them will deplete your health.");
    }

    void ShowCredits(GameObject canvas)
    {
        ShowPopup(canvas, "CREDITS",
            "Game developed by:\n" +
            "Anish Bansal & Madhav Ramakrishnan\n\n" +
            "BGM by Shumworld on Freesound\n" +
            "SFX by Freesound-community on Pixabay,\nRaclure and BradWesson on Freesound\n\n" +
            "Key sprite by Giuseppe Ramos on Vecteezy\n" +
            "Enemy sprite by Warren Clark on itch.io\n" +
            "Player sprite by Pipoya on itch.io\n\n" +
            "Made with Unity\n\n" +
            "AI Disclaimer: Generative AI coding tools such as\nClaude and ChatGPT were used to help develop this project");
    }

    void ShowPopup(GameObject canvas, string title, string content)
    {
        // Remove existing popup if any
        Transform existing = canvas.transform.Find("Popup");
        if (existing != null) Destroy(existing.gameObject);

        // Dark overlay panel
        GameObject popup = new GameObject("Popup");
        popup.transform.SetParent(canvas.transform, false);
        Image popupBg = popup.AddComponent<Image>();
        popupBg.color = new Color(0, 0, 0, 0.92f);
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.1f, 0.1f);
        popupRect.anchorMax = new Vector2(0.9f, 0.9f);
        popupRect.offsetMin = Vector2.zero;
        popupRect.offsetMax = Vector2.zero;

        // Title
        CreateText(popup, "PopupTitle", title,
            new Vector2(0.1f, 0.75f), new Vector2(0.9f, 0.95f),
            36, FontStyle.Bold, Color.yellow);

        // Content
        CreateText(popup, "PopupContent", content,
            new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.75f),
            22, FontStyle.Normal, Color.white);

        // Close button
        CreateButton(popup, "CloseButton", "Close",
            new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.18f),
            new Color(0.7f, 0.2f, 0.2f), () =>
            {
                Destroy(popup);
            });
    }

    void CreateButton(GameObject parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btn = new GameObject(name);
        btn.transform.SetParent(parent.transform, false);
        Image img = btn.AddComponent<Image>();
        img.color = color;
        Button button = btn.AddComponent<Button>();
        RectTransform rect = btn.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Button hover effect
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(color.r + 0.2f, color.g + 0.2f, color.b + 0.2f);
        button.colors = colors;

        // Label
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btn.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 28;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        button.onClick.AddListener(onClick);
    }

    void CreateText(GameObject parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, int fontSize, FontStyle style, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}