using UnityEngine;

/// <summary>
/// Swims a fish horizontally between two world-space X boundaries. Attach to
/// the Fish root next to its CatchableObject. Author the fish with its mouth
/// and Hook child on the local +X side: facing is set by the sign of the
/// root's localScale.x, so the Hook mirrors with the mouth.
/// </summary>
[RequireComponent(typeof(CatchableObject))]
public class FishMovement : MonoBehaviour
{
    [Tooltip("Temporary tuning value, not final: swim speed in world units per second.")]
    [SerializeField] private float swimSpeed = 1f;

    [Tooltip("Temporary tuning value, not final: world-space X at which the fish turns back to the right.")]
    [SerializeField] private float leftBoundaryX = -6f;

    [Tooltip("Temporary tuning value, not final: world-space X at which the fish turns back to the left.")]
    [SerializeField] private float rightBoundaryX = 6f;

    [Tooltip("Initial swim direction: right when enabled, left otherwise.")]
    [SerializeField] private bool startMovingRight = true;

    private CatchableObject catchable;
    private float direction;

    private void Awake()
    {
        catchable = GetComponent<CatchableObject>();
        SetDirection(startMovingRight ? 1f : -1f);
    }

    private void Update()
    {
        // Catches arrive from physics callbacks, which run before Update in
        // the same frame, so the fish never moves again after its catch. This
        // also keeps the world-space writes below from fighting the magnet
        // that now parents it. A collected fish is inactive and gets no Update.
        if (catchable.IsCaught)
        {
            return;
        }

        Vector3 position = transform.position;
        position.x += direction * swimSpeed * Time.deltaTime;

        // Checking only the boundary ahead avoids flip-flopping every frame
        // when a fish starts outside its range: it just swims back in.
        if (direction > 0f && position.x >= rightBoundaryX)
        {
            position.x = rightBoundaryX;
            SetDirection(-1f);
        }
        else if (direction < 0f && position.x <= leftBoundaryX)
        {
            position.x = leftBoundaryX;
            SetDirection(1f);
        }

        transform.position = position;
    }

    private void SetDirection(float newDirection)
    {
        direction = newDirection;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * newDirection;
        transform.localScale = scale;
    }
}
