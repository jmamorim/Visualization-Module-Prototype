using System.Collections.Generic;
using UnityEngine;

public class CanvasControlls : MonoBehaviour
{
    public bool isIntroScene = false;
    public List<GameObject> canvases;

    int activeCanvasIndex = 0;

    public void EnableControls()
    {
        if(!isIntroScene)
            DissableRotationAllCameras();
        canvases[activeCanvasIndex].SetActive(false);
        activeCanvasIndex = 1;
        canvases[activeCanvasIndex].SetActive(true);
    }

    public void EnableGlossary()
    {
        if (!isIntroScene)
            DissableRotationAllCameras();
        canvases[activeCanvasIndex].SetActive(false);
        activeCanvasIndex = 2;
        canvases[activeCanvasIndex].SetActive(true);
    }

    public void EnableVisCanvas()
    {
        if (!isIntroScene)
            EnableRotationAllCameras();
        canvases[activeCanvasIndex].SetActive(false);
        activeCanvasIndex = 0;
        canvases[activeCanvasIndex].SetActive(true);
    }

    private void DissableRotationAllCameras()
    {
        var cameras = Camera.allCameras;
        foreach (Camera cam in cameras)
        {
            var cameraBehaviour = cam.GetComponent<CameraBehaviour>();
            cameraBehaviour.DisableCameraMovement();
        }
    }

    private void EnableRotationAllCameras()
    {
        var cameras = Camera.allCameras;
        foreach (Camera cam in cameras)
        {
            var cameraBehaviour = cam.GetComponent<CameraBehaviour>();
            cameraBehaviour.EnableCameraMovement();
        }
    }
}
