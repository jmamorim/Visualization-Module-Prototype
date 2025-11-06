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
    public TMP_InputField intervalInputField;
    public Parser parser;

    //more then one scenario in one file
    [SerializeField] bool isMulti = false;

    string filePath = "";

    [SerializeField]
    bool isYieldTable = false;

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

        string currentFilePath = paths[0];

        if (isYieldTable)
        {
            if (isMulti)
            {
                parser.receiveMultiYieldTablePath(currentFilePath, filePath);
            }
            else
            {
                parser.receiveYieldTablePath(currentFilePath, filePath);
            }
            filePath = currentFilePath;
        }
        else
        {
            parser.receiveSoloTreePath(currentFilePath, filePath);
            filePath = currentFilePath;
        }
    }
}
