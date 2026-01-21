using System.Collections.Generic;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    public string value;
    public int id = -1;

    public void Init(Sprite itemSprite, int id, string value)
    {
        // Set up item sprite
        GetComponent<SpriteRenderer>().sprite = itemSprite;

        // Fill other data
        this.value = value;
        this.id = id;
    }
}