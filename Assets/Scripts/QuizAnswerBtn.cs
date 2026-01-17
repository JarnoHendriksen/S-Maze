using UnityEngine;
using TMPro;

public class QuizAnswerBtn : MonoBehaviour
{
    Transform quizPanel;

    private void Start()
    {
        quizPanel = transform.parent.parent;
    }

    public void SetText(string txt)
    {
        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = txt;
    }

    public void BtnPressed()
    {
        // Get text in this button
        string txt = transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
        quizPanel.GetComponent<QuizHandler>().AnswerBtnPressed(txt);
    }
}
