using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class MemoryPuzzle : MonoBehaviour
{
    [Header("UI Feedback")]
    public TextMeshProUGUI statusText;

    [Header("Configuration")]
    public List<PuzzleKey> keys;
    public List<AudioClip> noteSounds;
    public AudioClip successSound;
    public AudioClip errorSound;

    [Header("Settings")]
    public int sequenceLength = 3;
    public int puzzleMode = 1; // 1=Normal, 2=Reverse, 3=Audio

    [Header("Debug")]
    public bool overrideSettings = false;
    [Range(1, 3)] public int debugMode = 1;
    [Range(3, 10)] public int debugLength = 3;

    [Header("State")]
    public bool canInteract = false;
    public bool isGameActive = false;
    public bool isSolved = false;

    private List<int> sequence;
    private int inputIndex = 0;
    private AudioSource audioSource;

    List<QuizQuestion> quizQuestions;

    Color defaultColor = new Color(1f, 0.8f, 0f);
    Color wrongColor = new Color(1f, 1f, 1f);
    Color correctColor = Color.green;

    private void Awake()
    {
        quizQuestions = new();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        sequence = new List<int>();
        if (statusText != null) statusText.text = "";

        if (overrideSettings)
        {
            InitPuzzle(debugMode, debugLength);
        }

        canInteract = true;
        isGameActive = false;
        isSolved = false;
    }

    public void InitPuzzle(int mode, int length, int quizQuestionCount = 1)
    {
        if (overrideSettings)
        {
            puzzleMode = debugMode;
            sequenceLength = debugLength;
        }
        else
        {
            puzzleMode = mode;
            sequenceLength = length;
        }

        quizQuestions = GameManager.instance.GetQuizQuestions(quizQuestionCount);
    }

    public void StartPuzzleGame()
    {
        if (isGameActive || isSolved) return;

        isGameActive = true;

        string msg = "MEMORIZE!";

        if (puzzleMode == 2) msg = "REVERSE!";
        if (puzzleMode == 3) msg = "LISTEN!";

        if (statusText != null) statusText.text = msg;

        GenerateSequence();
        StartCoroutine(PlaySequence());
    }

    void GenerateSequence()
    {
        sequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            sequence.Add(Random.Range(0, keys.Count));
        }

        string debugSeq = "Sequence: ";
        foreach (int i in sequence) debugSeq += i + " ";
        Debug.Log(debugSeq);
    }

    IEnumerator PlaySequence()
    {
        canInteract = false;
        yield return new WaitForSeconds(1f);

        if (statusText != null) statusText.text = "";

        foreach (int keyIndex in sequence)
        {
            if (puzzleMode != 3)
            {
                if (keyIndex < keys.Count)
                    StartCoroutine(keys[keyIndex].FlashKey());
            }

            if (keyIndex < noteSounds.Count && noteSounds[keyIndex] != null)
                audioSource.PlayOneShot(noteSounds[keyIndex]);

            yield return new WaitForSeconds(0.8f);
        }

        canInteract = true;
        inputIndex = 0;

        if (statusText != null) statusText.text = "REPEAT!";
    }

    public void OnKeyRead(int id)
    {
        if (!canInteract) return;

        if (id < noteSounds.Count && noteSounds[id] != null)
            audioSource.PlayOneShot(noteSounds[id]);

        if (!isGameActive) return;

        int expectedID = -1;

        if (puzzleMode == 2)
        {
            int reverseIndex = sequence.Count - 1 - inputIndex;
            expectedID = sequence[reverseIndex];
        }
        else
        {
            expectedID = sequence[inputIndex];
        }

        Debug.Log($"Input: {id}. Expected: {expectedID}");

        if (id == expectedID)
        {
            inputIndex++;
            if (inputIndex >= sequence.Count)
            {
                PuzzleSolved();
            }
        }
        else
        {
            StartCoroutine(HandleFailure());
        }
    }

    IEnumerator HandleFailure()
    {
        Debug.Log("Wrong!");
        canInteract = false;

        if (errorSound != null) audioSource.PlayOneShot(errorSound);
        if (statusText != null)
        {
            statusText.text = "WRONG!";
            statusText.color = Color.red;
        }

        yield return new WaitForSeconds(1.5f);

        if (statusText != null) statusText.color = Color.white;
        StartCoroutine(PlaySequence());
    }

    void PuzzleSolved()
    {
        canInteract = false;
        isGameActive = false;
        isSolved = true;

        Debug.Log("PUZZLE SOLVED!");

        if (successSound != null) audioSource.PlayOneShot(successSound);

        if (statusText != null)
        {
            statusText.text = "SOLVED!";
            statusText.color = Color.green;
        }

        QuizHandler.instance.RunQuiz(this, quizQuestions);

        //if (UIHandler.instance != null)
        //{
        //    UIHandler.instance.PuzzleCompleted();
        //    UIHandler.instance.ShowTextPrompt("Secret Note Found!");
        //}
    }

    public void ResetPuzzle()
    {
        // Reset text
        statusText.text = "";
        statusText.color = defaultColor;

        // Reset state
        canInteract = true;
        isGameActive = false;
        isSolved = false;
    }
}