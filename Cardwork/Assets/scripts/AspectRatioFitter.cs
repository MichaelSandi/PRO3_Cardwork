using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioFitter : MonoBehaviour
{
    public float targetAspect = 16f / 9f;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateAspect();
    }

    void UpdateAspect()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = cam.rect;

        if (scaleHeight < 1.0f)
        {
            // Bildschirm ist zu hoch -> Balken oben/unten
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // Bildschirm ist zu breit -> Balken links/rechts
            float scaleWidth = 1.0f / scaleHeight;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        cam.rect = rect;
    }

    void OnPreCull()
    {
        GL.Clear(true, true, Color.black);
    }
}