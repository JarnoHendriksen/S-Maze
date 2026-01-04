using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.PlayerLoop;

public class MenuUIHandler : MonoBehaviour
{
    bool playBtnPressed = false;
    bool isAnimating = false;
    bool animationFinished = false;

    [SerializeField] Transform uiElements;
    [SerializeField] Transform logo;
    [SerializeField] Transform notes;
    [SerializeField] Transform blackScreen;

    [SerializeField] float animationTime;

    public void StartGame()
    {
        playBtnPressed = true;

    }

    public void OpenSettings()
    {
        // MainMenuUIHandler.instance.OpenSettings();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (playBtnPressed)
        {
            if (!isAnimating && !animationFinished) StartCoroutine(StartGameAnimation(animationTime));
            if (!isAnimating && animationFinished) SceneManager.LoadScene("S-Maze");
        }
    }

    IEnumerator StartGameAnimation(float duration)
    {
        isAnimating = true;
        float currentTime = 0.0f;

        blackScreen.gameObject.SetActive(true);

        Vector3 uiElementsStartPos = uiElements.position;
        Vector3 logoStartPos = logo.position;

        Vector3 uiElementsEndPos = uiElementsStartPos - new Vector3(250, 0);
        Vector3 logoEndPos = logoStartPos - new Vector3(250, 0);

        while (currentTime < duration)
        {
            float t = currentTime / duration;

            // Slide menu elements to the right and fade out
            float slideUI_t = EaseInOutCubic(t);
            uiElements.transform.position = Vector3.Lerp(uiElementsStartPos, uiElementsEndPos, slideUI_t);
            logo.transform.position = Vector3.Lerp(logoStartPos, logoEndPos, slideUI_t);

            float fadeUI_t = 1.0f - ((1f-t) * (1f-t)); // (EaseOutQuad)
            uiElements.GetComponent<CanvasGroup>().alpha = 1.0f - fadeUI_t;
            Color c = logo.GetComponent<Image>().color;
            logo.GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1.0f - fadeUI_t);

            // Slow down notes in background and fade out
            float notes_t = t; // (linear)
            MenuAnimationController.instance.speedMultiplier = 1.0f - notes_t;
            notes.GetComponent<CanvasGroup>().alpha = 1.0f - notes_t;

            // Fade screen to black
            float fadeBlack_t = t * t * t * t * t; // (EaseInQuint)
            c = blackScreen.GetComponent<Image>().color;
            blackScreen.GetComponent<Image>().color = new Color(c.r, c.g, c.b, fadeBlack_t);

            currentTime += Time.deltaTime;

            yield return null;
        }

        isAnimating = false;
        animationFinished = true;

        yield return null;
    }

    float EaseInOutCubic(float x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
    }
}
