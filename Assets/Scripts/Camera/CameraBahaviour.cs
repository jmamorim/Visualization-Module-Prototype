using UnityEngine;
using UnityEngine.EventSystems;

public class CameraBehaviour : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public Manager manager;
    public Transform paralelPos;

    [Header("Visualization")]
    public bool isMultiVisualization = false;

    [Header("Camera Settings")]
    [SerializeField] private bool isFree = false;
    [SerializeField] private bool canUseCameraMovement = true;
    [SerializeField] private float freeCameraMoveSpeed = 10.0f;
    [SerializeField] private float lookSensitivity = 2.0f;
    [SerializeField] private float minVerticalAngle = -45f;
    [SerializeField] private float maxVerticalAngle = 45f;

    //Private fields
    private float zoomSpeed = 5.0f;
    private float rotationSpeed = 30.0f;
    private float panSpeed = 0.3f;
    private float minZoomFOV = 20f;
    private float maxZoomFOV = 120f;
    private float minZoomOrtho = 10f;
    private float maxZoomOrtho = 100f;
    private float orthographicSize;

    private bool isTopographic = false;
    private Vector3 lastMousePosition;
    private Vector3 initialPosition;
    private Transform initialLookAt;
    private Quaternion initialRotation;
    private float pitch = 0f;
    private float yaw = 0f;
    private float currentVerticalAngle = 0f;

    private Camera cam;

    #region Unity Methods

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKeyDown(KeyCode.R))
            ResetCamera();

        if (scroll != 0 && IsMouseOverViewport() && !isOverUI)
        {
            if (!isTopographic)
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - scroll * zoomSpeed, minZoomFOV, maxZoomFOV);
            else
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoomOrtho, maxZoomOrtho);
        }

        if (canUseCameraMovement && !isOverUI && ((IsMouseOverViewport() && isMultiVisualization) || !isMultiVisualization))
        {
            if (isFree && !isTopographic)
                HandleFreeCameraMovement();
            else
                HandleOrbitOrPan();
        }
    }

    #endregion

    #region Camera Movement Methods

    private void HandleFreeCameraMovement()
    {
        if (Input.GetMouseButton(1) && IsMouseOverViewport())
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) movement += transform.forward;
        if (Input.GetKey(KeyCode.S)) movement -= transform.forward;
        if (Input.GetKey(KeyCode.A)) movement -= transform.right;
        if (Input.GetKey(KeyCode.D)) movement += transform.right;
        if (Input.GetKey(KeyCode.Q)) movement -= Vector3.up;
        if (Input.GetKey(KeyCode.E)) movement += Vector3.up;

        if (movement != Vector3.zero)
            transform.position += movement.normalized * freeCameraMoveSpeed * Time.deltaTime;
    }

    private void HandleOrbitOrPan()
    {
        if (Input.GetMouseButtonDown(0) && IsMouseOverViewport())
            lastMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0) && IsMouseOverViewport())
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            if (!isTopographic)
            {
                float angleX = delta.x * rotationSpeed * Time.deltaTime;
                float angleY = -delta.y * rotationSpeed * Time.deltaTime;

                transform.RotateAround(target.position, Vector3.up, angleX);
                float newVerticalAngle = currentVerticalAngle + angleY;
                newVerticalAngle = Mathf.Clamp(newVerticalAngle, minVerticalAngle, maxVerticalAngle);
                float actualAngleY = newVerticalAngle - currentVerticalAngle;
                transform.RotateAround(target.position, transform.right, actualAngleY);

                currentVerticalAngle = newVerticalAngle;
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

    #endregion

    #region Camera State Methods

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
        currentVerticalAngle = 0f;
        manager.ResetSelected();
    }

    public void SetToTopographic()
    {
        isTopographic = !isTopographic;
        cam.orthographic = isTopographic;
        if (isTopographic)
        {
            cam.orthographicSize = orthographicSize;
            cam.transform.position = paralelPos.position;
            cam.transform.rotation = paralelPos.rotation;
        }
        else
        {
            ResetCamera();
        }
    }

    public void SetOrthographicSize(float size) => orthographicSize = size;

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

    #endregion

    #region Utility Methods

    public bool IsMouseOverViewport()
    {
        Vector3 mouse = Input.mousePosition;
        float normalizedX = mouse.x / Screen.width;
        float normalizedY = mouse.y / Screen.height;
        return cam.rect.Contains(new Vector2(normalizedX, normalizedY));
    }

    public bool isTopograhicMode() => isTopographic;

    public void EnableCameraMovement() => canUseCameraMovement = true;
    public void DisableCameraMovement() => canUseCameraMovement = false;
    public bool CanMoveCamera() => canUseCameraMovement;

    public void SetFreeCamera(bool free)
    {
        isFree = free;
        if (free)
        {
            Vector3 currentRotation = transform.eulerAngles;
            yaw = currentRotation.y;
            pitch = currentRotation.x;
            if (pitch > 180f) pitch -= 360f;
        }
        else
        {
            ResetCamera();
        }
    }

    public bool IsFreeCamera() => isFree;

    #endregion
}
