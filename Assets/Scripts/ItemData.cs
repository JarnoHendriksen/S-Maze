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
}

public enum ItemType
{
    None,
    Note,
    Chord,
};