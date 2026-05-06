using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(ReflectionProbe))]
public class OptimizedProbeUpdater : MonoBehaviour
{
    public float moveThreshold = 0.1f; // Only update if moved this far
    private ReflectionProbe _probe;
    private Vector3 _lastPosition;

    void Start()
    {
        _probe = GetComponent<ReflectionProbe>();
        _probe.mode = ReflectionProbeMode.Realtime;
        _probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;

        // Spread the work over 14 frames to stop the "spike" in lag
        _probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;

        _lastPosition = transform.position;
    }

    void Update()
    {
        // Only trigger a render if the probe has moved enough to matter
        if (Vector3.Distance(transform.position, _lastPosition) > moveThreshold)
        {
            _probe.RenderProbe();
            _lastPosition = transform.position;
        }
    }
}
