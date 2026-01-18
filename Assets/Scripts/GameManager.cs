//using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Transform player;

    [SerializeField] int level;

    [SerializeField] public List<Item> items;

    [SerializeField] List<QuizQuestion> questions;

    List<QuizQuestion> undistributedQuestions;

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

        undistributedQuestions = new();

        foreach (var q in questions)
        {
            q.answers = new string[4]{ q.answer1, q.answer2, q.answer3, q.answer4};
            undistributedQuestions.Add(q);
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
        AudioSystem.instance.PlaySoundEffect(SoundEffectType.PuzzleCompleted);
        UIHandler.instance.PuzzleCompleted();
        puzzlesCompleted++;

        if (puzzlesCompleted == puzzlesInLevel)
        {
            LevelCompleted = true;
        }
    }

    public IEnumerator LoadNextLevel()
    {
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

        puzzlesInLevel = MazeBuilder.instance.RoomCount;

        // Reset UI elements
        UIHandler.instance.ResetUI();
        UIHandler.instance.SetLevel(Level);
        UIHandler.instance.SetPuzzleCount(puzzlesInLevel);

        // Slide black screen out of view
        UIHandler.instance.ShowScreen();

        LevelCompleted = false;

        yield return null;
    }
    public List<QuizQuestion> GetQuizQuestions(int count)
    {
        if (undistributedQuestions.Count < count)
        {
            int missing = count - undistributedQuestions.Count;
            Debug.LogWarning($"Not enough questions declared! Reusing {missing} questions.");

            HashSet<int> randIdx = new();

            while(randIdx.Count < missing)
            {
                randIdx.Add(Random.Range(0, questions.Count));
            }

            List<QuizQuestion> randQs = new();

            foreach (int i in randIdx)
            {
                randQs.Add(questions[i]);
            }

            randQs.Concat(undistributedQuestions).ToList();

            return randQs;
        }

        List<QuizQuestion> qs = undistributedQuestions.Take(count).ToList();
        undistributedQuestions.RemoveRange(0, qs.Count);

        return qs;
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

    [HideInInspector] public string[] answers;
}
