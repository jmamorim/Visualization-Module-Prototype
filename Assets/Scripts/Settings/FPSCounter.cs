using TMPro;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText;

    float timer;
    int frames;
    float fps;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            fpsText.enabled = !fpsText.enabled;
        }

        frames++;
        timer += Time.unscaledDeltaTime;

        if (timer >= 0.5f)
        {
            fps = frames / timer;
            fpsText.text = $"{Mathf.RoundToInt(fps)}";

            frames = 0;
            timer = 0f;
        }
    }
}