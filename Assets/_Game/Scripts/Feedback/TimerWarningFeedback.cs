using UnityEngine;
using TMPro;

/// <summary>
/// Tints and pulses the timer label once the level is nearly out of time, and
/// restores its normal look when the level ends. Sits on the timer label itself.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TimerWarningFeedback : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;

    [Tooltip("The warning starts when this many seconds remain.")]
    [SerializeField] private float warningSeconds = 10f;

    [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0.45f, 1f);

    [Tooltip("Scale multiplier at the peak of each pulse.")]
    [SerializeField] private float pulseScale = 1.08f;

    [Tooltip("Pulses per second.")]
    [SerializeField] private float pulsesPerSecond = 1f;

    private TextMeshProUGUI label;
    private Color normalColor;
    private Vector3 normalScale;
    private float warningTime;

    private void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
        normalColor = label.color;
        normalScale = transform.localScale;
    }

    private void Update()
    {
        if (levelManager == null)
        {
            return;
        }

        if (levelManager.State != LevelState.Playing)
        {
            label.color = normalColor;
            transform.localScale = normalScale;
            enabled = false;
            return;
        }

        if (levelManager.RemainingTime > warningSeconds)
        {
            return;
        }

        label.color = warningColor;

        float pulse = Mathf.Abs(Mathf.Sin(warningTime * pulsesPerSecond * Mathf.PI));
        transform.localScale = normalScale * Mathf.Lerp(1f, pulseScale, pulse);
        warningTime += Time.deltaTime;
    }
}
