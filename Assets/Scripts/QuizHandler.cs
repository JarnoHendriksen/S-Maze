using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class QuizHandler : MonoBehaviour
{
    public static QuizHandler instance;

    [SerializeField] TextMeshProUGUI question;
    [SerializeField] Transform answerContainer;

    Queue<QuizQuestion> pendingQuestions;

    QuizQuestion currentQuestion;

    MemoryPuzzle attachedPuzzle;

    int maxQCount = 5;

    int points = 0;
    int askedQuestions = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        pendingQuestions = new Queue<QuizQuestion>(maxQCount);
        currentQuestion = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunQuiz(MemoryPuzzle puzzle, List<QuizQuestion> qs)
    {
        attachedPuzzle = puzzle;

        foreach (var q in qs)
            pendingQuestions.Enqueue(q);

        maxQCount = qs.Count;

        if (pendingQuestions.Count <= 0)
            return;

        // Fill quiz panel with the first question
        SetupNextQuestion();


        // Show the quiz panel
        // StartCoroutine(UIHandler.instance.SlidePanel(transform, 200, 0.5f));
        UIHandler.instance.ShowQuizPanel();
        
    }

    void SetButtonsInteractable(bool interactable)
    {
        for(int i = 0; i < answerContainer.childCount; i++)
        {
            answerContainer.GetChild(i).GetComponent<Button>().interactable = interactable; ;
        }
    }

    public void AnswerBtnPressed(string answer)
    {
        // TODO: Test with and without this vvv
        SetButtonsInteractable(false);
        Debug.Log("Answer Given: " + answer);
        // If answer equals the correct answer, add point
        string givenAnswer = answer.Trim().ToLower();
        string correctAnswer = currentQuestion.answers[currentQuestion.correctAnswer];
        correctAnswer = correctAnswer.Trim().ToLower();

        Debug.Log($"Correct/Given Answer: {correctAnswer}/{givenAnswer}");

        DataCollector.Instance.AddAttempt();

        if (givenAnswer == correctAnswer)
        {
            UIHandler.instance.ShowTextPrompt("Correct Answer!");
            if (AudioSystem.instance != null) AudioSystem.instance.PlaySoundEffect(SoundEffectType.PuzzleCompleted);
            points++;

            DataCollector.Instance.LogQuizAttempts(currentQuestion.question);
        }
        else
        {
            UIHandler.instance.ShowTextPrompt("Wrong Answer...");
            if (AudioSystem.instance != null) AudioSystem.instance.PlaySoundEffect(SoundEffectType.InvalidAction);
            
        }

        if (pendingQuestions.Count <= 0)
        {
            if (points == maxQCount)
            {
                GameManager.instance.PuzzleCompleted();
            }
            else
            {
                UIHandler.instance.ShowTextPrompt("You need to answer all questions correctly to complete the puzzle!");
                if (AudioSystem.instance != null) AudioSystem.instance.PlaySoundEffect(SoundEffectType.InvalidAction);
                attachedPuzzle.ResetPuzzle();
            }
        }
        
        StartCoroutine(HandlePanel());
    }

    IEnumerator HandlePanel()
    {
        yield return new WaitForSeconds(1.5f);

        

        if (pendingQuestions.Count > 0)
        {
            SetupNextQuestion();
            SetButtonsInteractable(true);
            yield break;
        }
            
        else
        {
            ResetQuiz();
            UIHandler.instance.HideQuizPanel();
        }

        SetButtonsInteractable(true);

        yield return null;
    }

    void SetupNextQuestion()
    {
        QuizQuestion newQ = pendingQuestions.Dequeue();
        currentQuestion = newQ;

        question.text = newQ.question;

        for (int i = 0; i < answerContainer.childCount; i++)
        {
            answerContainer.GetChild(i).GetComponent<QuizAnswerBtn>().SetText(newQ.answers[i]);
        }
    }

    void ResetQuiz()
    {
        maxQCount = 0;
        currentQuestion = null;
        points = 0;
        pendingQuestions.Clear();
    }
}
