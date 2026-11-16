using UnityEngine;

/// <summary>
/// Simple looping motion for illustration icons: a swing around the pivot, a
/// pulse in scale, or both. A zero amount leaves that motion off.
/// </summary>
public class UiIconMotion : MonoBehaviour
{
    [SerializeField] private float swingDegrees;
    [SerializeField] private float swingSpeed = 1.5f;
    [SerializeField] private float pulseAmount;
    [SerializeField] private float pulseSpeed = 4f;

    private void Update()
    {
        float time = Time.unscaledTime;

        if (swingDegrees != 0f)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(time * swingSpeed) * swingDegrees);
        }

        if (pulseAmount != 0f)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(time * pulseSpeed);
            transform.localScale = Vector3.one * (1f + pulseAmount * pulse);
        }
    }
}
