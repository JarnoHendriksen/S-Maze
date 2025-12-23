using System.Collections;
using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    public static UIHandler instance;

    [Header("Fixed UI elements")]
    [SerializeField] TextMeshProUGUI levelHeader;
    [SerializeField] Transform progressContainer;

    [Header("Dynamic UI symbols")]
    [SerializeField] Transform puzzlePending;
    [SerializeField] Transform puzzleCompleted;

    [Header("Menu Panels")]
    [SerializeField] Transform gamePaused;
    [SerializeField] Transform settingsMenu;
    [SerializeField] Transform controlsInfo;

    [Header("")]
    [SerializeField] float animationDuration;

    float pausePanelY = 780;
    float settingsPanelY = 200;
    float controlsPanelY = 200;
    float hiddenY = 1100;

    bool isPanelMoving = false;
    bool isPausePanelOpen = false;
    bool isSettingsOpen = false;

    int puzzleCount = 0;
    int puzzlesCompleted = 0;

    int[] puzzleSymbols;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
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

    IEnumerator SlidePanel(Transform panel, float targetYPos, float duration)
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

    public void PauseBtnClick()
    {
        if (isSettingsOpen) return; // Pause already controlled by settings menu

        GameManager.instance.TogglePaused();
    }

    public void SettingsBtnClick()
    {
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

    public void ControlsPanelBtnClick()
    {
        GameManager.instance.TogglePaused(false);
        StartCoroutine(SlidePanel(controlsInfo, hiddenY, animationDuration));
    }
}
