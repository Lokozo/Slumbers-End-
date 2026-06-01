using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public BaseEnemy enemy;
    public Slider slider;
    public Image fill;

    [Header("Visual")]
    public Gradient gradient;
    public float smoothSpeed = 8f;

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;

        if (enemy == null)
            enemy = GetComponentInParent<BaseEnemy>();

        if (enemy != null)
        {
            slider.maxValue = enemy.maxHealth;
            slider.value = enemy.health;
            fill.color = gradient.Evaluate(1f);
        }
    }

    private void Update()
    {
        if (enemy == null)
            return;

        slider.value = Mathf.Lerp(
            slider.value,
            enemy.health,
            Time.deltaTime * smoothSpeed
        );

        fill.color =
            gradient.Evaluate(slider.normalizedValue);

        if (mainCam != null)
        {
            transform.forward =
                mainCam.transform.forward;
        }

        if (enemy.health <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}