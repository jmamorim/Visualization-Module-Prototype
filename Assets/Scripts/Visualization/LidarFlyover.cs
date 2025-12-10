using UnityEngine;
using System.Collections;

public class LidarFlyover : MonoBehaviour
{
    public Manager manager;

    [Header("Flyover Settings")]
    [SerializeField] float flyoverHeight = 50f;
    [SerializeField] float flyoverSpeed = 10f;
    [SerializeField] float scanWidth = 5f;
    [SerializeField] float tiltAngle = 15f;

    Terrain currentTerrain;
    Camera flyoverCamera;
    CameraBehaviour behaviour;
    bool isFlying = false;
    float distanceScalingFactor = 0.6f;

    private void Start()
    {
        flyoverCamera = GetComponent<Camera>();
        behaviour = GetComponent<CameraBehaviour>();
    }

    public bool isCurrentlyFlying()
    {
        return isFlying;
    }

    public void StartFlyover(Terrain terrain)
    {
        if (isFlying)
        {
            Debug.Log("Stopping previous flyover");
            StopFlyover();
            return;
        }

        manager.positionViewPorts(false, flyoverCamera);
        manager.visulaizationCanvas.gameObject.SetActive(false);

        currentTerrain = terrain;
        Debug.Log($"Starting flyover on terrain at {terrain.transform.position}, size: {terrain.terrainData.size}");
        manager.canInteract = false;
        StartCoroutine(FlyoverRoutine());
    }

    public void StopFlyover()
    {
        StopAllCoroutines();
        isFlying = false;
        manager.visulaizationCanvas.gameObject.SetActive(true);
        manager.positionViewPorts(behaviour.isMultiVisualization, null);
        manager.canInteract = true;
        behaviour.ResetCamera();

        Debug.Log("Flyover stopped");
    }

    private IEnumerator FlyoverRoutine()
    {
        isFlying = true;
        Vector3 terrainPos = currentTerrain.transform.position;
        Vector3 terrainSize = currentTerrain.terrainData.size;

        Debug.Log($"Terrain position: {terrainPos}, size: {terrainSize}");

        float startX = terrainPos.x;
        float startZ = terrainPos.z;
        float endX = terrainPos.x + terrainSize.x;
        float endZ = terrainPos.z + terrainSize.z;

        bool movingForward = true;
        float currentZ = startZ;

        int scanLineCount = 0;

        while (currentZ <= endZ && isFlying)
        {
            scanLineCount++;
            float fromX = movingForward ? startX : endX;
            float toX = movingForward ? endX : startX;

            Vector3 startPos = new Vector3(fromX, terrainPos.y + flyoverHeight, currentZ);
            Vector3 endPos = new Vector3(toX, terrainPos.y + flyoverHeight, currentZ);

            Debug.Log($"Scan line {scanLineCount}: Flying from {startPos} to {endPos}");

            yield return StartCoroutine(FlyBetweenPoints(startPos, endPos));

            // Move to next scan line
            currentZ += scanWidth;
            movingForward = !movingForward;
        }

        isFlying = false;
        StopFlyover();
        Debug.Log($"Flyover complete! Scanned {scanLineCount} lines");
    }

    private IEnumerator FlyBetweenPoints(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        float duration = distance / flyoverSpeed;
        float elapsed = 0f;

        Debug.Log($"Flying between points, distance: {distance}, duration: {duration}");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(start, end, t);
            transform.position = currentPos;

            Vector3 forward = (end - start).normalized;
            if (forward.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
                targetRotation *= Quaternion.Euler(90f - tiltAngle, 0f, 0f);
                transform.rotation = targetRotation;
            }
            else
            {
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            yield return null;
        }

        transform.position = end;
    }
    public void StartOrbitalFlyover(Terrain terrain, float radius, float duration)
    {
        if (isFlying)
        {
            StopFlyover();
            return;
        }

        manager.positionViewPorts(false, flyoverCamera);
        manager.visulaizationCanvas.gameObject.SetActive(false);

        currentTerrain = terrain;
        Debug.Log($"Starting orbital flyover, radius: {radius}, duration: {duration}");
        StartCoroutine(OrbitalRoutine(terrain, radius, duration));
    }

    private IEnumerator OrbitalRoutine(Terrain terrain, float radius, float duration)
    {
        isFlying = true;
        currentTerrain = terrain;

        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 center = terrain.transform.position + new Vector3(terrainSize.x / 2f, 0f, terrainSize.z / 2f);
        float diagonalSize = Mathf.Sqrt(terrainSize.x * terrainSize.x + terrainSize.z * terrainSize.z);

        center.y = (diagonalSize * distanceScalingFactor) + flyoverHeight;

        Debug.Log($"Orbital center: {center}, terrain max height: {flyoverHeight}");

        float elapsed = 0f;
        while (elapsed < duration && isFlying)
        {
            elapsed += Time.deltaTime;
            float angle = (elapsed / duration) * 360f * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            transform.position = center + offset;

            Vector3 lookTarget = terrain.transform.position + new Vector3(terrainSize.x / 2f, 0f, terrainSize.z / 2f);
            transform.LookAt(lookTarget);

            yield return null;
        }

        isFlying = false;
        StopFlyover();
        Debug.Log("Orbital flyover complete!");
    }
}