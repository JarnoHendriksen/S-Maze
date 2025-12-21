using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System;
using System.Linq;

public class MazeBuilder : MonoBehaviour
{
    [SerializeField] GameObject wallCell, floorCell, doorCell, fakeWallCell, itemPlaceholder;
    [SerializeField] Transform wallObjects, fakeWallObjects, floorObjects, itemObjects;
    [SerializeField] Transform player;
    [SerializeField] Texture2D mazeLayout;

    float cellSizeInUnits;

    PlayerController playerCtrl;
    bool fakeCorridorsVisible = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Sprite wallSprite = wallCell.GetComponent<SpriteRenderer>().sprite;
        cellSizeInUnits = wallSprite.bounds.size.x;

        Img2Map();

        fakeWallObjects.GetComponent<CompositeCollider2D>().GenerateGeometry();

        playerCtrl = player.GetComponent<PlayerController>();
    }

    private void Update()
    {
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

    void Img2Map()
    {
        int w = mazeLayout.width;
        int h = mazeLayout.height;

        Transform newCell;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                Color pixel = mazeLayout.GetPixel(x, y);
                if (pixel == Color.black)
                {
                    newCell = CreateCell(x, y, wallCell);
                    newCell.SetParent(wallObjects);
                    continue;
                }
                else
                {
                    newCell = CreateCell(x, y, floorCell);
                    newCell.SetParent(floorObjects);
                }

                PixelData pd = new PixelData(x, y, pixel);
                ColorSet validDoorOrientations = new ColorSet(0x00, 0x19, 0x32, 0x4b);

                bool success;
                success = ParsePlayer(pd, new ColorConstraints(new ExactColor(0xff), new ExactColor(0x00), new ExactColor(0x00)));
                if (success) continue;
                success = ParseDoor(pd, new ColorConstraints(new ExactColor(0xff), new ExactColor(0xff), validDoorOrientations));
                if (success) continue;
                success = ParseObject(pd, new ColorConstraints(new ExactColor(0x00), new ExactColor(0x00), new ExactColor(0xff)));
                if (success) continue;
                success = ParseHiddenPath(pd, new ColorConstraints(new ExactColor(0x50), new ExactColor(0x50), new ExactColor(0x50)));
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
        door.SetParent(itemObjects);
        float rotation = b / 100f;
        Debug.Log(rotation);
        door.eulerAngles = new Vector3(0, 0, -360f * rotation);

        return true;
    }

    bool ParseObject(PixelData pixel, ColorConstraints cc)
    {
        (byte r, byte g, byte b) = ToRGB255(pixel.color);
        if (!cc.red.IsValid(r) || !cc.green.IsValid(g) || !cc.blue.IsValid(b)) return false;

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

    public override bool IsValid(byte v)
    {
        return v >= min || v <= max;
    }
}