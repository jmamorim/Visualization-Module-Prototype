using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.IO;
using TMPro;

public class CSVPicker : MonoBehaviour
{
    public Button pickFileButton;
    public TMP_Text feedbackText;
    public TMP_Text fileNameText;
    public TMP_InputField intervalInputField;
    public Parser parser; 
    
    void Start()
    {
        pickFileButton.onClick.AddListener(OnPickFileClicked);
    }

    void OnPickFileClicked()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open CSV File", "", "csv", false);

        if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
        {
            ShowMessage("No file selected", Color.red);
            return;
        }
        string csvPath = paths[0];

        ShowFileName(Path.GetFileName(csvPath));

        parser.receiveData(csvPath);
    }

    void ShowMessage(string msg, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.color = color;
        }
    }

    void ShowFileName(string msg)
    {
        if (fileNameText != null)
        {
            fileNameText.text = msg;
        }
    }
}
