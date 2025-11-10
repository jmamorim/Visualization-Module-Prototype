using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 5.0f;
    public float zoomSpeed = 20.0f;
    public float minZoomDistance = 50f;
    public float maxZoomDistance = 100000.0f;
    public bool isMultiVisualization = false;

    Vector3 lastMousePosition;
    [SerializeField] bool canRotate = false;

    void Update()
    {
        if (!canRotate) return;
        if ((IsMouseOverViewport() && isMultiVisualization) || !isMultiVisualization)
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                float angleX = delta.x * rotationSpeed * Time.deltaTime;
                float angleY = -delta.y * rotationSpeed * Time.deltaTime;
                transform.RotateAround(target.position, Vector3.up, angleX);
                transform.RotateAround(target.position, transform.right, angleY);
                lastMousePosition = Input.mousePosition;
            }

            // Zoom In
            if (Input.GetKey(KeyCode.Z))
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance > minZoomDistance)
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    transform.position += direction * zoomSpeed * Time.deltaTime;
                }
            }

            // Zoom Out
            if (Input.GetKey(KeyCode.C))
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < maxZoomDistance)
                {
                    Vector3 direction = (transform.position - target.position).normalized;
                    transform.position += direction * zoomSpeed * Time.deltaTime;
                }
            }
        }
    }

    private bool IsMouseOverViewport()
    {
        Vector3 mouse = Input.mousePosition;
        float normalizedX = mouse.x / Screen.width;
        float normalizedY = mouse.y / Screen.height;
        // Check if inside this camera's rect
        return GetComponent<Camera>().rect.Contains(new Vector2(normalizedX, normalizedY));
    }

    public void EnableRotation()
    {
        canRotate = true;
    }

    public void DisableRotation()
    {
        canRotate = false;
    }
}