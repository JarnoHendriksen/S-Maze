using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    Dictionary<string, InventoryItem> inventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        inventory = new();
    }

    public void CollectItem(ItemData item)
    {
        if (inventory.TryGetValue(item.ID, out InventoryItem item_))
        {
            item_.amount++;
        }
        else
        {
            inventory.Add(item.ID, new InventoryItem(item.type, item.value, 1));
        }

        Debug.Log("Obtained item: " + item.type.ToString() + " " + item.value);
        Debug.Log("Currently in inventory: " + PrintInventory());

        DataCollector.Instance.LogItemCollected(item);
    }

    public List<InventoryItem> MissingItems(List<InventoryItem> query)
    {
        List<InventoryItem> result = new();

        foreach(var i in query)
        {
            if (inventory.TryGetValue(i.ID, out InventoryItem item_))
            {
                if (item_.amount < i.amount) result.Add(new InventoryItem(i.type, i.value, i.amount - item_.amount));
            }
            else result.Add(i);
        }

        return result;
    }

    public string PrintInventory()
    {
        string result = "";

        foreach(var item in inventory.Values)
        {
            result += item.ID + ":" + item.amount + ", ";
        }

        return result;
    }
}

[System.Serializable]
public class InventoryItem
{
    public ItemType type;
    public string value;
    public int amount;

    public string ID
    {
        get
        {
            return type + value; // Example: A minor chord = 2Am, C# note = 1C#
        }
    }

    public InventoryItem(ItemType type, string value, int amount)
    {
        this.type = type;
        this.value = value;
        this.amount = amount;
    }
}