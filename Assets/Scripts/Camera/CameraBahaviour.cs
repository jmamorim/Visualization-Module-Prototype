using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBahaviour : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 5.0f;
    public float zoomSpeed = 20.0f;
    public float minZoomDistance = 0f;
    public float maxZoomDistance = 100000.0f;

    private Vector3 lastMousePosition;
    [SerializeField]
    private bool canRotate = false;

    void Update()
    {
        if (canRotate)
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
            if (Input.GetKey(KeyCode.Z))
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance > minZoomDistance)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target.position, zoomSpeed * Time.deltaTime);
                }
            }

            if (Input.GetKey(KeyCode.C))
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < maxZoomDistance)
                {
                    transform.position = Vector3.MoveTowards(transform.position, transform.position - (target.position - transform.position), zoomSpeed * Time.deltaTime);
                }
            }
        }

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
