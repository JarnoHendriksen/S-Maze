using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Transform player;

    [SerializeField] public List<Item> items;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
}

[System.Serializable]
public class Item
{
    public int id;
    public ItemType type;
    public Sprite sprite;
    public string value;
}
