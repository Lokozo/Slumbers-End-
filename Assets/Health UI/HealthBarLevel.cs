using UnityEngine;
using UnityEngine.UI;

public class HealthBarLevel : MonoBehaviour
{
    public PlayerStats stats; //  Drag Player GameObject here
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    [SerializeField] private float smoothSpeed = 5f;

    void Awake()
    {

    }


    void Start()
    {
        if (stats == null)
            stats = PlayerStats.Instance;
        slider.maxValue = stats.maxHealth;
        slider.value = stats.health;
        fill.color = gradient.Evaluate(1f);
    }

    void Update()
    {
        float target = stats.health;
        slider.value = Mathf.Lerp(slider.value, target, Time.deltaTime * smoothSpeed);
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
