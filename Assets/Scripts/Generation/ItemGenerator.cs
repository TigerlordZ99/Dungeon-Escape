using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemGenerator : MonoBehaviour
{
    [Serializable]
    public struct RarityWeight {
        public ItemRarity rarity;
        public int baseWeight;
        public Sprite sprite;
    }

    [Header("Prefabs & Scaling")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Vector3 itemScale = new Vector3(2.5f, 2.5f, 1f);

    [Header("Trade-Off Parameters")]
    [Range(0.5f, 3.0f)] [SerializeField] private float penaltyIntensity = 1.0f;
    [Range(0.5f, 2.0f)] [SerializeField] private float globalDifficulty = 1.0f;

    [Header("Rarity Scaling")]
    [Tooltip("The higher this is, the faster rarity scales toward Legendary as roomLevel increases.")]
    [Range(0.1f, 1.0f)] [SerializeField] private float rarityScalingRate = 0.4f;

    [Header("Natural Placement")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;

    [Header("Dependencies")]
    [SerializeField] private List<RarityWeight> raritySettings;

    // nodeId -> Tilemap, populated by RoomGeneratorScript
    private Dictionary<int, Tilemap> roomTilemaps = new Dictionary<int, Tilemap>();

    // Cached max room level so rarity can scale relative to the dungeon size
    private int maxRoomLevel = 1;

    public void ManualSpawnWithData(DungeonSaveData data, Dictionary<int, Tilemap> tilemaps)
    {
        roomTilemaps = tilemaps;

        // Cache the highest room level so scaling is always relative, not absolute
        foreach (var room in data.rooms)
            if (room.roomLevel > maxRoomLevel) maxRoomLevel = room.roomLevel;

        ExecuteSpawning(data);
    }

    private void ExecuteSpawning(DungeonSaveData data)
    {
        ClearItems();
        GameObject rootFolder = new GameObject("Items Root");
        rootFolder.transform.SetParent(this.transform);

        foreach (var room in data.rooms)
        {
            if (room.type == RoomType.Start) continue;

            if (!roomTilemaps.TryGetValue(room.nodeId, out Tilemap tilemap) || tilemap == null)
            {
                Debug.LogWarning($"ItemGenerator: No tilemap found for room nodeId={room.nodeId}, skipping.");
                continue;
            }

            Transform roomFolder = new GameObject($"Room_{room.roomLevel}").transform;
            roomFolder.SetParent(rootFolder.transform);

            // Always spawn exactly 1 item per room
            SpawnInRoom(room, 1, roomFolder, tilemap);
        }
    }

    private void SpawnInRoom(RoomSaveData room, int targetCount, Transform parent, Tilemap tilemap)
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
            Debug.LogWarning($"ItemGenerator: No valid tiles in room nodeId={room.nodeId} (level {room.roomLevel}). cellBounds={tilemap.cellBounds}");
            return;
        }

        candidates = candidates.OrderBy(t => UnityEngine.Random.value).ToList();
        int spawned = 0;

        foreach (var worldPos in candidates)
        {
            if (spawned >= targetCount) break;
            if (Physics2D.OverlapCircle(worldPos, wallCheckRadius, wallLayer) != null) continue;

            ItemRarity rarity = GetScaledRarity(room.roomLevel, room.type);
            Equipment equip = new Equipment(rarity, room.roomLevel, globalDifficulty, penaltyIntensity, true);
            CreateItemObject(worldPos, equip, parent);
            spawned++;
        }
    }

    private ItemRarity GetScaledRarity(int roomLevel, RoomType type)
    {
        // Boss rooms always get Legendary
        if (type == RoomType.Boss) return ItemRarity.Legendary;

        // progress goes from 0.0 (first room) to 1.0 (last room before boss)
        float progress = Mathf.Clamp01((float)roomLevel / maxRoomLevel);

        float total = 0;
        List<float> weights = new List<float>();

        for (int i = 0; i < raritySettings.Count; i++)
        {
            // Higher rarity index = higher i
            // As progress increases, multiply higher rarities exponentially more
            // rarityScalingRate controls how aggressively this shifts toward Legendary
            float progressBoost = Mathf.Pow(progress + 0.01f, (raritySettings.Count - 1 - i) * rarityScalingRate);
            float rarityBoost = Mathf.Pow(i + 1, progress * raritySettings.Count * rarityScalingRate);

            float finalW = raritySettings[i].baseWeight * rarityBoost / (progressBoost + 0.001f);
            weights.Add(finalW);
            total += finalW;
        }

        float roll = UnityEngine.Random.Range(0, total);
        float cursor = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            cursor += weights[i];
            if (roll <= cursor) return raritySettings[i].rarity;
        }

        return ItemRarity.Normal;
    }

    private void CreateItemObject(Vector3 pos, Equipment data, Transform parent)
    {
        GameObject obj = Instantiate(itemPrefab, pos, Quaternion.identity, parent);
        obj.tag = "Item";

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