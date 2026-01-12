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
    public int difficultyLevel = 3;

    [Header("State")]
    public bool canInteract = false;
    public bool isGameActive = false;
    public bool isSolved = false;

    private List<int> sequence;
    private int inputIndex = 0;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        sequence = new List<int>();
        if (statusText != null) statusText.text = "";

        canInteract = true;
        isGameActive = false;
        isSolved = false;
    }

    public void StartPuzzleGame()
    {
        if (isGameActive || isSolved) return;

        isGameActive = true;
        if (statusText != null) statusText.text = "MEMORIZE!";

        GenerateSequence();
        StartCoroutine(PlaySequence());
    }

    void GenerateSequence()
    {
        sequence.Clear();
        for (int i = 0; i < difficultyLevel; i++)
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
            if (keyIndex < keys.Count)
                StartCoroutine(keys[keyIndex].FlashKey());

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

        Debug.Log($"Input Received: {id}. Expected: {sequence[inputIndex]}");

        if (id == sequence[inputIndex])
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
    }
}