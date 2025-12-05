using UnityEngine;

public class EmptyClickHandler : MonoBehaviour
{
    public Manager manager;
    private Vector3 mouseDownPosition;
    private float dragThreshold = 5f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            float dragDistance = Vector3.Distance(mouseDownPosition, Input.mousePosition);

            if (dragDistance < dragThreshold)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (!Physics.Raycast(ray, out RaycastHit hit))
                {
                    OnDisable();
                }
                else
                {
                    if (hit.collider.GetComponent<Tree>() == null)
                    {
                        OnDisable();
                    }
                }
            }
        }
    }

    void OnDisable()
    {
        manager.DeselectTree();
        var cameras = Camera.allCameras;
        foreach (var cam in cameras)
        {
            var behaviour = cam.GetComponent<CameraBehaviour>();
            if (behaviour != null && behaviour.IsMouseOverViewport() && behaviour.CanRotate())
            {
                behaviour.ResetLookAt();
                break;
            }
        }
    }

}