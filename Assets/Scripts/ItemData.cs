using System.Collections.Generic;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    public ItemType type;
    public string value;

    public string ID
    {
        get
        {
            return type + value; // Example: A minor chord = ChordAm, C# note = NoteC#
        }
    }

    public void Init(Dictionary<string, Sprite> itemSprites, ItemType type, string value, bool showValue)
    {
        // Set up item sprite
        (string note_name, string modifier) = GetSpriteNames(value);
        string item_type = type.ToString().ToLower();

        if (itemSprites.TryGetValue(item_type, out Sprite itemSprite))
        {
            GetComponent<SpriteRenderer>().sprite = itemSprite;
        }

        if (!showValue) return;


        if (itemSprites.TryGetValue(note_name, out Sprite itemNameSprite))
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = itemNameSprite;
        }

        if (itemSprites.TryGetValue(modifier, out Sprite modifierSprite))
        {
            transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = modifierSprite;
        }

        // Fill other data
        this.type = type;
        this.value = value;
    }

    (string, string) GetSpriteNames(string value)
    {
        string note_name = "note_name_";
        string modifier = "";

        switch (value[0])
        {
            case 'A':
                note_name += "A";
                break;
            case 'B':
                note_name += "B";
                break;
            case 'C':
                note_name += "C";
                break;
            case 'D':
                note_name += "D";
                break;
            case 'E':
                note_name += "E";
                break;
            case 'F':
                note_name += "F";
                break;
            case 'G':
                note_name += "G";
                break;
        }

        if (value.Length == 1) return (note_name, modifier);

        switch (value[1])
        {
            case 'm':
                modifier = "minor";
                break;
            case 'b':
                modifier = "flat";
                break;
            case '#':
                modifier = "sharp";
                break;
        }

        return (note_name, modifier);
    } 
}

public enum ItemType
{
    None,
    Note,
    Chord,
};