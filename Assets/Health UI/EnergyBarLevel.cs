using UnityEngine;
using UnityEngine.UI;

public class EnergyBarLevel : MonoBehaviour
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
            stats = PlayerStats.Get();
        slider.maxValue = stats.maxEnergy;
        slider.value = stats.energy;
        fill.color = gradient.Evaluate(1f);
    }

    void Update()
    {
        if (PlayerStats.Get() != null)
        {
            float target = PlayerStats.Get().energy;
            slider.value = Mathf.Lerp(slider.value, target, Time.deltaTime * smoothSpeed);
            fill.color = gradient.Evaluate(slider.normalizedValue);
        }

    }
}