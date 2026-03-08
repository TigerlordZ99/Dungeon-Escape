using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGeneratorScript : MonoBehaviour
{
    [Header("Room Generation Rules")]
    public uint mainPathLength;
    [Range(0, 3)] public int maxBranchesPerRoom;
    [Range(0f, 1f)] public float extraConnectionChance;

    [Header("Rest Stop Rules")]
    public uint restStopCount;
    public uint minRestStopSeparation = 3;

    [Header("Layout Difficulty Rules")]
    [Range(0f, 1f)] public float difficultyBias;

    [Header("Visual Representation")]
    public GameObject RoomNodePrefab;
    public Material lineMaterial;

    // Adjusted to prevent 9x9 room overlap
    public float gridSpacing = 12.0f;
    private int idCounter = 1;

    private List<RoomNode> nodes = new List<RoomNode>();
    private RoomNode startNode;
    private RoomNode endNode;
    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    private Vector2Int[] directions = {
        Vector2Int.right,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.down
    };

    void Start()
    {
        GenerateGraph();
        DrawGraph();
    }

    public void GenerateGraph()
    {
        nodes.Clear();
        occupied.Clear();
        idCounter = 1;

        GenerateMainPath(nodes);
        GenerateBranching(nodes);
        ConnectBranches();
        GenerateRestStops(nodes, restStopCount);
        AssignRoomDifficulty(nodes);
    }

    void GenerateMainPath(List<RoomNode> nodes)
    {
        Vector2Int currentGrid = Vector2Int.zero;

        startNode = new RoomNode(0, GridToWorld(currentGrid));
        startNode.isStart = true;
        nodes.Add(startNode);
        occupied.Add(currentGrid);

        RoomNode current = startNode;

        for (int i = 1; i < mainPathLength; i++)
        {
            List<Vector2Int> validMoves = new List<Vector2Int>();
            foreach (Vector2Int direction in directions)
            {
                Vector2Int candidate = currentGrid + direction;
                if (!occupied.Contains(candidate)) validMoves.Add(direction);
            }

            if (validMoves.Count == 0) break;

            Vector2Int chosenDirection = validMoves[Random.Range(0, validMoves.Count)];
            currentGrid += chosenDirection;

            RoomNode next = new RoomNode(idCounter++, GridToWorld(currentGrid));
            nodes.Add(next);
            occupied.Add(currentGrid);

            ConnectRooms(current, next);
            current = next;
        }

        endNode = current;
        current.isEnd = true;
    }

    void GenerateBranching(List<RoomNode> nodes)
    {
        List<RoomNode> spine = new List<RoomNode>(nodes);
        foreach (RoomNode node in spine)
        {
            if (node.isStart || node.isEnd) continue;

            Vector2Int baseGrid = WorldToGrid(node.position);
            int branches = Random.Range(0, maxBranchesPerRoom + 1);

            for (int i = 0; i < branches; i++)
            {
                List<Vector2Int> validDirs = new List<Vector2Int>();
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int candidate = baseGrid + dir;
                    if (!occupied.Contains(candidate)) validDirs.Add(dir);
                }

                if (validDirs.Count == 0) break;

                Vector2Int chosenDir = validDirs[Random.Range(0, validDirs.Count)];
                Vector2Int branchGrid = baseGrid + chosenDir;

                RoomNode branch = new RoomNode(idCounter++, GridToWorld(branchGrid));
                nodes.Add(branch);
                occupied.Add(branchGrid);

                ConnectRooms(node, branch);
            }
        }
    }

    void ConnectRooms(RoomNode a, RoomNode b)
    {
        if (!a.connections.Contains(b)) a.connections.Add(b);
        if (!b.connections.Contains(a)) b.connections.Add(a);
    }

    void ConnectBranches()
    {
        foreach (RoomNode node in nodes)
        {
            Vector2Int grid = WorldToGrid(node.position);
            foreach (Vector2Int direction in directions)
            {
                if (Random.value > extraConnectionChance) continue;

                Vector2Int neighborGrid = grid + direction;
                RoomNode neighbor = GetNodeAtGrid(neighborGrid);

                if (neighbor != null && !node.connections.Contains(neighbor))
                {
                    ConnectRooms(node, neighbor);
                }
            }
        }
    }

    void GenerateRestStops(List<RoomNode> nodes, uint numRests)
    {
        List<RoomNode> candidates = nodes.FindAll(n => !n.isStart && !n.isEnd);
        List<RoomNode> placed = new List<RoomNode>();

        int attempts = 0;
        while (placed.Count < numRests && attempts < 100)
        {
            attempts++;
            RoomNode candidate = candidates[Random.Range(0, candidates.Count)];

            if (GridDistance(candidate, startNode) > minRestStopSeparation &&
                GridDistance(candidate, endNode) > minRestStopSeparation)
            {
                candidate.isRestStop = true;
                placed.Add(candidate);
                candidates.Remove(candidate);
            }
        }
    }

    void AssignRoomDifficulty(List<RoomNode> nodes)
    {
        List<RoomNode> combatRooms = nodes.FindAll(n => !n.isRestStop && !n.isStart && !n.isEnd);
        int total = combatRooms.Count;

        float hardWeight = Mathf.Pow(difficultyBias, 2f);
        float easyWeight = Mathf.Pow(1f - difficultyBias, 2f);
        float mediumWeight = 1f - (hardWeight + easyWeight);

        int easy = Mathf.RoundToInt(total * (easyWeight / (easyWeight + mediumWeight + hardWeight)));
        int medium = Mathf.RoundToInt(total * (mediumWeight / (easyWeight + mediumWeight + hardWeight)));

        for (int i = 0; i < combatRooms.Count; i++)
        {
            if (i < easy) combatRooms[i].difficulty = RoomDifficulty.Easy;
            else if (i < easy + medium) combatRooms[i].difficulty = RoomDifficulty.Medium;
            else combatRooms[i].difficulty = RoomDifficulty.Hard;
        }
    }

    void DrawGraph()
    {
        foreach (RoomNode node in nodes)
        {
            // Spawn the prefab and initialize logic/visuals
            GameObject room = Instantiate(RoomNodePrefab, node.position, Quaternion.identity);

            RoomController controller = room.GetComponent<RoomController>();
            if (controller != null) controller.SetupRoom(node);

            RoomNodeView view = room.GetComponent<RoomNodeView>();
            if (view != null) view.Init(node);

            foreach (RoomNode target in node.connections)
            {
                DrawLine(node.position, target.position);
            }
        }
    }

    void DrawLine(Vector2 a, Vector2 b)
    {
        GameObject line = new GameObject("edge");
        LineRenderer renderer = line.AddComponent<LineRenderer>();
        renderer.material = lineMaterial;
        renderer.startWidth = 0.1f;
        renderer.endWidth = 0.1f;
        renderer.positionCount = 2;
        renderer.SetPosition(0, a);
        renderer.SetPosition(1, b);
    }

    // Helper functions
    RoomNode GetNodeAtGrid(Vector2Int gridPos) => nodes.Find(n => WorldToGrid(n.position) == gridPos);
    Vector2 GridToWorld(Vector2Int gridPos) => new Vector2(gridPos.x * gridSpacing, gridPos.y * gridSpacing);
    Vector2Int WorldToGrid(Vector2 worldPos) => new Vector2Int(Mathf.RoundToInt(worldPos.x / gridSpacing), Mathf.RoundToInt(worldPos.y / gridSpacing));
    int GridDistance(RoomNode a, RoomNode b)
    {
        Vector2Int ga = WorldToGrid(a.position);
        Vector2Int gb = WorldToGrid(b.position);
        return Mathf.Abs(ga.x - gb.x) + Mathf.Abs(ga.y - gb.y);
    }
}