using UnityEngine;
using UnityEngine.UI;

public class HungerBar : MonoBehaviour
{
    public PlayerStats stats; //  Drag Player GameObject here
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    [SerializeField] private float smoothSpeed = 5f;

    void Start()
    {
        slider.maxValue = stats.maxHunger;
        slider.value = stats.hunger;
        fill.color = gradient.Evaluate(1f);
    }

    void Update()
    {
        float target = stats.hunger;
        slider.value = Mathf.Lerp(slider.value, target, Time.deltaTime * smoothSpeed);
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}