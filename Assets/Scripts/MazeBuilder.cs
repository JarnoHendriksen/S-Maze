using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System;

public class MazeBuilder : MonoBehaviour
{
    [SerializeField] GameObject wallCell, floorCell, doorCell;
    [SerializeField] Transform wallObjects, floorObjects, itemObjects;
    [SerializeField] Transform player;
    [SerializeField] Texture2D mazeLayout;

    float cellSizeInUnits;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Sprite wallSprite = wallCell.GetComponent<SpriteRenderer>().sprite;
        cellSizeInUnits = wallSprite.bounds.size.x;

        Img2Map();
    }

    void Img2Map()
    {
        int w = mazeLayout.width;
        int h = mazeLayout.height;

        Transform newCell;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0;y < h; y++)
            {
                Color pixel = mazeLayout.GetPixel(x, y);
                if (pixel == Color.black)
                {
                    newCell = CreateCell(x, y, wallCell);
                    newCell.SetParent(wallObjects);
                }
                else
                {
                    newCell = CreateCell(x, y, floorCell);
                    newCell.SetParent(floorObjects);

                    if (pixel == FromRGB255(0xff, 0, 0))
                        player.position = new Vector3(x * cellSizeInUnits, y * cellSizeInUnits, 0);
                    else if (pixel.r == pixel.g && pixel.g == 1f && pixel.b < 1f)
                    {
                        Transform door = CreateCell(x, y, doorCell);
                        door.SetParent(itemObjects);
                        door.eulerAngles = new Vector3(0, 0, -360f * pixel.b);
                    }
                }
            }
        }
    }

    Transform CreateCell(int x, int y, GameObject cellType)
    {
        var newCell = Instantiate(cellType);
        newCell.transform.position = new Vector3(x * cellSizeInUnits, y * cellSizeInUnits, 0);

        return newCell.transform;
    }

    public static (byte, byte, byte) ToRGB255(Color c)
    {
        byte r = (byte)(c.r * 255);
        byte g = (byte)(c.g * 255);
        byte b = (byte)(c.b * 255);

        return (r, g, b);
    }

    public static Color FromRGB255(byte r, byte g, byte b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }
}