using UnityEngine;

public class EmptyClickHandler : MonoBehaviour
{
    public Manager manager;
    private Vector3 mouseDownPosition;
    private float dragThreshold = 5f;
    private bool isMouseDown = false;
    private int mouseButton = -1;

    void Update()
    {
        if (!manager.canInteract) return;

        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPosition = Input.mousePosition;
            isMouseDown = true;
            mouseButton = 0;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            mouseDownPosition = Input.mousePosition;
            isMouseDown = true;
            mouseButton = 1;
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            if (isMouseDown)
            {
                float dragDistance = Vector3.Distance(mouseDownPosition, Input.mousePosition);
                if (dragDistance < dragThreshold)
                {
                    CheckForEmptyClick(mouseButton);
                }
                isMouseDown = false;
                mouseButton = -1;
            }
        }
    }

    void CheckForEmptyClick(int button)
    {
        Camera clickedCamera = GetCameraUnderMouse();
        if (clickedCamera == null) return;

        Ray ray = clickedCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit) || hit.collider.GetComponent<Tree>() == null)
        {
            HandleEmptyClick(button);
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

    void HandleEmptyClick(int button)
    {
        if (button == 0) 
        {
            manager.DeselectTree();
        }
        else if (button == 1) 
        {
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
}