using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
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
    float panSpeed = 0.3f;
    float minZoomFOV = 20f;
    float maxZoomFOV = 120f;
    float minZoomOrtho = 10f;
    float maxZoomOrtho = 100f;
    float orthographicSize;
    bool isTopographic = false;

    [SerializeField] bool isFree = false;
    [SerializeField] bool canUseCameraMovement = true;
    [SerializeField] float freeCameraMoveSpeed = 10.0f;
    [SerializeField] float lookSensitivity = 2.0f;

    private float pitch = 0f;
    private float yaw = 0f;

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


    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCamera();
        }
        if (scroll != 0 && IsMouseOverViewport())
        {
            if (!isTopographic)
            {
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * zoomSpeed, minZoomFOV, maxZoomFOV);
            }
            else
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoomOrtho, maxZoomOrtho);
            }
        }
        if (canUseCameraMovement && ((IsMouseOverViewport() && isMultiVisualization) || !isMultiVisualization))
        {
            if (isFree && !isTopographic)
            {
                float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

                yaw += mouseX;
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, -89f, 89f);

                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

                // WASD movement
                Vector3 movement = Vector3.zero;

                if (Input.GetKey(KeyCode.W))
                {
                    movement += transform.forward;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    movement -= transform.forward;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    movement -= transform.right;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    movement += transform.right;
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    movement -= Vector3.up;
                }
                if (Input.GetKey(KeyCode.E))
                {
                    movement += Vector3.up;
                }

                if (movement != Vector3.zero)
                {
                    transform.position += movement.normalized * freeCameraMoveSpeed * Time.deltaTime;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && IsMouseOverViewport())
                {
                    lastMousePosition = Input.mousePosition;
                }

                if (Input.GetMouseButton(0) && IsMouseOverViewport())
                {
                    Vector3 delta = Input.mousePosition - lastMousePosition;

                    if (!isTopographic)
                    {
                        float angleX = delta.x * rotationSpeed * Time.deltaTime;
                        float angleY = -delta.y * rotationSpeed * Time.deltaTime;
                        transform.RotateAround(target.position, Vector3.up, angleX);
                        transform.RotateAround(target.position, transform.right, angleY);
                    }
                    else
                    {
                        Vector3 move = (transform.right * -delta.x + transform.up * -delta.y) * panSpeed;
                        move.y = 0;
                        transform.position += move;
                        target.position += move;
                    }

                    lastMousePosition = Input.mousePosition;
                }
            }
        }
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

    public void SetToTopographic()
    {
        isTopographic = !isTopographic;
        if (isTopographic)
        {
            cam.orthographic = isTopographic;
            cam.orthographicSize = orthographicSize;
            cam.transform.position = paralelPos.position;
            cam.transform.rotation = paralelPos.rotation;
        }
        else
        {
            ResetCamera();
            cam.orthographic = isTopographic;
        }
    }

    public bool isTopograhicMode()
    {
        return isTopographic;
    }

    public void ChangeLookAt(Transform newPoint)
    {
        if (!isTopographic)
        {
            target = newPoint;
            transform.LookAt(target.position);
        }
    }

    public void ResetLookAt()
    {
        if (!isTopographic)
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

    public void EnableCameraMovement()
    {
        canUseCameraMovement = true;
    }

    public void DisableCameraMovement()
    {
        canUseCameraMovement = false;
    }

    public bool CanMoveCamera()
    {
        return canUseCameraMovement;
    }

    public void SetFreeCamera(bool free)
    {
        isFree = free;
        if (free)
        {
            Vector3 currentRotation = transform.eulerAngles;
            yaw = currentRotation.y;
            pitch = currentRotation.x;
            if (pitch > 180f)
                pitch -= 360f;
        }
        else
        {
            ResetCamera();
        }
    }

    public bool IsFreeCamera()
    {
        return isFree;
    }
}