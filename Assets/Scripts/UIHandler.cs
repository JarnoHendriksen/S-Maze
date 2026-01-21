using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class UIHandler : MonoBehaviour
{
    public static UIHandler instance;

    [Header("Fixed UI elements")]
    [SerializeField] TextMeshProUGUI levelHeader;
    [SerializeField] Transform progressContainer;
    [SerializeField] Transform blackScreen;

    [Header("Dynamic UI elements")]
    [SerializeField] Transform puzzlePending;
    [SerializeField] Transform puzzleCompleted;
    [SerializeField] Transform textPromptContainer;
    [SerializeField] TextMeshProUGUI textPrompt;

    [Header("Menu Panels")]
    [SerializeField] Transform gamePaused;
    [SerializeField] Transform controlsInfo;
    [SerializeField] Transform levelCompleted;
    [SerializeField] Transform quizPanel;
    [SerializeField] Transform hintsPanel;

    [Header("Settings")]
    [SerializeField] float animationDuration = 0.5f;

    float panelActiveY = 200;
    float pausePanelY = 780;
    float levelCompletedY = 300;
    float hintActiveY = 400;
    float hiddenY = 2500; // Increased to ensure it's off-screen

    bool isPanelMoving = false;
    bool isPausePanelOpen = false;
    bool isControlsPanelOpen = true; // Starts open
    bool isQuizPanelOpen = false;
    bool isHintsPanelOpen = false;
    bool isLevelCompletedPanelOpen = false;
    bool isBlackScreenVisible = true;
    bool isTransitioning = false;
    bool isTextPromptVisible = false;

    int puzzleCount = 0;
    int puzzlesCompleted = 0;
    int[] puzzleSymbols;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // PRESS ESC: Toggle the Initial Tutorial / Controls Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleControlsMenu();
        }
    }

    public void ToggleControlsMenu()
    {
        if (isPanelMoving) return;

        if (isControlsPanelOpen)
        {
            ControlsPanelBtnClick(true);
        }
        else
        {
            GameManager.instance.SetPaused(true);
            StartCoroutine(SlidePanel(controlsInfo, panelActiveY, animationDuration));
            isControlsPanelOpen = true;
        }
    }

    public void SetLevel(int lvl)
    {
        levelHeader.text = "Level " + lvl;
    }

    public void SetPuzzleCount(int count)
    {
        puzzleCount = count;
        puzzleSymbols = new int[count];

        for (int i = 0; i < puzzleCount; i++)
        {
            Transform newSymbol = Instantiate(puzzlePending);
            newSymbol.SetParent(progressContainer);
            puzzleSymbols[i] = 0;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(progressContainer.GetComponent<RectTransform>());
    }

    public void PuzzleCompleted()
    {
        if (puzzlesCompleted < puzzleCount)
        {
            for (int i = progressContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(progressContainer.GetChild(i).gameObject);
            }

            puzzleSymbols[puzzlesCompleted] = 1;
            puzzlesCompleted++;

            for (int i = 0; i < puzzleCount; i++)
            {
                Transform symbol;
                if (puzzleSymbols[i] == 0) symbol = Instantiate(puzzlePending);
                else symbol = Instantiate(puzzleCompleted);

                symbol.SetParent(progressContainer);
            }
        }
    }

    public void TogglePause()
    {
        if (!isPausePanelOpen)
        {
            StartCoroutine(SlidePanel(gamePaused, pausePanelY, animationDuration));
            isPausePanelOpen = true;
        }
        else
        {
            StartCoroutine(SlidePanel(gamePaused, hiddenY, animationDuration));
            isPausePanelOpen = false;
        }
    }

    public void SetPause(bool pauseGame)
    {
        if (!isPausePanelOpen && pauseGame)
            StartCoroutine(SlidePanel(gamePaused, pausePanelY, animationDuration));
        else if (isPausePanelOpen && !pauseGame)
            StartCoroutine(SlidePanel(gamePaused, hiddenY, animationDuration));

        isPausePanelOpen = pauseGame;
    }

    // // PANEL MANAGEMENT // //
    public void ShowQuizPanel()
    {
        if (isQuizPanelOpen) return;
        isQuizPanelOpen = true;
        GameManager.instance.SetPaused(true);
        StartCoroutine(UIHandler.instance.SlidePanel(quizPanel, panelActiveY, animationDuration));
    }

    public void HideQuizPanel()
    {
        if (!isQuizPanelOpen) return;
        isQuizPanelOpen = false;
        GameManager.instance.SetPaused(false);
        StartCoroutine(UIHandler.instance.SlidePanel(quizPanel, hiddenY, animationDuration));
    }

    public void ShowHint(string text)
    {
        if (isHintsPanelOpen) return;
        isHintsPanelOpen = true;
        GameManager.instance.SetPaused(true);
        hintsPanel.Find("Hint").GetComponent<TextMeshProUGUI>().text = text;
        StartCoroutine(UIHandler.instance.SlidePanel(hintsPanel, hintActiveY, animationDuration));
    }

    public void HideHint()
    {
        if (!isHintsPanelOpen) return;
        isHintsPanelOpen = false;
        GameManager.instance.SetPaused(false);
        StartCoroutine(UIHandler.instance.SlidePanel(hintsPanel, hiddenY, animationDuration));
    }

    public void ShowLevelCompletedScreen()
    {
        GameManager.instance.SetPaused(true);

        if (GameManager.instance.Level < 3)
            levelCompleted.Find("NextLevelBtn/Text").GetComponent<TextMeshProUGUI>().text = "To Level " + (GameManager.instance.Level + 1);
        else
            levelCompleted.Find("NextLevelBtn/Text").GetComponent<TextMeshProUGUI>().text = "To Main Menu";

        StartCoroutine(SlidePanel(levelCompleted, levelCompletedY, animationDuration));
    }

    public void HideLevelCompletedScreen()
    {
        GameManager.instance.SetPaused(false);
        levelCompleted.position = new Vector3(levelCompleted.position.x, hiddenY);
    }

    public void ShowTextPrompt(string msg)
    {
        textPrompt.text = msg;
        textPromptContainer.gameObject.SetActive(true);
        float visibleTime = msg.Split(' ').Length / 3.333f;
        StartCoroutine(ShowPrompt(animationDuration, visibleTime));
    }

    public void ShowScreen(float waitTime = 0.0f)
    {
        if (!isBlackScreenVisible) return;
        StartCoroutine(FadeBlack(animationDuration, waitTime, false));
    }

    public void HideScreen()
    {
        if (isBlackScreenVisible) return;
        StartCoroutine(FadeBlack(animationDuration));
    }

    public void ResetUI()
    {
        puzzleCount = 0;
        puzzleSymbols = null;
        puzzlesCompleted = 0;
        for (int i = progressContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(progressContainer.GetChild(i).gameObject);
        }
    }

    // // BUTTON CALLBACKS // //

    public void PauseBtnClick(bool playSound = true)
    {
        if (isBlackScreenVisible
            || isControlsPanelOpen
            || isQuizPanelOpen
            || isLevelCompletedPanelOpen)
            return;

        if (playSound && AudioSystem.instance != null) AudioSystem.instance.PlaySoundEffect(SoundEffectType.UI_BtnPressed);

        GameManager.instance.TogglePaused();
    }

    public void ToMenuBtnClick()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ControlsPanelBtnClick(bool playSound = true)
    {
        if (playSound && AudioSystem.instance != null)
        {
            AudioSystem.instance.PlaySoundEffect(SoundEffectType.UI_BtnPressed);
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.SetPaused(false);
        }

        if (controlsInfo != null)
        {
            StartCoroutine(SlidePanel(controlsInfo, hiddenY, animationDuration));
            isControlsPanelOpen = false;
        }
        else
        {
            Debug.LogError("UIHandler Error: 'Controls Info' is missing! Drag the panel into the UIHandler slot in the Inspector.");
        }
    }

    public void HintBtnClick()
    {
        HideHint();
    }

    public void NextLevelBtnClick()
    {
        if (GameManager.instance.Level < 3)
            StartCoroutine(GameManager.instance.LoadNextLevel());
        else
        {
            if (AudioSystem.instance != null) AudioSystem.instance.PlayMusicForLevel(1);
            SceneManager.LoadScene("MainMenu");
        }
    }

    // // COROUTINES // //

    public IEnumerator SlidePanel(Transform panel, float targetYPos, float duration)
    {
        // Wait for previous move to finish
        while (isPanelMoving) yield return null;

        isPanelMoving = true;
        float currentTime = 0;

        // Capture Start Position BEFORE loop
        Vector3 startPos = panel.position;
        Vector3 targetPos = new Vector3(startPos.x, targetYPos, startPos.z);

        while (currentTime < duration)
        {
            // Use percentage (t) for smooth Lerp
            float t = currentTime / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            panel.position = Vector3.Lerp(startPos, targetPos, t);

            currentTime += Time.unscaledDeltaTime;
            yield return null;
        }

        panel.position = targetPos;
        isPanelMoving = false;
    }

    IEnumerator FadeBlack(float fadeTime, float waitTime = 0.0f, bool toBlack = true)
    {
        isTransitioning = true;
        float currentTime = 0.0f;

        while (currentTime < waitTime)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }

        currentTime = 0;
        if (toBlack) blackScreen.gameObject.SetActive(true);

        while (currentTime < fadeTime)
        {
            float t = currentTime / fadeTime;
            float fade_t = t * t * t * t * t;

            Color c = blackScreen.GetComponent<Image>().color;
            blackScreen.GetComponent<Image>().color = new Color(c.r, c.g, c.b, toBlack ? fade_t : 1.0f - fade_t);

            currentTime += Time.deltaTime;
            yield return null;
        }

        Color c_ = blackScreen.GetComponent<Image>().color;
        blackScreen.GetComponent<Image>().color = new Color(c_.r, c_.g, c_.b, toBlack ? 1.0f : 0.0f);

        if (!toBlack) blackScreen.gameObject.SetActive(false);

        isTransitioning = false;
        isBlackScreenVisible = toBlack;
    }

    IEnumerator ShowPrompt(float animTime, float visibleTime)
    {
        while (isTextPromptVisible) yield return null;
        isTextPromptVisible = true;

        Vector3 visiblePos = new Vector3(460, 150);
        Vector3 hiddenPos = new Vector3(460, -150);
        float currentTime = 0;

        while (currentTime < animTime)
        {
            float t = currentTime / animTime;
            float eased_t = MenuAnimationController.Easing(EasingFunc.EaseInOutQuad, t);
            textPrompt.transform.position = Vector3.Lerp(hiddenPos, visiblePos, eased_t);
            textPromptContainer.GetComponent<CanvasGroup>().alpha = eased_t;
            currentTime += Time.deltaTime;
            yield return null;
        }

        textPrompt.transform.position = visiblePos;
        textPromptContainer.GetComponent<CanvasGroup>().alpha = 1.0f;

        yield return new WaitForSeconds(visibleTime);

        currentTime = 0;
        while (currentTime < animTime)
        {
            float t = currentTime / animTime;
            float eased_t = MenuAnimationController.Easing(EasingFunc.EaseInOutQuad, t);
            textPrompt.transform.position = Vector3.Lerp(visiblePos, hiddenPos, eased_t);
            textPromptContainer.GetComponent<CanvasGroup>().alpha = 1.0f - eased_t;
            currentTime += Time.deltaTime;
            yield return null;
        }

        textPrompt.transform.position = hiddenPos;
        textPromptContainer.GetComponent<CanvasGroup>().alpha = 0.0f;
        textPromptContainer.gameObject.SetActive(false);
        isTextPromptVisible = false;
    }
}

public enum Panel
{
    Pause,
    Controls,
    Quiz,
    Hint,
    LevelCompleted
};