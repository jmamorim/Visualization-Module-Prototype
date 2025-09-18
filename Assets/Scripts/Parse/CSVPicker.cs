using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.IO;
using TMPro;
using System;

public class CSVPicker : MonoBehaviour
{
    public Button pickFileButton;
    public TMP_Text feedbackText;
    public TMP_Text fileNameText;
    public TMP_InputField intervalInputField;
    public Parser parser;

    [SerializeField]
    private bool isYieldTable = false;
    
    void Start()
    {
        pickFileButton.onClick.AddListener(OnPickFileClicked);
    }

    void OnPickFileClicked()
    {
        var filters = new[] {
            new ExtensionFilter("CSV Files", "csv"),
            new ExtensionFilter("Excel Files", "xlsx")
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", filters, false);

        if (paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
        {
            ShowMessage("No file selected", Color.red);
            return;
        }
        string filePath = paths[0];

        ShowFileName(Path.GetFileName(filePath));

        if (isYieldTable)
        {
            parser.receiveYieldTableData(filePath);
        }
        else
        {
            parser.receiveSoloTreeData(filePath);
        }
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
