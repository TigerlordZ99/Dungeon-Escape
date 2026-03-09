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

    void Awake()
    {
        if (floorTilemap == null)
            floorTilemap = GetComponentInChildren<Tilemap>();
    }

    public void SetupRoom(RoomNode node)
    {
        // Reset doors
        if (northDoor) northDoor.SetActive(false);
        if (southDoor) southDoor.SetActive(false);
        if (eastDoor) eastDoor.SetActive(false);
        if (westDoor) westDoor.SetActive(false);

        // Open doors based on logical connections
        foreach (RoomNode connection in node.connections)
        {
            Vector2 direction = connection.position - node.position;
            if (direction.y > 0.5f && northDoor) northDoor.SetActive(true);
            if (direction.y < -0.5f && southDoor) southDoor.SetActive(true);
            if (direction.x > 0.5f && eastDoor) eastDoor.SetActive(true);
            if (direction.x < -0.5f && westDoor) westDoor.SetActive(true);
        }

        ApplyRoomColor(node);
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
}