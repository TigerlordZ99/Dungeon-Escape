using System.Collections.Generic;
using System.Linq;
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

    [Header("Spawning")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject winObjectPrefab;

    private List<RoomNode> nodes = new List<RoomNode>();
    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
    private Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

    private static readonly KeyColor[] lockColors = { KeyColor.Red, KeyColor.Blue, KeyColor.Green, KeyColor.Yellow };

    void Start()
    {
        // Random dungeon size each run — between 6 and 15 rooms
        mainPathLength = (uint)Random.Range(6, 16);
        Debug.Log($"Dungeon size this run: {mainPathLength} rooms");
        GenerateGraph();
        DrawGraph();
    }

    void GenerateGraph()
    {
        nodes.Clear();
        occupied.Clear();

        Vector2Int currentGrid = Vector2Int.zero;
        RoomNode previous = null;

        for (int i = 0; i < mainPathLength; i++)
        {
            RoomNode newNode = new RoomNode(i, GridToWorld(currentGrid));
            newNode.roomLevel = i + 1;

            if (i == 0) { newNode.isStart = true; newNode.type = RoomType.Start; }
            if (i == mainPathLength - 1) { newNode.isEnd = true; newNode.type = RoomType.Boss; }

            nodes.Add(newNode);
            occupied.Add(currentGrid);

            if (previous != null)
            {
                previous.connections.Add(newNode);
                newNode.connections.Add(previous);
            }

            previous = newNode;

            List<Vector2Int> validDirs = new List<Vector2Int>();
            foreach (Vector2Int dir in directions)
                if (!occupied.Contains(currentGrid + dir))
                    validDirs.Add(dir);

            if (validDirs.Count == 0) break;
            currentGrid += validDirs[Random.Range(0, validDirs.Count)];
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
        Dictionary<int, RoomNode> nodeById = new Dictionary<int, RoomNode>();

        RoomNode startNode = null;
        RoomNode endNode = null;
        List<Vector3> enemySpawnPoints = new List<Vector3>();

        // SPAWN ALL ROOMS
        for (int i = 0; i < nodes.Count; i++)
        {
            RoomNode node = nodes[i];
            GameObject room = Instantiate(RoomNodePrefab, node.position, Quaternion.identity);
            RoomController controller = room.GetComponent<RoomController>();

            if (controller != null)
            {
                controller.SetupRoom(node);
                spawnedControllers.Add(controller);
                nodeById[node.id] = node;

                Tilemap tm = controller.floorTilemap;
                if (tm != null)
                {
                    roomTilemaps[node.id] = tm;
                    Vector3Int cellCenter = new Vector3Int(
                        Mathf.RoundToInt(node.position.x),
                        Mathf.RoundToInt(node.position.y), 0);
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
            }

            if (node.isStart) startNode = node;
            else if (node.isEnd) endNode = node;
            else enemySpawnPoints.Add(new Vector3(node.position.x, node.position.y, 0));
        }

        // BUILD THE KEY CHAIN
        // Shuffle all non-start, non-boss rooms randomly
        List<RoomNode> chainRooms = nodes
            .Where(n => !n.isStart && !n.isEnd)
            .OrderBy(_ => Random.value)
            .ToList();

        // Generate a shuffled color deck — cycles all 4 colors before repeating,
        // so no matter how many rooms there are, colors stay balanced and random
        List<KeyColor> colorDeck = GenerateShuffledColorDeck(chainRooms.Count);

        Dictionary<int, KeyColor> roomKeyAssignments = new Dictionary<int, KeyColor>();

        for (int i = 0; i < chainRooms.Count; i++)
        {
            KeyColor colorForThisStep = colorDeck[i];

            // Lock this room with this color
            RoomController rc = GetControllerForNode(spawnedControllers, nodes, chainRooms[i]);
            if (rc != null) rc.SetLock(colorForThisStep, true);

            // i==0 is the first open room — no key assignment needed before it
            if (i == 0) continue;

            // Room before this one holds the key to open this room
            roomKeyAssignments[chainRooms[i - 1].id] = colorForThisStep;
        }

        // Last chain room holds the master key to open the boss room
        if (chainRooms.Count > 0)
            roomKeyAssignments[chainRooms[chainRooms.Count - 1].id] = KeyColor.Master;

        // Lock boss room with Master
        RoomController bossController = GetControllerForNode(spawnedControllers, nodes, endNode);
        if (bossController != null) bossController.SetLock(KeyColor.Master, true);

        // Start room — always white, no key inside
        RoomController startController = GetControllerForNode(spawnedControllers, nodes, startNode);
        if (startController != null) startController.SetLock(KeyColor.Red, false);

        // First chain room — unlocked so player can enter and grab its key
        if (chainRooms.Count > 0)
        {
            RoomController firstChainController = GetControllerForNode(spawnedControllers, nodes, chainRooms[0]);
            if (firstChainController != null) firstChainController.SetLock(colorDeck[0], false);
        }

        // SPAWN PLAYER
        if (playerPrefab != null && startNode != null)
        {
            GameObject player = Instantiate(playerPrefab,
                new Vector3(startNode.position.x, startNode.position.y, 0),
                Quaternion.identity);
            if (player.GetComponent<PlayerInventory>() == null)
                player.AddComponent<PlayerInventory>();
        }

        // SPAWN WIN OBJECT
        if (winObjectPrefab != null && endNode != null)
        {
            Vector3 centeredPosition = new Vector3(
                endNode.position.x + (gridSpacing / 24),
                endNode.position.y - (gridSpacing / 24), 0);
            Instantiate(winObjectPrefab, centeredPosition, Quaternion.identity);
        }

        // SPAWN KEYS via ItemGenerator
        ItemGenerator itemGen = GetComponent<ItemGenerator>();
        if (itemGen != null)
            itemGen.ManualSpawnWithData(currentDungeonData, roomTilemaps, roomKeyAssignments);

        // REGISTER SPAWN POINTS with EnemySpawner
        EnemySpawner enemySpawner = GetComponent<EnemySpawner>();
        if (enemySpawner != null)
            enemySpawner.RegisterSpawnPoints(enemySpawnPoints);
        else
            Debug.LogWarning("No EnemySpawner found on DungeonManager!");
    }

    // Builds a color list by shuffling all 4 colors into batches until we have enough.
    // e.g. for 9 rooms: [G,R,Y,B] + [B,Y,R,G] + [G] — always all 4 before repeating
    private List<KeyColor> GenerateShuffledColorDeck(int count)
    {
        List<KeyColor> deck = new List<KeyColor>();
        List<KeyColor> batch = new List<KeyColor>(lockColors);

        while (deck.Count < count)
        {
            // Fisher-Yates shuffle the batch
            for (int i = batch.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                KeyColor temp = batch[i];
                batch[i] = batch[j];
                batch[j] = temp;
            }
            deck.AddRange(batch);
        }

        return deck.GetRange(0, count);
    }

    private RoomController GetControllerForNode(List<RoomController> controllers, List<RoomNode> nodeList, RoomNode target)
    {
        int index = nodeList.IndexOf(target);
        if (index >= 0 && index < controllers.Count)
            return controllers[index];
        return null;
    }

    Vector2 GridToWorld(Vector2Int gridPos)
    {
        return new Vector2(gridPos.x * gridSpacing, gridPos.y * gridSpacing);
    }

    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / gridSpacing),
            Mathf.RoundToInt(worldPos.y / gridSpacing));
    }
}