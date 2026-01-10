using UnityEngine;

public class RoomData : MonoBehaviour
{
    public byte RoomID { get; private set; }
    public byte QuestID { get; private set; }

    public void Init(byte roomID, byte questId)
    {
        RoomID = roomID;
        QuestID = questId;
    }
}
