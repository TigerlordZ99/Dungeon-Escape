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

    private RoomNode currentNode;

    void Awake()
    {
        if (floorTilemap == null)
            floorTilemap = GetComponentInChildren<Tilemap>();
    }

    public void SetupRoom(RoomNode node)
    {
        currentNode = node;

        // Default all doors to BLOCKED (collider on, no passage)
        SetDoorBlocked(northDoor, true);
        SetDoorBlocked(southDoor, true);
        SetDoorBlocked(eastDoor, true);
        SetDoorBlocked(westDoor, true);

        // Open doors that have connections
        foreach (RoomNode connection in node.connections)
        {
            Vector2 direction = connection.position - node.position;
            if (direction.y > 0.5f) SetDoorBlocked(northDoor, false);
            if (direction.y < -0.5f) SetDoorBlocked(southDoor, false);
            if (direction.x > 0.5f) SetDoorBlocked(eastDoor, false);
            if (direction.x < -0.5f) SetDoorBlocked(westDoor, false);
        }

        ApplyRoomColor(node);
    }

    private void SetDoorBlocked(GameObject door, bool blocked)
    {
        if (door == null) return;

        Collider2D col = door.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = blocked; // blocked = true means wall, false means open
    }

    public void ApplyRoomColor(RoomNode node)
    {
        if (floorTilemap == null) return;

        Color targetColor = Color.white;

        if (node.isStart) targetColor = Color.green;
        else if (node.isEnd) targetColor = Color.red;
        else if (node.isRestStop) targetColor = Color.yellow;

        floorTilemap.color = targetColor;
        Debug.Log($"ApplyRoomColor | isStart={node.isStart} | type={node.type} | tilemap={(floorTilemap == null ? "NULL" : "OK")} | color={targetColor}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentNode != null && currentNode.isEnd && other.CompareTag("Player"))
        {
            Debug.Log("You escaped the dungeon!");
        }
    }
}