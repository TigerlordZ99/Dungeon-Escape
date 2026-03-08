using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

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

    private float gridSpacing = 0.22f;
    private int idCounter = 1;

    private List<RoomNode> nodes = new List<RoomNode>();
    private RoomNode startNode;
    private RoomNode endNode;

    HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    Vector2Int[] directions =
    {
        Vector2Int.right,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.down
    };

    public enum RoomDifficulty
    {
        None,
        Easy,
        Medium,
        Hard
    }

    void Start()
    {
        GenerateGraph();
        DrawGraph();
    }

    void GenerateGraph() 
    {
        // Resets generation
        nodes.Clear();
        occupied.Clear();
        idCounter = 1;

        GenerateMainPath(nodes);

        GenerateBranching(nodes);

        ConnectBranches();

        GenerateRestStops(nodes, restStopCount);

        AssignRoomDifficulty(nodes);
    }

    // Generates the main path "spine" for the rest of generation to base off of, includes start and end rooms
    void GenerateMainPath(List<RoomNode> nodes)
    {
        // Initializes grid, creates starting node
        Vector2Int currentGrid = Vector2Int.zero;

        startNode = new RoomNode(0, GridToWorld(currentGrid));
        startNode.isStart = true;
        nodes.Add(startNode);
        occupied.Add(currentGrid);

        RoomNode current = startNode;

        // Main loop to create main path
        for (int i = 1; i < mainPathLength; i++)
        {
            Vector2Int nextGrid = currentGrid + Vector2Int.right;
            List<Vector2Int> validMoves = new List<Vector2Int>();

            //Figures out which direction the node can go
            foreach (Vector2Int direction in directions)
            {
                Vector2Int candidate = currentGrid + direction;

                if (!occupied.Contains(candidate))
                {
                    validMoves.Add(direction);
                }
            }

            if (validMoves.Count == 0) break;

            //Chooses direction for node
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

    // Creates rooms that branch off of the main "spine", based on a random chance chosen by user
    void GenerateBranching(List<RoomNode> nodes)
    {
        List<RoomNode> spine = new(nodes);

        foreach (RoomNode node in spine)
        {
            Vector2Int baseGrid = WorldToGrid(node.position);
            int branches = Random.Range(0, maxBranchesPerRoom + 1);

            if (node.isStart || node.isEnd)
            {
                continue;
            }

            for (int i = 0; i < branches; i++)
            {
                List<Vector2Int> validDirs = new();

                // Figures out which direction the node can go
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int candidate = baseGrid + dir;
                    if (!occupied.Contains(candidate))
                        validDirs.Add(dir);
                }

                if (validDirs.Count == 0)
                    break;

                //Chooses a random available direction for the node to be
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
        a.connections.Add(b);
        b.connections.Add(a);
    }

    // Helper function to return a node at a specific grid position
    RoomNode GetNodeAtGrid(Vector2Int gridPos)
    {
        foreach (RoomNode node in nodes)
        {
            if (WorldToGrid(node.position) == gridPos)
            {
                return node;
            }
        }

        return null;
    }

    // Connects the branches after they've generated, based on the extra connection chance
    void ConnectBranches()
    {
        foreach(RoomNode node in nodes)
        {
            Vector2Int grid = WorldToGrid(node.position);

            foreach(Vector2Int direction in directions)
            {
                if (Random.value > extraConnectionChance) continue;

                Vector2Int neighborGrid = grid + direction;
                RoomNode neighbor = GetNodeAtGrid(neighborGrid);

                if (neighbor == null) continue;

                if (node.connections.Contains(neighbor)) continue;

                ConnectRooms(node, neighbor);
            }
        }
    }

    // Generates rest stops within the given layout, just changes a room into being a rest stop through the nodes data
    void GenerateRestStops(List<RoomNode> nodes, uint numRests)
    {
        List<RoomNode> validCandidates = new();

        // Finds possible rooms that could be nodes, excludes start and end
        foreach (RoomNode node in nodes)
        {
            if (node == startNode || node == endNode)
                continue;

            validCandidates.Add(node);
        }

        List<RoomNode> placedRestStops = new();

        int safety = 0;
        int maxAttempts = 1000;
        
        // Continually loops until it finds a possible spot based on parameters, if unsuccessful, it will not generate enough rest stops
        while (placedRestStops.Count < numRests && safety < maxAttempts)
        {
            safety++;

            RoomNode candidate = validCandidates[Random.Range(0, validCandidates.Count)];

            bool tooClose = false;

            // Candidates cannot be start or end (sanity check)
            if (candidate == null || candidate == startNode || candidate == endNode)
            {
                continue;
            }

            // Cannot generate next to a start or end node
            if (GridDistance(candidate, startNode) <= minRestStopSeparation || GridDistance(candidate, endNode) <= minRestStopSeparation)
            {
                continue;
            }

            // Checks to see if the rest stops are too close to each other
            foreach (RoomNode rest in placedRestStops)
            {
                if (GridDistance(candidate, rest) <= minRestStopSeparation)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) continue;

            candidate.isRestStop = true;
            placedRestStops.Add(candidate);  
        }
    }

    // Applies a difficulty to each room based on a curve/bias
    void AssignRoomDifficulty(List<RoomNode> nodes)
    {
        // Create new list for the combat rooms
        List<RoomNode> combatRooms = new();

        foreach (RoomNode node in nodes)
        {
            if (node.isRestStop || node == startNode || node == endNode) continue;

            combatRooms.Add(node);
        }

        int total = combatRooms.Count;

        // Bias values based on difficulty slider
        float hardWeight = Mathf.Pow(difficultyBias, 2f);
        float easyWeight = Mathf.Pow(1f - difficultyBias, 2f);
        float mediumWeight = 1f - (hardWeight + easyWeight);

        float weightSum = easyWeight + mediumWeight + hardWeight;
        easyWeight /= weightSum;
        mediumWeight /= weightSum;
        hardWeight /= weightSum;

        int easy = Mathf.RoundToInt(total * easyWeight);
        int medium = Mathf.RoundToInt(total * mediumWeight);
        int hard = total - easy - medium;

        Shuffle(combatRooms);

        int index = 0;

        for (int i = 0; i < easy; i++)
            combatRooms[index++].difficulty = RoomDifficulty.Easy;

        for (int i = 0; i < medium; i++)
            combatRooms[index++].difficulty = RoomDifficulty.Medium;

        for (int i = 0; i < hard; i++)
            combatRooms[index++].difficulty = RoomDifficulty.Hard;
    }

    // Helper function for room difficulty to shuffle list elements
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Non-essential function for generating the map, but this provides the visual representation
    void DrawGraph() 
    { 
        foreach(RoomNode node in nodes)
        {
            GameObject room = Instantiate(RoomNodePrefab, node.position, Quaternion.identity);

            RoomNodeView view = room.GetComponent<RoomNodeView>();
            if (view != null)
                view.Init(node);

            SpriteRenderer sr = room.GetComponent<SpriteRenderer>();
            sr.sortingLayerName = "Rooms";
            sr.sortingOrder = 0;

            if (node == startNode)
            {
                sr.color = Color.green;
            } else if (node == endNode)
            {
                sr.color = Color.red;
            } else if (node.isRestStop)
            {
                sr.color = Color.yellow;
            } else if (node.difficulty == RoomDifficulty.Hard)
            {
                sr.color = new Color(0.3f, 0.3f, 0.3f);
            } else if (node.difficulty == RoomDifficulty.Medium)
            {
                sr.color = new Color(0.6f, 0.6f, 0.6f);
            } else if (node.difficulty == RoomDifficulty.Easy)
            {
                sr.color = new Color(0.9f, 0.9f, 0.9f);
            }

            foreach (RoomNode target in node.connections)
            {
                DrawLine(node.position, target.position);
            }
        }
    }

    // Creates lines to conenct each room visually (NOT CONNECTING ACTUAL GAME OBJECTS, JUST RENDERING VISUALS)
    void DrawLine(Vector2 a, Vector2 b)
    {
        GameObject line = new GameObject("edge");
        LineRenderer renderer = line.AddComponent<LineRenderer>();
        renderer.material = lineMaterial;
        renderer.startWidth = 0.03f;
        renderer.endWidth = 0.03f;
        renderer.positionCount = 2;
        renderer.SetPosition(0, a);
        renderer.SetPosition(1, b);

        renderer.sortingLayerName = "Connections";
        renderer.sortingOrder = 0;
    }

    //Helper functions for the grid system
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

    int GridDistance(RoomNode a, RoomNode b)
    {
        Vector2Int ga = WorldToGrid(a.position);
        Vector2Int gb = WorldToGrid(b.position);

        return Mathf.Abs(ga.x - gb.x) + Mathf.Abs(ga.y - gb.y);
    }
}
