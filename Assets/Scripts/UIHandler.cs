using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

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
    [SerializeField] Transform settingsMenu;
    [SerializeField] Transform controlsInfo;
    [SerializeField] Transform levelCompleted;

    [Header("")]
    [SerializeField] float animationDuration;

    float pausePanelY = 780;
    float settingsPanelY = 200;
    float controlsPanelY = 200;
    float levelCompletedY = 300;
    float hiddenY = 1100;

    bool isPanelMoving = false;
    bool isPausePanelOpen = false;
    bool isSettingsOpen = false;
    bool isBlackScreenVisible = true;
    bool isTransitioning = false;
    bool isTextPromptVisible = false;

    int puzzleCount = 0;
    int puzzlesCompleted = 0;

    int[] puzzleSymbols;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //if (isBlackScreenVisible && !isTransitioning) ShowScreen(1.0f);
    }

    public void SetLevel(int lvl)
    {
        levelHeader.text = "Level " + lvl;
    }

    public void SetPuzzleCount(int count)
    {
        puzzleCount = count;
        puzzleSymbols = new int[count];

        for (int i = 0;  i < puzzleCount; i++)
        {
            Transform newSymbol = Instantiate(puzzlePending);
            newSymbol.SetParent(progressContainer);
            puzzleSymbols[i] = 0;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(progressContainer.GetComponent<RectTransform>());
    }

    public void PuzzleCompleted()
    {
        if (puzzlesCompleted <  puzzleCount)
        {
            // Delete all children of the progress bar
            for (int i = progressContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(progressContainer.GetChild(i).gameObject);
            }

            // Replace symbol in array
            puzzleSymbols[puzzlesCompleted] = 1;
            puzzlesCompleted++;

            // Rebuild progress bar with updated array
            for (int i = 0; i < puzzleCount; i++)
            {
                Transform symbol;
                if (puzzleSymbols[i] == 0)
                    symbol = Instantiate(puzzlePending);
                else
                    symbol = Instantiate(puzzleCompleted);

                symbol.SetParent(progressContainer);
            }
        }
        else
        {
            Debug.LogWarning("No more puzzles to complete!");
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

    public void ShowLevelCompletedScreen()
    {
        GameManager.instance.TogglePaused(false);

        if (GameManager.instance.Level < 3)
            levelCompleted.Find("NextLevelBtn/Text").GetComponent<TextMeshProUGUI>().text =
                "To Level " + GameManager.instance.Level + 1;
        else
            levelCompleted.Find("NextLevelBtn/Text").GetComponent<TextMeshProUGUI>().text =
                "To Main Menu";

        StartCoroutine(SlidePanel(levelCompleted, levelCompletedY, animationDuration));
    }

    public void HideLevelCompletedScreen()
    {
        GameManager.instance.TogglePaused(false);
        levelCompleted.position = new Vector3(levelCompleted.position.x, hiddenY);
    }

    public void ShowTextPrompt(string msg)
    {
        textPrompt.text = msg;
        textPromptContainer.gameObject.SetActive(true);
        float visibleTime = msg.Split(' ').Length / 3.333f; // Assuming a reading speed of 200 WPM / 3.33.. WPS
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

        for ( int i = progressContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(progressContainer.GetChild(i).gameObject);
        }
    }

    // // BUTTON CALLBACKS // //

    public void PauseBtnClick(bool playSound = true)
    {
        if (isSettingsOpen) return; // Pause already controlled by settings menu

        if (playSound) AudioSystem.instance.PlaySoundEffect(SoundEffectType.UI_BtnPressed);

        GameManager.instance.TogglePaused();
    }

    public void SettingsBtnClick(bool playSound = true)
    {
        if (playSound) AudioSystem.instance.PlaySoundEffect(SoundEffectType.UI_BtnPressed);

        // Don't unpause if already paused when clicking settings and vice versa
        if ((!GameManager.instance.GamePaused && !isSettingsOpen)
            || (GameManager.instance.GamePaused && isSettingsOpen)) GameManager.instance.TogglePaused(false);

        if (isSettingsOpen)
        {
            StartCoroutine(SlidePanel(settingsMenu, hiddenY, animationDuration));
            isSettingsOpen = false;
        }
        else
        {
            if (isPausePanelOpen) TogglePause(); // Move pause panel away before showing settings
            StartCoroutine(SlidePanel(settingsMenu, settingsPanelY, animationDuration));
            isSettingsOpen = true;
        }
    }

    public void ControlsPanelBtnClick(bool playSound = true)
    {
        //if (playSound) AudioSystem.instance.PlaySoundEffect(SoundEffectType.UI_BtnPressed);
        GameManager.instance.TogglePaused(false);

        StartCoroutine(SlidePanel(controlsInfo, hiddenY, animationDuration));
    }

    public void NextLevelBtnClick()
    {
        if (GameManager.instance.Level < 3)
            StartCoroutine(GameManager.instance.LoadNextLevel());
        else
        {
            AudioSystem.instance.PlayMusicForLevel(1);
            SceneManager.LoadScene("MainMenu");
        }
    }

    // // COROUTINES // //

    public IEnumerator SlidePanel(Transform panel, float targetYPos, float duration)
    {
        // Wait for any panels to finish moving
        while (isPanelMoving) yield return null;

        isPanelMoving = true;
        float currentTime = 0;

        Vector3 targetPos = new Vector3(panel.position.x, targetYPos, panel.position.z);

        while (currentTime < duration)
        {
            panel.position = Vector3.Lerp(panel.position, targetPos, currentTime);
            currentTime += Time.unscaledDeltaTime; // Time.deltaTime won't work if Time.timeScale = 0 when game is paused
            yield return null;
        }

        panel.position = targetPos;

        isPanelMoving = false;
        yield return null;
    }

    IEnumerator FadeBlack(float fadeTime, float waitTime = 0.0f, bool toBlack = true)
    {
        isTransitioning = true;

        float currentTime = 0.0f;

        while(currentTime < waitTime)
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

        yield return null;
    }

    IEnumerator ShowPrompt(float animTime, float visibleTime)
    {
        // Wait for last prompt to finish
        while (isTextPromptVisible) yield return null;

        isTextPromptVisible = true;

        Vector3 visiblePos = new Vector3(460, 150);
        Vector3 hiddenPos = new Vector3(460, -150);

        float currentTime = 0;

        // Slide + fade in text prompt
        while(currentTime < animTime)
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

        // wait for visibleTime seconds
        yield return new WaitForSeconds(visibleTime);

        // Slide and fade out text prompt
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

        yield return null;
    }
}
