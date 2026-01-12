using UnityEngine;

public class PuzzleStartButton : MonoBehaviour
{
    public MemoryPuzzle puzzleController;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = Color.yellow;
    }

    void OnMouseDown()
    {
        if (puzzleController != null && !puzzleController.isGameActive && !puzzleController.isSolved)
        {
            StartCoroutine(FlashButton());
            puzzleController.StartPuzzleGame();
        }
    }

    System.Collections.IEnumerator FlashButton()
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        sr.color = Color.yellow;
    }
}