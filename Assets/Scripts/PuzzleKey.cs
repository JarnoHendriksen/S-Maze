using UnityEngine;
using System.Collections;

public class PuzzleKey : MonoBehaviour
{
    public int id;
    public MemoryPuzzle puzzleController;

    private SpriteRenderer sr;
    private Color originalColor;
    public Color flashColor = Color.grey;
    private bool isPressed = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPressed)
        {
            isPressed = true; // Lock the key

            StartCoroutine(FlashKey());

            if (puzzleController != null)
            {
                puzzleController.OnKeyRead(id);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPressed = false; // Reset the lock
        }
    }

    public IEnumerator FlashKey()
    {
        sr.color = flashColor;
        yield return new WaitForSeconds(0.3f);
        sr.color = originalColor;
    }
}