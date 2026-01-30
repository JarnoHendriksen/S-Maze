using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Transform player;

    [SerializeField] int level;

    [SerializeField] Minimap minimap;

    [SerializeField] Sprite itemSprite;

    [SerializeField] public List<Item> items;

    [SerializeField] List<QuizQuestion> questions;

    Queue<QuizQuestion> pendingQuestions = new();

    int puzzlesCompleted = 0;
    int puzzlesInLevel = 0;

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

    public bool LevelCompleted { get; private set; }

    bool isPaused;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        foreach (var q in questions)
        {
            q.answers = new string[4]{ q.answer1, q.answer2, q.answer3, q.answer4};
            pendingQuestions.Enqueue(q);
        }

        for (int i = 0; i < items.Count; i++)
        {
            items[i].id = i;
            items[i].sprite = itemSprite;
        }

        LevelCompleted = false;
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
        SetPaused(true);
    }

    public void TogglePaused(bool togglePausePanel = true) // don't toggle pause panel when showing settings
    {
        GamePaused = !GamePaused;

        if (togglePausePanel) UIHandler.instance.TogglePause();
    }

    public void SetPaused(bool pauseGame, bool togglePausePanel = false)
    {
        GamePaused = pauseGame;

        if (togglePausePanel) UIHandler.instance.SetPause(pauseGame);
    }

    public void PuzzleCompleted()
    {
        UIHandler.instance.ShowTextPrompt("Puzzle Completed!");
        if (AudioSystem.instance != null) AudioSystem.instance.PlaySoundEffect(SoundEffectType.PuzzleCompleted);
        UIHandler.instance.PuzzleCompleted();
        puzzlesCompleted++;

        if (puzzlesCompleted == puzzlesInLevel)
        {
            LevelCompleted = true;
        }
    }

    public IEnumerator LoadNextLevel()
    {
        DataCollector.Instance.LevelUp(Level);
        // Hide game by sliding black screen over it
        UIHandler.instance.HideScreen();
        UIHandler.instance.HideLevelCompletedScreen();

        float waitTime = 1.0f;

        yield return new WaitForSeconds(waitTime);

        // Delete old level in MazeBuilder
        MazeBuilder.instance.DeleteMaze();

        Level++;

        // Build next maze
        yield return MazeBuilder.instance.GenerateMaze(Level);

        if (AudioSystem.instance != null) AudioSystem.instance.PlayMusicForLevel(Level);

        puzzlesInLevel = MazeBuilder.instance.RoomCount;

        // Reset UI elements
        UIHandler.instance.ResetUI();
        UIHandler.instance.SetLevel(Level);
        UIHandler.instance.SetPuzzleCount(puzzlesInLevel);

        minimap.SetLevel(Level);

        // Slide black screen out of view
        UIHandler.instance.ShowScreen();

        LevelCompleted = false;
        puzzlesCompleted = 0;

        yield return null;
    }

    public List<QuizQuestion> GetQuizQuestions(int count)
    {
        if (pendingQuestions.Count < count)
        {
            Debug.LogWarning("Too many questions requested!");
        }

        List<QuizQuestion> qs = new();

        while (qs.Count < count)
        {
            var q = pendingQuestions.Dequeue();
            qs.Add(q);
            pendingQuestions.Enqueue(q);
        }

        return qs;
    }

    public List<QuizQuestion> LookaheadQuestions(int count)
    {
        if (pendingQuestions.Count < count)
        {
            Debug.LogWarning("Too many questions requested!");
        }

        List<QuizQuestion> qs = pendingQuestions.Take(count).ToList();

        return qs;
    }
}



[System.Serializable]
public class Item
{
    [HideInInspector] public int id;
    [HideInInspector] public Sprite sprite;
    [TextArea(3,5)] public string value;
}

[System.Serializable]
public class QuizQuestion
{
    public string question;
    [Tooltip("Index of the correct answer (0-3)")]
    [Range(0, 3)] public int correctAnswer;
    public string answer1;
    public string answer2;
    public string answer3;
    public string answer4;

    [Tooltip("Indices of notes that are related to this question.")]
    public int[] relevantInfoIds;

    [HideInInspector] public string[] answers;
}
