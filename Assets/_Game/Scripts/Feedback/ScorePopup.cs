using UnityEngine;
using TMPro;

/// <summary>
/// Shows "+value" when MagnetLaunchController awards a reward, floating up from
/// this object's scene position and fading out. A single reused label is enough
/// because a new award simply restarts it.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class ScorePopup : MonoBehaviour
{
    [SerializeField] private MagnetLaunchController launchController;

    [Tooltip("Seconds from appearing to fully faded.")]
    [SerializeField] private float duration = 0.9f;

    [Tooltip("World units the popup rises over its lifetime.")]
    [SerializeField] private float riseDistance = 0.55f;

    [Tooltip("Fraction of the lifetime spent fully opaque before fading.")]
    [Range(0f, 1f)]
    [SerializeField] private float holdFraction = 0.45f;

    private TextMeshPro label;
    private Vector3 startPosition;
    private float elapsed;

    // Subscribed in Awake rather than OnEnable: the component disables itself between popups.
    private void Awake()
    {
        label = GetComponent<TextMeshPro>();
        startPosition = transform.position;
        label.enabled = false;
        enabled = false;

        if (launchController != null)
        {
            launchController.RewardAwarded += Show;
        }
    }

    private void OnDestroy()
    {
        if (launchController != null)
        {
            launchController.RewardAwarded -= Show;
        }
    }

    private void Show(int reward)
    {
        label.text = $"+{reward}";
        label.alpha = 1f;
        label.enabled = true;
        transform.position = startPosition;
        elapsed = 0f;
        enabled = true;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;

        if (t >= 1f)
        {
            label.enabled = false;
            enabled = false;
            return;
        }

        transform.position = startPosition + Vector3.up * (riseDistance * Mathf.Sin(t * Mathf.PI * 0.5f));
        label.alpha = 1f - Mathf.InverseLerp(holdFraction, 1f, t);
    }
}
