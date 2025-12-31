using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Transform player;

    [SerializeField] int level;

    [SerializeField] public List<Item> items;

    public int Level
    {
        get
        {
            return level;
        }

        set
        {
            UIHandler.instance.SetLevel(value);
            level = value;
        }
    }

    public bool GamePaused
    {
        get
        {
            return isPaused;
        }
        set
        {
            isPaused = value;
        }
    }

    bool isPaused;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIHandler.instance.SetLevel(level);

        UIHandler.instance.SetPuzzleCount(MazeBuilder.instance.QuestCount);

        // Start game paused, since control menu is showing
        TogglePaused(false);
    }

    public void TogglePaused(bool togglePausePanel = true) // don't toggle pause panel when showing settings
    {
        GamePaused = !GamePaused;

        if (togglePausePanel) UIHandler.instance.TogglePause();
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
