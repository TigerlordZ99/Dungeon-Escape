using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RoomGeneratorScript;

public class RoomNode
{
    public int id;
    public Vector2 position;
    public bool isRestStop = false;
    public bool isStart = false;
    public bool isEnd = false;
    public List<RoomNode> connections = new List<RoomNode>();
    public RoomDifficulty difficulty = RoomDifficulty.None;

    public  RoomNode(int id, Vector2 pos)
    {
        this.id = id;
        this.position = pos;
    }
}
