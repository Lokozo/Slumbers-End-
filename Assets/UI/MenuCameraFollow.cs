using UnityEngine;

public class MenuCameraFollow : MonoBehaviour
{
    public float moveAmount = 30f;
    public float smoothSpeed = 5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void LateUpdate()
    {
        float mouseX = (Input.mousePosition.x / Screen.width - 0.5f) * 2f;
        float mouseY = (Input.mousePosition.y / Screen.height - 0.5f) * 2f;

        Vector3 target = startPos + new Vector3(mouseX * moveAmount, mouseY * moveAmount, 0);

        transform.position = Vector3.Lerp(
            transform.position,
            target,
            Time.deltaTime * smoothSpeed
        );
    }
}