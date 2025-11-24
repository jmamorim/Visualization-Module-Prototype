using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 5.0f;
    public float zoomSpeed = 50.0f;
    public float minZoomDistance = 50f;
    public float maxZoomDistance = 100000.0f;
    public bool isMultiVisualization = false;
    public Manager manager;

    Camera cam;
    Vector3 lastMousePosition;
    [SerializeField] bool canRotate = true;

    private void Start()
    {
        cam = gameObject.gameObject.GetComponent<Camera>();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // Zoom In (scroll up)
            if (scroll > 0)
            {
                if (distance > minZoomDistance && !manager.isParalelCameraActive)
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    transform.position += direction * zoomSpeed * Time.deltaTime * scroll * 10f;
                }
                else if (cam.orthographic && cam.orthographicSize > 30)
                {
                    cam.orthographicSize -= scroll * 10f;
                }
            }
            // Zoom Out (scroll down)
            else if (scroll < 0)
            {
                if (distance < maxZoomDistance && !manager.isParalelCameraActive)
                {
                    Vector3 direction = (transform.position - target.position).normalized;
                    transform.position += direction * zoomSpeed * Time.deltaTime * Mathf.Abs(scroll) * 10f;
                }
                else if (cam.orthographic)
                {
                    cam.orthographicSize += Mathf.Abs(scroll) * 10f;
                }
            }
        }

        if (!manager.isParalelCameraActive & canRotate & ((IsMouseOverViewport() && isMultiVisualization) || !isMultiVisualization))

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