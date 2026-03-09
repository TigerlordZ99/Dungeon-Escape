using UnityEngine;

public class RoomController : MonoBehaviour
{
    public bool isEnd = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(isEnd && other.CompareTag("Player"))
        {
            Debug.Log("You escaped the dungeon!");
        }
    }
}