using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemGenerator : MonoBehaviour
{
    [Serializable]
    public struct RarityWeight
    {
        public ItemRarity rarity;
        public int baseWeight;
        public Sprite sprite;
    }

    [Header("Prefabs & Scaling")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Vector3 itemScale = new Vector3(2.5f, 2.5f, 1f);

    [Header("Natural Placement")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;

    [Header("Dependencies")]
    [SerializeField] private List<RarityWeight> raritySettings;

    private Dictionary<int, Tilemap> roomTilemaps = new Dictionary<int, Tilemap>();

    // nodeId -> exact key color to spawn, set by RoomGeneratorScript
    private Dictionary<int, KeyColor> keyAssignments = new Dictionary<int, KeyColor>();

    public void ManualSpawnWithData(DungeonSaveData data, Dictionary<int, Tilemap> tilemaps, Dictionary<int, KeyColor> assignments)
    {
        roomTilemaps = tilemaps;
        keyAssignments = assignments;
        ExecuteSpawning(data);
    }

    private void ExecuteSpawning(DungeonSaveData data)
    {
        ClearItems();
        GameObject rootFolder = new GameObject("Items Root");
        rootFolder.transform.SetParent(this.transform);

        foreach (var room in data.rooms)
        {
            // Only spawn a key if this room has an assignment
            if (!keyAssignments.ContainsKey(room.nodeId)) continue;

            if (!roomTilemaps.TryGetValue(room.nodeId, out Tilemap tilemap) || tilemap == null)
            {
                Debug.LogWarning($"ItemGenerator: No tilemap for nodeId={room.nodeId}");
                continue;
            }

            Transform roomFolder = new GameObject($"Room_{room.roomLevel}").transform;
            roomFolder.SetParent(rootFolder.transform);

            SpawnKeyInRoom(room, roomFolder, tilemap, keyAssignments[room.nodeId]);
        }
    }

    private void SpawnKeyInRoom(RoomSaveData room, Transform parent, Tilemap tilemap, KeyColor keyColor)
    {
        List<Vector3> candidates = new List<Vector3>();
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin + 2; x < bounds.xMax - 2; x++)
        {
            for (int y = bounds.yMin + 2; y < bounds.yMax - 2; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(cell))
                {
                    Vector3 worldPos = tilemap.GetCellCenterWorld(cell);
                    worldPos.z = -1f;
                    candidates.Add(worldPos);
                }
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"ItemGenerator: No valid tiles in room nodeId={room.nodeId}");
            return;
        }

        candidates = candidates.OrderBy(_ => UnityEngine.Random.value).ToList();

        foreach (var worldPos in candidates)
        {
            if (Physics2D.OverlapCircle(worldPos, wallCheckRadius, wallLayer) != null) continue;

            // Convert KeyColor to ItemRarity for Equipment constructor
            ItemRarity rarity = KeyColorToRarity(keyColor);
            Equipment equip = new Equipment(rarity, room.roomLevel, 1f, 1f, false);

            CreateItemObject(worldPos, equip, parent);
            return; // Only spawn one key per room
        }
    }

    private ItemRarity KeyColorToRarity(KeyColor color)
    {
        return color switch
        {
            KeyColor.Red => ItemRarity.Red,
            KeyColor.Blue => ItemRarity.Blue,
            KeyColor.Green => ItemRarity.Green,
            KeyColor.Yellow => ItemRarity.Yellow,
            KeyColor.Master => ItemRarity.Master,
            _ => ItemRarity.Red
        };
    }

    private void CreateItemObject(Vector3 pos, Equipment data, Transform parent)
    {
        GameObject obj = Instantiate(itemPrefab, pos, Quaternion.identity, parent);
        obj.tag = "Item";
        obj.transform.localScale = itemScale;

        var set = raritySettings.Find(s => s.rarity == data.rarity);
        data.itemSprite = set.sprite;

        if (obj.TryGetComponent(out ItemObject itemObj))
            itemObj.Setup(data);
    }

    public void ClearItems()
    {
        foreach (var s in GameObject.FindGameObjectsWithTag("Item"))
            DestroyImmediate(s);
    }
}