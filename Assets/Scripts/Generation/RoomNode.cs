using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomNode
{
    public int id;
    public Vector2 position;
    public List<RoomNode> connections = new List<RoomNode>();
    
    // Required for ItemGenerator & BFS Scaling
    public RoomType type; 
    public int roomLevel; // Distance from start
    
    public bool isStart;
    public bool isEnd;
    public bool isRestStop;
    
    public RoomDifficulty difficulty;

    public RoomNode(int id, Vector2 position)
    {
        this.id = id;
        this.position = position;
    }
}