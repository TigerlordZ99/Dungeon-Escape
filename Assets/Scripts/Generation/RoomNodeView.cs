using UnityEngine;
using static RoomGeneratorScript;

public class RoomNodeView : MonoBehaviour
{
    [Header("Generated Data (Read Only)")]
    [SerializeField] int id;
    [SerializeField] bool isStart;
    [SerializeField] bool isEnd;
    [SerializeField] bool isRestStop;
    [SerializeField] RoomDifficulty difficulty;

    // Called by the generator after instantiation
    public void Init(RoomNode node)
    {
        id = node.id;
        isStart = node.isStart;
        isEnd = node.isEnd;
        isRestStop = node.isRestStop;
        difficulty = node.difficulty;
    }
}