using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
        // TODO: look for savefile and load data

        // If no savefile, start with level 1 (Set to 0 because LoadNextLevel increments it)
        Level = 0;

        MazeBuilder.instance.Init();
        StartCoroutine(LoadNextLevel());

        // Start game paused, since control menu is showing
        TogglePaused(false);
    }

    public void TogglePaused(bool togglePausePanel = true) // don't toggle pause panel when showing settings
    {
        GamePaused = !GamePaused;

        if (togglePausePanel) UIHandler.instance.TogglePause();
    }

    public IEnumerator LoadNextLevel()
    {
        // Hide game by sliding black screen over it
        UIHandler.instance.HideScreen();

        float waitTime = 1.0f;

        yield return new WaitForSeconds(waitTime);

        // Delete old level in MazeBuilder
        MazeBuilder.instance.DeleteMaze();

        Level++;

        // Build next maze
        yield return MazeBuilder.instance.GenerateMaze(Level);

        // Reset UI elements
        UIHandler.instance.ResetUI();
        UIHandler.instance.SetLevel(Level);
        UIHandler.instance.SetPuzzleCount(MazeBuilder.instance.QuestCount); // TODO: attach quests to rooms instead of mazebuilder

        // Slide black screen out of view
        UIHandler.instance.ShowScreen();

        yield return null;
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
