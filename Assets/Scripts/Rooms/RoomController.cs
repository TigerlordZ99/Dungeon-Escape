using UnityEngine;
using UnityEngine.Tilemaps; // Required to control the Floor_Tilemap component

public class RoomController : MonoBehaviour
{
    [Header("Door Triggers")]
    public GameObject northDoor;
    public GameObject southDoor;
    public GameObject eastDoor;
    public GameObject westDoor;

    [Header("Visuals")]
    // Drag your Floor_Tilemap child object into this slot in the Inspector
    public Tilemap floorTilemap;

    public void SetupRoom(RoomNode node)
    {
        // 1. Reset all doors to inactive initially
        if (northDoor) northDoor.SetActive(false);
        if (southDoor) southDoor.SetActive(false);
        if (eastDoor) eastDoor.SetActive(false);
        if (westDoor) westDoor.SetActive(false);

        // 2. Open doors based on logical connections from the RoomNode data
        foreach (RoomNode connection in node.connections)
        {
            Vector2 direction = connection.position - node.position;

            // Threshold handles floating-point precision for grid alignment
            if (direction.y > 0.5f && northDoor) northDoor.SetActive(true);
            if (direction.y < -0.5f && southDoor) southDoor.SetActive(true);
            if (direction.x > 0.5f && eastDoor) eastDoor.SetActive(true);
            if (direction.x < -0.5f && westDoor) westDoor.SetActive(true);
        }

        // 3. Apply color coding (Green for Start, Red for End)
        ApplyRoomColor(node);
    }

    private void ApplyRoomColor(RoomNode node)
    {
        if (floorTilemap == null) return;

        // Use Color.white to keep your original gray tile color for standard rooms
        Color targetColor = Color.white;

        if (node.isStart)
            targetColor = Color.green;
        else if (node.isEnd)
            targetColor = Color.red;
        else if (node.isRestStop)
            targetColor = Color.yellow;

        // Tints the entire tilemap; White means "no tint" (original colors)
        floorTilemap.color = targetColor;
    }
}