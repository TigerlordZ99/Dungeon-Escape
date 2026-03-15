using System;
using System.Collections.Generic;
using UnityEngine;

// Tracks collected keys and broadcasts events so other systems can react.
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    // Keys the player is currently holding
    private List<KeyColor> collectedKeys = new List<KeyColor>();

    public bool HasMasterKey { get; private set; } = false;
    public int KeyCount => collectedKeys.Count;

    // Lifetime total — only ever goes up, drives enemy spawning
    public int TotalKeysCollected { get; private set; } = 0;

    // Fired whenever a new key is picked up — EnemySpawner listens to this
    public event Action<int> OnKeyCountChanged;

    // Fired when a key is consumed to open a door
    public event Action<KeyColor> OnKeyConsumed;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CollectKey(Equipment keyData)
    {
        TotalKeysCollected++;

        if (keyData.isMasterKey)
        {
            HasMasterKey = true;
            Debug.Log("Collected the MASTER KEY!");
        }
        else
        {
            collectedKeys.Add(keyData.keyColor);
            Debug.Log($"Collected {keyData.keyColor} key. Total ever: {TotalKeysCollected}");
        }

        // Always fire with lifetime total so enemy spawning never goes backwards
        OnKeyCountChanged?.Invoke(TotalKeysCollected);
        ShowKeyUI(keyData);
    }

    // Returns true and removes the key if the player has it
    public bool TryUseKey(KeyColor color)
    {
        if (color == KeyColor.Master)
        {
            if (!HasMasterKey) return false;
            HasMasterKey = false;
            OnKeyConsumed?.Invoke(color);
            return true;
        }

        if (collectedKeys.Contains(color))
        {
            collectedKeys.Remove(color);
            OnKeyConsumed?.Invoke(color);
            return true;
        }

        return false;
    }

    public bool HasKey(KeyColor color)
    {
        if (color == KeyColor.Master) return HasMasterKey;
        return collectedKeys.Contains(color);
    }

    // Simple floating text so the player gets feedback on pickup
    private void ShowKeyUI(Equipment keyData)
    {
        string msg = keyData.isMasterKey ? "Got the MASTER KEY!" : $"Got {keyData.keyColor} Key!";
        Debug.Log(msg);
        // Wire up your UI here if you have a HUD — e.g. update a key counter panel
    }
}