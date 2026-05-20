using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MenuGlowPlace : MonoBehaviour
{
    [Header("Glow Colors")]
    public Color baseColor = Color.white;
    public Color glowColor = Color.cyan;

    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;

    [Header("Scale Pulse (Optional)")]
    public bool scalePulse = true;
    public float scaleAmount = 0.05f;

    [Header("Mouse Influence Clamp")]
    public float influenceClamp = 1f; // prevents over-pulling outside screen

    private Image img;
    private RectTransform rect;
    private Vector3 startScale;

    void Start()
    {
        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        startScale = rect.localScale;
    }

    void Update()
    {
        // Normalized mouse (-1 to 1)
        Vector2 mouse = Input.mousePosition;

        Vector2 normalized = new Vector2(
            (mouse.x / Screen.width - 0.5f) * 2f,
            (mouse.y / Screen.height - 0.5f) * 2f
        );

        // Clamp so it never exceeds screen influence
        normalized = Vector2.ClampMagnitude(normalized, influenceClamp);

        // Pulse glow
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        img.color = Color.Lerp(baseColor, glowColor, t) * intensity;

        // Optional breathing scale
        if (scalePulse)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * scaleAmount;
            rect.localScale = startScale * scale;
        }
    }
}