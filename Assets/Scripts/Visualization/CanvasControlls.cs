using System.Collections.Generic;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
    [SerializeField] private List<Canvas> canvases;
    [SerializeField] private bool isIntroScene = false;

    private int currentCanvasIndex = 0;
    private int previousCanvasIndex = 0;

    private void Awake()
    {
        InitializeCanvases();
    }

    private void InitializeCanvases()
    {
        for (int i = 0; i < canvases.Count; i++)
        {
            canvases[i].enabled = (i == currentCanvasIndex);
        }
    }

    public void SwitchToCanvas(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= canvases.Count)
        {
            Debug.LogWarning($"Invalid canvas index: {targetIndex}");
            return;
        }

        if (targetIndex == currentCanvasIndex)
            return;

        previousCanvasIndex = currentCanvasIndex;
        currentCanvasIndex = targetIndex;

        if(!isIntroScene)
            ManageRotationAllCameras();

        canvases[previousCanvasIndex].enabled = false;
        canvases[currentCanvasIndex].enabled = true;
    }

    public void SwitchToPrevious()
    {
        SwitchToCanvas(previousCanvasIndex);
    }

    public int GetCurrentIndex() => currentCanvasIndex;
    public int GetPreviousIndex() => previousCanvasIndex;

    private void ManageRotationAllCameras()
    {
        var cameras = Camera.allCameras;
        foreach (Camera cam in cameras)
        {
            var cameraBehaviour = cam.GetComponent<CameraBehaviour>();
            if (!cameraBehaviour.CanMoveCamera())
                cameraBehaviour.DisableCameraMovement();
            else
                cameraBehaviour.EnableCameraMovement();
        }
    }
}
