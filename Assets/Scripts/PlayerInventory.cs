using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;

    Dictionary<int, InventoryItem> inventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        inventory = new();
    }

    public void CollectItem(ItemData item)
    {
        if (inventory.TryGetValue(item.id, out InventoryItem item_))
        {
            item_.amount++;
        }
        else
        {
            inventory.Add(item.id, new InventoryItem(item.id, item.value, 1));
        }

        Debug.Log("Obtained item: " + item.id + " " + item.value);
        Debug.Log("Currently in inventory: " + PrintInventory());

        DataCollector.Instance.LogItemCollected(item);
    }

    public List<InventoryItem> MissingItems(List<InventoryItem> query)
    {
        List<InventoryItem> result = new();

        foreach(var i in query)
        {
            if (inventory.TryGetValue(i.id, out InventoryItem item_))
            {
                if (item_.amount < i.amount) result.Add(new InventoryItem(i.id, i.value, i.amount - item_.amount));
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
            result += item.id + ":" + item.amount + ", ";
        }

        return result;
    }
}

[System.Serializable]
public class InventoryItem
{
    public string value;
    public int amount;

    public int id;

    public InventoryItem(int id, string value, int amount)
    {
        this.id = id;
        this.value = value;
        this.amount = amount;
    }
}