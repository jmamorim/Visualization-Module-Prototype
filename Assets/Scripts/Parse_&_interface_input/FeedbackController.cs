using TMPro;
using UnityEngine;

public class FeedbackController : MonoBehaviour
{
    public TMP_Text[] feedbackTexts;

    public void ShowMessage(string msg)
    {
        foreach (var feedbackText in feedbackTexts)
        {
            if (feedbackText != null)
            {
                feedbackText.text += msg;
                feedbackText.GetComponentInParent<ScrollControler>().ScrollDown();
            }
        }
    }

    public void ShowMessage(string msg, bool isSolo)
    {
        if (isSolo && feedbackTexts[0] != null)
        {
            feedbackTexts[0].text += msg;
            feedbackTexts[0].GetComponentInParent<ScrollControler>().ScrollDown();
        }
        else
        {
            feedbackTexts[1].text += msg;
            feedbackTexts[1].GetComponentInParent<ScrollControler>().ScrollDown();
        }
    }
}
