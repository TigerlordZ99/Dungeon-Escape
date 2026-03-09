using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGeneratorScript : MonoBehaviour
{
    [Header("Room Generation Rules")]
    public uint mainPathLength = 6;
    [Range(0, 3)] public int maxBranchesPerRoom = 2;
    public float gridSpacing = 12.0f;

    [Header("Visual Representation")]
    public GameObject RoomNodePrefab;
    public Material lineMaterial;

    private List<RoomNode> nodes = new List<RoomNode>();
    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
    private Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

    void Start()
    {
        GenerateGraph();
        DrawGraph();
    }

    void GenerateGraph()
    {
        nodes.Clear();
        occupied.Clear();

        Vector2Int currentGrid = Vector2Int.zero;
        RoomNode previous = null;

        // MAIN PATH
        for (int i = 0; i < mainPathLength; i++)
        {
            RoomNode newNode = new RoomNode(i, GridToWorld(currentGrid));
            newNode.roomLevel = i + 1;

            if (i == 0)
            {
                newNode.isStart = true;
                newNode.type = RoomType.Start;
            }

            if (i == mainPathLength - 1)
            {
                newNode.isEnd = true;
                newNode.type = RoomType.Boss;
            }

            nodes.Add(newNode);
            occupied.Add(currentGrid);

            if (previous != null)
            {
                previous.connections.Add(newNode);
                newNode.connections.Add(previous);
            }

            previous = newNode;
            currentGrid += directions[Random.Range(0, 4)];
        }

        // BRANCHING
        List<RoomNode> spine = new List<RoomNode>(nodes);

        foreach (RoomNode node in spine)
        {
            if (node.isStart || node.isEnd) continue;

            int branches = Random.Range(0, maxBranchesPerRoom + 1);

            for (int i = 0; i < branches; i++)
            {
                Vector2Int branchGrid = WorldToGrid(node.position) + directions[Random.Range(0, 4)];

                if (!occupied.Contains(branchGrid))
                {
                    RoomNode branch = new RoomNode(nodes.Count, GridToWorld(branchGrid));
                    branch.roomLevel = node.roomLevel + 1;
                    branch.type = RoomType.Common;

                    nodes.Add(branch);
                    occupied.Add(branchGrid);

                    node.connections.Add(branch);
                    branch.connections.Add(node);
                }
            }
        }
    }

    void DrawGraph()
    {
        DungeonSaveData currentDungeonData = new DungeonSaveData { seed = Random.Range(0, 99999) };

        List<RoomController> spawnedControllers = new List<RoomController>();
        Dictionary<int, Tilemap> roomTilemaps = new Dictionary<int, Tilemap>();

        for (int i = 0; i < nodes.Count; i++)
        {
            RoomNode node = nodes[i];
            GameObject room = Instantiate(RoomNodePrefab, node.position, Quaternion.identity);

            RoomController controller = room.GetComponent<RoomController>();

            if (controller != null)
            {
                controller.SetupRoom(node);
                spawnedControllers.Add(controller);

                Tilemap tm = controller.floorTilemap;

                if (tm != null)
                {
                    roomTilemaps[node.id] = tm;

                    // Use the node's world position directly as cell coordinates.
                    // WorldToCell always returns (0,0,0) because the tilemap itself
                    // hasn't moved — only its parent GameObject has been repositioned.
                    Vector3Int cellCenter = new Vector3Int(
                        Mathf.RoundToInt(node.position.x),
                        Mathf.RoundToInt(node.position.y),
                        0
                    );
                    int half = 4;

                    currentDungeonData.rooms.Add(new RoomSaveData
                    {
                        nodeId = node.id,
                        min = new Vector3Int(cellCenter.x - half, cellCenter.y - half, 0),
                        size = new Vector3Int(9, 9, 0),
                        type = node.type,
                        roomLevel = node.roomLevel
                    });
                }
                else
                {
                    Debug.LogWarning($"RoomGeneratorScript: No tilemap found on prefab instance for node id={node.id}");
                }
            }
            else
            {
                Debug.LogWarning($"RoomGeneratorScript: No RoomController found on prefab instance for node id={node.id}");
            }
        }

        // SPAWN ITEMS
        ItemGenerator itemGen = GetComponent<ItemGenerator>();
        if (itemGen != null)
            itemGen.ManualSpawnWithData(currentDungeonData, roomTilemaps);

        // REAPPLY COLORS AFTER ITEM SPAWN — item spawner can dirty tilemap state
        for (int i = 0; i < spawnedControllers.Count; i++)
            spawnedControllers[i].ApplyRoomColor(nodes[i]);
    }

    Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(gridPos.x * gridSpacing, gridPos.y * gridSpacing);
    }

    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / gridSpacing),
            Mathf.RoundToInt(worldPos.y / gridSpacing)
        );
    }
}