using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomNode
{
    public int id;
    public Vector2 position;
    
    [NonSerialized]
    public List<RoomNode> connections = new List<RoomNode>();
    
    public RoomType type; 
    public int roomLevel;
    
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