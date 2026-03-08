using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomNode
{
    public int id;
    public Vector2 position;
    public List<RoomNode> connections = new List<RoomNode>();

    // Status flags for the generator and view
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