using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraBehaviour : MonoBehaviour
{
    public Transform target;
    public Manager manager;
    public bool isMultiVisualization = false;
    public Transform paralelPos;

    Camera cam;
    Vector3 lastMousePosition;
    Vector3 initialPosition;
    Transform initialLookAt;
    Quaternion initialRotation;
    float zoomSpeed = 5.0f;
    float rotationSpeed = 30.0f;
    float minZoomFOV = 20f;
    float maxZoomFOV = 120f;
    float minZoomOrtho = 10f;
    float maxZoomOrtho = 100f;
    float orthographicSize;

    [SerializeField] bool canRotate = true;

    private void Start()
    {
        cam = gameObject.gameObject.GetComponent<Camera>();
    }

    public void SetOrthographicSize(float size)
    {
        orthographicSize = size;
    }

    public float GetOrthographicSize()
    {
        return orthographicSize;
    }

    public void InitializeCamera(Vector3 initPos, Quaternion initRot, Transform lookAt, Vector3 paralelPos, Quaternion paralelRot)
    {
        initialPosition = initPos;
        initialLookAt = lookAt;
        initialRotation = initRot;
        this.paralelPos.position = paralelPos;
        this.paralelPos.rotation = paralelRot;
        target = lookAt;
        transform.LookAt(target.position);
    }

    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        target.position = initialLookAt.position;
        manager.ResetSelected();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCamera();
        }
        if (scroll != 0 && IsMouseOverViewport())
        {
            if (!manager.isParalelCameraActive)
            {
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * zoomSpeed, minZoomFOV, maxZoomFOV);
            }
            else if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoomOrtho, maxZoomOrtho);
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

    public void ChangeLookAt(Transform newPoint)
    {
        if (!manager.isParalelCameraActive)
        {
            target = newPoint;
            transform.LookAt(target.position);
        }
    }

    public void ResetLookAt()
    {
        if (!manager.isParalelCameraActive)
        {
            target = initialLookAt;
            transform.rotation = initialRotation;
            transform.LookAt(target);
        }
    }

    public bool IsMouseOverViewport()
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

    public bool CanRotate()
    {
        return canRotate;
    }
}