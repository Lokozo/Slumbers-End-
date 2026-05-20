using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MenuParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)]
    public float parallaxStrength = 0.5f;

    private RectTransform rect;
    private Vector2 startPos;
    private RectTransform canvasRect;
    private Camera cam;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;

        Canvas canvas = GetComponentInParent<Canvas>();
        cam = Camera.main;
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        Vector2 mouse = Input.mousePosition;

        Vector2 normalized = new Vector2(
            (mouse.x / Screen.width - 0.5f) * 2f,
            (mouse.y / Screen.height - 0.5f) * 2f
        );

        // Clamp so it cannot go beyond screen bounds
        normalized = Vector2.ClampMagnitude(normalized, 1f);

        Vector2 targetOffset = normalized * parallaxStrength * 50f;

        rect.anchoredPosition = Vector2.Lerp(
            rect.anchoredPosition,
            startPos + targetOffset,
            Time.deltaTime * 5f
        );
    }
}