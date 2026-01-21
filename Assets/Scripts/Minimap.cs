using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [SerializeField] Texture2D level1Map;
    [SerializeField] Texture2D level2Map;
    [SerializeField] Texture2D level3Map;

    [SerializeField] Transform player;

    int l1Size, l2Size, l3Size;

    Texture2D targetMap1, targetMap2, targetMap3;
    Texture2D[] targetMaps;
    Sprite targetSprite;

    int currentLevel = 1;

    ColorConstraints hiddenWallConst;

    private void Awake()
    {
        l1Size = level1Map.width;
        l2Size = level2Map.width;
        l3Size = level3Map.width;
    }

    public void SetLevel(int lvl)
    {
        currentLevel = lvl;
    }

    void CopyPixel(Texture2D source, Texture2D dest, int x, int y)
    {
        dest.SetPixel(x, y, source.GetPixel(x, y));
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetMap1 = new Texture2D(l1Size, l1Size);
        targetMap1.filterMode = FilterMode.Point;

        targetMap2 = new Texture2D(l2Size, l2Size);
        targetMap2.filterMode = FilterMode.Point;

        targetMap3 = new Texture2D(l3Size, l3Size);
        targetMap3.filterMode = FilterMode.Point;

        targetMaps = new Texture2D[3] {targetMap1, targetMap2,  targetMap3};

        ResetTargetMaps();

        Rect fullRect = new Rect(0, 0, targetMaps[currentLevel-1].width, targetMaps[currentLevel-1].height);
        targetSprite = Sprite.Create(targetMap1, fullRect, Vector2.zero);
        GetComponent<Image>().sprite = targetSprite;

        ColorRange range = new(0x10, 0xbb);
        hiddenWallConst = new(range, range, range);
    }

    void ResetTargetMaps()
    {
        foreach (var map in targetMaps)
        {
            for (int x = 0; x < map.width; x++)
            {
                for ( int y = 0; y < map.height; y++)
                {
                    map.SetPixel(x, y, Color.black);
                }
            }

            map.Apply();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Texture2D currentMap = currentLevel switch
        {
            1 => level1Map,
            2 => level2Map,
            3 => level3Map,
            _ => level1Map
        };

        int playerX = Mathf.RoundToInt(player.position.x);
        int playerY = Mathf.RoundToInt(player.position.y);

        // Consider a square extending 1 pixel out from the player pos
        int minX = playerX - 1;
        int maxX = playerX + 1;
        int minY = playerY - 1;
        int maxY = playerY + 1;

        Color playerPosCol = currentMap.GetPixel(playerX, playerY);
        var playerPosrgb255 = MazeBuilder.ToRGB255(playerPosCol);

        bool isInWall = hiddenWallConst.AreValid(playerPosrgb255);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (x < 0 || y < 0 || x > targetMaps[currentLevel-1].width || y > targetMaps[currentLevel - 1].height)
                    continue;

                Color currentPixel = currentMap.GetPixel(x, y);
                var currentrgb255 = MazeBuilder.ToRGB255(currentPixel);
                if (hiddenWallConst.AreValid(currentrgb255) && isInWall)
                {
                    if (isInWall)
                        CopyPixel(currentMap, targetMaps[currentLevel - 1], x, y);
                    else
                        continue;
                }
                else
                {
                    CopyPixel(currentMap, targetMaps[currentLevel - 1], x, y);
                } 
            }
        }

        targetMaps[currentLevel - 1].SetPixel(playerX, playerY, Color.red);
        targetMaps[currentLevel - 1].Apply();

        Rect fullRect = new Rect(0, 0, targetMaps[currentLevel - 1].width, targetMaps[currentLevel - 1].height);
        targetSprite = Sprite.Create(targetMaps[currentLevel - 1], fullRect, Vector2.zero);
        GetComponent<Image>().sprite = targetSprite;
    }
}
