using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomController : MonoBehaviour
{
    [Header("Door Triggers")]
    public GameObject northDoor;
    public GameObject southDoor;
    public GameObject eastDoor;
    public GameObject westDoor;

    [Header("Visuals")]
    public Tilemap floorTilemap;

    public KeyColor lockColor = KeyColor.Red;

    private RoomNode currentNode;

    void Awake()
    {
        if (floorTilemap == null)
            floorTilemap = GetComponentInChildren<Tilemap>();
    }

    public void SetupRoom(RoomNode node)
    {
        currentNode = node;
        ApplyRoomColor(node);

        // Everything starts locked and colored — SetLock() will override this
        SetAllDoors(Color.red, true);
    }

    // Called by RoomGeneratorScript to set the final lock state
    public void SetLock(KeyColor color, bool locked)
    {
        lockColor = color;
        if (locked)
            SetAllDoors(ItemObject.KeyColorToUnityColor(color), true);
        else
            SetAllDoors(Color.white, false);
    }

    // White + collider off = open. Any color + collider on = locked.
    private void SetAllDoors(Color color, bool colliderOn)
    {
        SetDoor(northDoor, color, colliderOn);
        SetDoor(southDoor, color, colliderOn);
        SetDoor(eastDoor, color, colliderOn);
        SetDoor(westDoor, color, colliderOn);
    }

    private void SetDoor(GameObject door, Color color, bool colliderOn)
    {
        if (door == null) return;

        SpriteRenderer sr = door.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;

        Collider2D col = door.GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = false;   // ALWAYS solid — never a trigger
            col.enabled = colliderOn;
        }
    }

    public void ApplyRoomColor(RoomNode node)
    {
        if (floorTilemap == null) return;
        Color c = Color.white;
        if (node.isStart) c = Color.green;
        else if (node.isEnd) c = Color.red;
        floorTilemap.color = c;
    }

    // Player enters the ROOM trigger (on the room root, not the doors)
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"RoomController trigger hit by: {other.gameObject.name} tag: {other.tag}");
        if (!other.CompareTag("Player")) return;
        Debug.Log($"Player entered room. Locked: {IsRoomLocked()}");
        if (!IsRoomLocked()) return;

        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv == null) return;

        KeyColor required = (currentNode != null && currentNode.isEnd) ? KeyColor.Master : lockColor;

        if (inv.HasKey(required))
        {
            inv.TryUseKey(required);
            SetAllDoors(Color.white, false);
            AudioManager.PlayOpenDoor();
            Debug.Log($"Unlocked with {required} key!");
        }
        else
        {
            // Bounce the player back out
            Vector2 pushDir = (other.transform.position - transform.position).normalized;
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = pushDir * 8f;
            Debug.Log($"Need {required} key!");
        }
    }

    private bool IsRoomLocked()
    {
        return IsDoorLocked(northDoor) || IsDoorLocked(southDoor) ||
               IsDoorLocked(eastDoor) || IsDoorLocked(westDoor);
    }

    private bool IsDoorLocked(GameObject door)
    {
        if (door == null) return false;
        SpriteRenderer sr = door.GetComponent<SpriteRenderer>();
        return sr != null && sr.color != Color.white;
    }
}