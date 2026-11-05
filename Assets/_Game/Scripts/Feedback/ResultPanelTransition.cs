using UnityEngine;

/// <summary>
/// Fades the result panel in and eases its content up to full size whenever the
/// panel is activated. Only the content scales, so the full-screen backdrop never
/// reveals its edges. Buttons stay clickable throughout the transition.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ResultPanelTransition : MonoBehaviour
{
    [Tooltip("Child holding the label and buttons. Scaled during the transition.")]
    [SerializeField] private RectTransform content;

    [SerializeField] private float duration = 0.25f;

    [Tooltip("Content scale at the start of the transition.")]
    [SerializeField] private float startScale = 0.92f;

    private CanvasGroup canvasGroup;
    private float elapsed;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        elapsed = 0f;
        Apply(0f);
    }

    private void Update()
    {
        if (elapsed >= duration)
        {
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        Apply(Mathf.Clamp01(elapsed / duration));
    }

    private void Apply(float t)
    {
        float eased = 1f - (1f - t) * (1f - t);
        canvasGroup.alpha = eased;

        if (content != null)
        {
            content.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, eased);
        }
    }
}
