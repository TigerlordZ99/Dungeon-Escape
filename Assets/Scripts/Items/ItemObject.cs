using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Attached to the key prefab. Handles pickup collision and visual gizmos.
public class ItemObject : MonoBehaviour
{
    public Equipment itemData;

    public void Setup(Equipment data)
    {
        itemData = data;

        if (TryGetComponent(out SpriteRenderer sr))
            sr.sprite = data.itemSprite;

        // Tint the sprite to match key color so it's readable at a glance
        if (TryGetComponent(out SpriteRenderer tint))
            tint.color = KeyColorToUnityColor(data.keyColor);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv == null) return;

        inv.CollectKey(itemData);
        AudioManager.PlayPickupKey();
        Destroy(gameObject);
    }

    public static Color KeyColorToUnityColor(KeyColor kc)
    {
        return kc switch
        {
            KeyColor.Red    => new Color(1f,   0.2f, 0.2f),
            KeyColor.Blue   => new Color(0f, 36f/255f, 1f),
            KeyColor.Green  => new Color(0.2f, 1f,   0.2f),
            KeyColor.Yellow => new Color(1f,   0.95f, 0.2f),
            KeyColor.Master => new Color(239f/255f, 0f, 1f),
            _               => Color.white
        };
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (itemData == null) return;

        Gizmos.color = KeyColorToUnityColor(itemData.keyColor);
        Gizmos.DrawWireCube(transform.position, transform.localScale * 0.7f);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = KeyColorToUnityColor(itemData.keyColor);
        style.fontSize         = 12;
        style.fontStyle        = FontStyle.Bold;
        style.alignment        = TextAnchor.MiddleCenter;

        string label = itemData.isMasterKey
            ? "MASTER KEY"
            : $"{itemData.keyColor} Key\n{itemData.GetStatString()}";

        Handles.Label(transform.position + Vector3.up * 0.6f, label, style);
    }
#endif
}