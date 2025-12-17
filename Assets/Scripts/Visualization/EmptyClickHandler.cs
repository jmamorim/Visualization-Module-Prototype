using UnityEngine;

public class EmptyClickHandler : MonoBehaviour
{
    public Manager manager;
    private Vector3 mouseDownPosition;
    private float dragThreshold = 5f;
    private bool isMouseDown = false;

    void Update()
    {
        if (!manager.canInteract) return;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            mouseDownPosition = Input.mousePosition;
            isMouseDown = true;
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            if (isMouseDown)
            {
                float dragDistance = Vector3.Distance(mouseDownPosition, Input.mousePosition);
                if (dragDistance < dragThreshold)
                {
                    CheckForEmptyClick();
                }
                isMouseDown = false;
            }
        }
    }

    void CheckForEmptyClick()
    {
        Camera clickedCamera = GetCameraUnderMouse();
        if (clickedCamera == null) return;

        Ray ray = clickedCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit) || hit.collider.GetComponent<Tree>() == null)
        {
            HandleEmptyClick();
        }
    }

    Camera GetCameraUnderMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector2 normalizedMouse = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);

        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.rect.Contains(normalizedMouse))
            {
                return cam;
            }
        }
        return null;
    }

    void HandleEmptyClick()
    {
        manager.DeselectTree();

        foreach (var cam in Camera.allCameras)
        {
            var behaviour = cam.GetComponent<CameraBehaviour>();
            if (behaviour != null && behaviour.IsMouseOverViewport() && behaviour.CanMoveCamera())
            {
                behaviour.ResetLookAt();
                break;
            }
        }
    }
}