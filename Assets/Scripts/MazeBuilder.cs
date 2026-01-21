using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Linq;

using ParseFunc = System.Func<PixelData, ColorConstraints, bool>;
using System.Collections;

public class MazeBuilder : MonoBehaviour
{
    public static MazeBuilder instance;

    [Header("Maze Object Prefabs")]
    [SerializeField] GameObject wallCell;
    [SerializeField] GameObject floorCell;
    [SerializeField] GameObject doorCell;
    [SerializeField] GameObject fakeWallCell;
    [SerializeField] GameObject itemPlaceholder;
    [SerializeField] GameObject room11x11Prefab;
    [SerializeField] GameObject exitCell;

    [Header("Puzzles")]
    [SerializeField] GameObject memoryPuzzlePrefab;

    [Header("Maze Object Containers")]
    [SerializeField] Transform wallObjects;
    [SerializeField] Transform fakeWallObjects;
    [SerializeField] Transform floorObjects;
    [SerializeField] Transform doorObjects;
    [SerializeField] Transform itemObjects;
    [SerializeField] Transform roomObjects;

    [Header("Maze Layouts")]
    [SerializeField] Texture2D level1;
    [SerializeField] Texture2D level2;
    [SerializeField] Texture2D level3;

    [Header("")]
    [SerializeField] Transform player;
    [SerializeField] Texture2D mazeLayout;
    [SerializeField] List<Quest> quests;

    List<Room> rooms;

    public int RoomCount { get; private set; }

    public int QuestCount { get; private set; }
    private List<GameObject> spawnedRooms = new List<GameObject>(); // List to track rooms
    private int currentGenerationLevel = 1;


    float cellSizeInUnits;

    PlayerController playerCtrl;

    public bool Ready { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        rooms = new();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Only call from GameManager.Start() to make sure
    // MazeBuilder is initialized before trying to generate maze
    public void Init()
    {
        QuestCount = quests.Count;

        Sprite wallSprite = wallCell.GetComponent<SpriteRenderer>().sprite;
        cellSizeInUnits = wallSprite.bounds.size.x;

        playerCtrl = player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (playerCtrl == null)
        {
            playerCtrl = player.GetComponent<PlayerController>();
            return;
        }
        if (playerCtrl.IsInWall)
        {
            for (int i = 0; i < fakeWallObjects.childCount; i++)
            {
                Transform c = fakeWallObjects.GetChild(i);
                c.GetComponent<SpriteRenderer>().enabled = false;
                c.GetComponent<ShadowCaster2D>().enabled = false;
            }
        }
        else if (!playerCtrl.IsInWall)
        {
            for (int i = 0; i < fakeWallObjects.childCount; i++)
            {
                Transform c = fakeWallObjects.GetChild(i);
                c.GetComponent<SpriteRenderer>().enabled = true;
                c.GetComponent<ShadowCaster2D>().enabled = true;
            }
        }
    }

    public IEnumerator GenerateMaze(int level = 1)
    {
        Ready = false;
        currentGenerationLevel = level;

        switch (level)
        {
            case 1: yield return StartCoroutine(Img2Map(level1)); break;
            case 2: yield return StartCoroutine(Img2Map(level2)); break;
            case 3: yield return StartCoroutine(Img2Map(level3)); break;
            default: yield break;
        }

        List<QuizQuestion> levelQuestions = GameManager.instance.GetQuizQuestions((level+1) * RoomCount, true);
        List<Item> allRequiredItems = new();

        foreach(var q in levelQuestions)
        {
            foreach (var i in q.relevantInfoIds)
            {
                Item item = GameManager.instance.items.Find(x => x.id.Equals(i));
                allRequiredItems.Add(item);
            }
        }

        PopulateItemPlaceholders(allRequiredItems);
        RemoveUnusedPlaceholders();

        SpawnAllPuzzles();

        // Combine colliders of child objects
        fakeWallObjects.GetComponent<CompositeCollider2D>().GenerateGeometry();

        wallObjects.GetComponent<CompositeCollider2D>().GenerateGeometry();

        QuestCount = rooms.Count;

        Ready = true;

        yield return null;
    }

    bool TouchesSpace(bool[] isWall, Vector2 coord, Vector2 mazeDim)
    {
        int idx = (int)coord.x + (int)coord.y * (int)mazeDim.x;

        if (!isWall[idx]) return false;

        int mazeSize = (int)(mazeDim.x * mazeDim.y);

        if (idx - 1 >= 0 && idx - 1 < mazeSize)
            if (!isWall[idx - 1]) return true;

        if (idx + 1 >= 0 && idx + 1 < mazeSize)
            if (!isWall[idx + 1]) return true;

        if (idx - mazeDim.x >= 0 && idx - mazeDim.x < mazeSize)
            if (!isWall[idx - (int)mazeDim.x]) return true;

        if (idx + mazeDim.x >= 0 && idx + mazeDim.x < mazeSize)
            if (!isWall[idx + (int)mazeDim.x]) return true;

        return false;
    }

    IEnumerator Img2Map(Texture2D layout)
    {
        Ready = false;

        int w = layout.width;
        int h = layout.height;

        bool[] isWall = new bool[w * h];

        Transform newCell;

        // Color coding:
        // Player: #ff0000
        // Door: #ffffXX, XX = clockwise rotation percentage in hex (i.e. 19 = 25%, 32 = 50%, 4b = 75%)
        // Item: #ff00ff
        // Hidden corridor: #505050
        // Room: #XXY0ZZ, XX = room id (10-99), Y = level id (0, 1, 2), ZZ = orientation
        // Exit: #beeeef

        ColorSet validOrientations = new(0x00, 0x19, 0x32, 0x4b);
        ColorRange validRoomIds = new(0x10, 0x99);
        ColorSet validLevelIds = new(0x00, 0x10, 0x20);

        ColorConstraints playerColor = new(0xff, 0x00, 0x00);
        ColorConstraints doorColor = new(new ExactColor(0xff), new ExactColor(0xff), validOrientations);
        ColorConstraints objectColor = new(0xff, 0x00, 0xff);
        ColorConstraints hiddenPathColor = new(new ExactColor(0x50), new ExactColor(0x50), new ExactColor(0x50));
        ColorConstraints roomColor = new(validRoomIds, validLevelIds, validOrientations);
        ColorConstraints exitColor = new(0xbe, 0xee, 0xef);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Color pixel = layout.GetPixel(x, y);
                if (pixel == Color.black)
                {
                    // Only mark that this is a wall position
                    // Do not instantiate yet
                    isWall[x + y * w] = true;
                    continue;
                }
                else
                {
                    newCell = CreateCell(x, y, floorCell);
                    newCell.SetParent(floorObjects);
                    isWall[x + y * w] = false;
                }

                PixelData pd = new (x, y, pixel);

                bool success = false;

                (ParseFunc parser, ColorConstraints cc)[] parsers =
                { 
                    (ParsePlayer, playerColor), (ParseDoor, doorColor),
                    (ParseObject, objectColor), (ParseHiddenPath, hiddenPathColor),
                    (ParseRoom, roomColor), (ParseExit, exitColor)
                };

                int funcIdx = 0;

                while(!success && funcIdx < parsers.Length)
                {
                    success = parsers[funcIdx].parser.Invoke(pd, parsers[funcIdx].cc);
                    funcIdx++;
                }
            }

            yield return null;
        }

        // Instantiate only the wall cells that are adjacent to the floor tiles
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (TouchesSpace(isWall, new Vector2(x, y), new Vector2(w, h)))
                {
                    Transform newWall = CreateCell(x, y, wallCell);
                    newWall.SetParent(wallObjects);
                }
            }
            yield return null;
        }
    }

    void SpawnAllPuzzles()
    {
        if (spawnedRooms.Count == 0 || memoryPuzzlePrefab == null) return;

        // Shuffle the list of rooms to ensure random locations
        for (int i = 0; i < spawnedRooms.Count; i++)
        {
            GameObject temp = spawnedRooms[i];
            int randomIndex = UnityEngine.Random.Range(i, spawnedRooms.Count);
            spawnedRooms[i] = spawnedRooms[randomIndex];
            spawnedRooms[randomIndex] = temp;
        }

        // Loop through all rooms and assign puzzles
        for (int i = 0; i < spawnedRooms.Count; i++)
        {
            //get type 1, 2, 3.
            int type = (i % 3) + 1;

            // get base length 3, 3, 3, 4, 4, 4.
            int length = (i / 3) + 3;

            GameObject chosenRoom = spawnedRooms[i];

            float xOffset = 3.5f * cellSizeInUnits;
            float yOffset = 8f * cellSizeInUnits;

            Vector3 puzzlePos = chosenRoom.transform.position + new Vector3(xOffset, yOffset, 0);

            GameObject puzzle = Instantiate(memoryPuzzlePrefab, puzzlePos, Quaternion.identity);

            puzzle.transform.SetParent(chosenRoom.transform);
            chosenRoom.name += $"_Puzzle_Type{type}_Len{length}";

            MemoryPuzzle mpScript = puzzle.GetComponent<MemoryPuzzle>();
            if (mpScript != null)
            {
                mpScript.InitPuzzle(type, length, GameManager.instance.Level + 1);
            }
        }
    }

    Transform CreateCell(int x, int y, GameObject cellType)
    {
        var newCell = Instantiate(cellType);
        newCell.transform.position = new Vector3(x * cellSizeInUnits, y * cellSizeInUnits, 0);

        return newCell.transform;
    }

    bool ParsePlayer(PixelData pixel, ColorConstraints cc)
    {
        (byte r, byte g, byte b) = ToRGB255(pixel.color);
        if (!cc.red.IsValid(r) || !cc.green.IsValid(g) || !cc.blue.IsValid(b)) return false;

        player.position = new Vector3(pixel.position.x * cellSizeInUnits, pixel.position.y * cellSizeInUnits, 0);

        return true;
    }
    bool ParseDoor(PixelData pixel, ColorConstraints cc)
    {
        (byte r, byte g, byte b) = ToRGB255(pixel.color);
        if (!cc.red.IsValid(r) || !cc.green.IsValid(g) || !cc.blue.IsValid(b)) return false;

        Transform door = CreateCell(pixel.position.x, pixel.position.y, doorCell);
        door.SetParent(doorObjects);
        float rotation = b / 100f;
        door.eulerAngles = new Vector3(0, 0, -360f * rotation);

        return true;
    }

    bool ParseObject(PixelData pixel, ColorConstraints cc)
    {
        (byte r, byte g, byte b) = ToRGB255(pixel.color);
        if (!cc.red.IsValid(r) || !cc.green.IsValid(g) || !cc.blue.IsValid(b)) return false;

        Debug.Log("parsing item");

        Transform obj = CreateCell(pixel.position.x, pixel.position.y, itemPlaceholder);
        obj.SetParent(itemObjects);

        return true;
    }

    bool ParseHiddenPath(PixelData pixel, ColorConstraints cc)
    {
        (byte r, byte g, byte b) = ToRGB255(pixel.color);
        if (!cc.red.IsValid(r) || !cc.green.IsValid(g) || !cc.blue.IsValid(b)) return false;

        Transform fakeWall = CreateCell(pixel.position.x, pixel.position.y, fakeWallCell);
        fakeWall.SetParent(fakeWallObjects);

        return true;
    }

    bool ParseRoom(PixelData pixel, ColorConstraints cc)    // Creates a room with a set of three possible spawns of forniture, texture are coming
    {
        (byte r, byte g, byte b) = ToRGB255(pixel.color);
        if (!cc.red.IsValid(r) || !cc.green.IsValid(g) || !cc.blue.IsValid(b)) return false;

        float originX = (pixel.position.x - 5) * cellSizeInUnits;
        float originY = (pixel.position.y - 5) * cellSizeInUnits;
        Vector3 finalPos = new Vector3(originX, originY, 0);
        GameObject newRoom = Instantiate(room11x11Prefab, finalPos, Quaternion.identity);

        newRoom.transform.SetParent(floorObjects);

        spawnedRooms.Add(newRoom);

        RoomCount++;

        return true;
    }

    bool ParseExit(PixelData pixel, ColorConstraints cc)    // The player just need to pass throw, we can set that he also have to press space
    {
        (byte r, byte g, byte b) = ToRGB255(pixel.color);
        if (!cc.red.IsValid(r) || !cc.green.IsValid(g) || !cc.blue.IsValid(b)) return false;

        Transform exit = CreateCell(pixel.position.x, pixel.position.y, exitCell);

        exit.SetParent(doorObjects);

        exit.eulerAngles = new Vector3(0, 0, 90);

        exit.name = $"Exit_{pixel.position.x}_{pixel.position.y}";

        return true;
    }

    void PopulateItemPlaceholders(List<Item> requiredItems)
    {
        List<ItemData> allPlaceholders = itemObjects.GetComponentsInChildren<ItemData>().ToList();

        Debug.Log("Required Item Count: " + requiredItems.Count);
        Debug.Log("Placeholder Count: " + allPlaceholders.Count);

        if (allPlaceholders.Count <= 0) return;

        List<Item> itemsToDistribute = requiredItems;

        while (itemsToDistribute.Count > 0)
        {
            int placeholderIdx = UnityEngine.Random.Range(0, allPlaceholders.Count);
            int itemIdx = UnityEngine.Random.Range(0, itemsToDistribute.Count);

            allPlaceholders[placeholderIdx].Init(itemsToDistribute[itemIdx].sprite, itemsToDistribute[itemIdx].id, itemsToDistribute[itemIdx].value);

            allPlaceholders.RemoveAt(placeholderIdx);
            itemsToDistribute.RemoveAt(itemIdx);
        }
    }

    void RemoveUnusedPlaceholders()
    {
        List<ItemData> allPlaceholders = itemObjects.GetComponentsInChildren<ItemData>().ToList();

        foreach (var item in allPlaceholders)
        {
            if (item.id == -1)
            {
                Destroy(item.gameObject);
            }
        }
    }

    void DestroyAllChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    public void DeleteMaze()
    {
        DestroyAllChildren(wallObjects);
        DestroyAllChildren(floorObjects);
        DestroyAllChildren(itemObjects);
        DestroyAllChildren(fakeWallObjects);
        DestroyAllChildren(doorObjects);
        DestroyAllChildren(roomObjects);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).gameObject.CompareTag("exit"))
                Destroy(transform.GetChild(i).gameObject);
        }
        rooms.Clear();

        spawnedRooms.Clear();
    }

    public static (byte, byte, byte) ToRGB255(Color c)
    {
        byte r = (byte)(c.r * 255);
        byte g = (byte)(c.g * 255);
        byte b = (byte)(c.b * 255);

        return (r, g, b);
    }

    public static byte ToRGB255(float c)
    {
        return (byte)(c * 255);
    }

    public static Color FromRGB255(byte r, byte g, byte b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    public void openDoor(GameObject door)
    {
        door.SetActive(false);
    }

    public GameObject closestDoor(GameObject[] Doors, Vector2 playerPosition, float threshold)
    {
        GameObject result = null;
        float distance = Mathf.Infinity;
        foreach (GameObject door in Doors)
        {
            distance = Vector3.Distance(door.transform.position, playerPosition);
            if (distance < threshold)
            {
                threshold = distance;
                result = door;
            }
        }
        return result;
    }
}

public class PixelData
{
    public Vector2Int position;
    public Color color;

    public PixelData(int x, int y, Color c)
    {
        position = new Vector2Int(x, y);
        color = c;
    }
}

[System.Serializable]
public class ColorConstraints
{
    public IColorConstraint red;
    public IColorConstraint green;
    public IColorConstraint blue;

    public ColorConstraints(IColorConstraint r, IColorConstraint g, IColorConstraint b)
    {
        red = r;
        green = g;
        blue = b;
    }
    public ColorConstraints(byte r, byte g, byte b)
    {
        red = new ExactColor(r);
        green = new ExactColor(g);
        blue = new ExactColor(b);
    }

    public bool AreValid((byte r, byte g, byte b) c)
    {
        return red.IsValid(c.r) && green.IsValid(c.g) && blue.IsValid(c.b);
    }
}

public abstract class IColorConstraint
{
    public abstract bool IsValid(byte v);
}

public class ExactColor : IColorConstraint
{
    public byte value;

    public ExactColor(byte v)
    {
        value = v;
    }

    public override bool IsValid(byte v)
    {
        return value == v;
    }
}

public class ColorSet : IColorConstraint
{
    public byte[] validValues;

    public ColorSet(params byte[] values)
    {
        validValues = values;
    }

    public override bool IsValid(byte v)
    {
        foreach (byte v2 in validValues) 
        {
            if (v == v2) return true;
        }

        return false;
    }
}

public class ColorRange : IColorConstraint
{
    public byte min;
    public byte max;

    public ColorRange(byte min, byte max)
    {
        this.min = min;
        this.max = max;
    }

    public override bool IsValid(byte v)
    {
        return v >= min && v <= max;       // Changed because causing problems with the spawning of forniture in the secret rooms
    }
}

[System.Serializable]
public class Quest
{
    public byte id;
    public string name;
    public string description;
    public List<int> requiredItemIds;
}

[System.Serializable]
public class RoomPrefab
{
    public GameObject prefab;

    [Tooltip("Can be used to prevent more complicated puzzles from appearing too soon.")]
    public byte minimumLevel;
    public Quest quest;
}

public class Room
{
    public Transform transform;
    public byte questId;
    public byte roomId;

    public Room(byte roomId, Transform transform, byte questId)
    {
        this.roomId = roomId;
        this.transform = transform;
        this.questId = questId;
    }
}