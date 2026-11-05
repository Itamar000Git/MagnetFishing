using UnityEngine;

/// <summary>
/// Plays a one-shot splash when a launched magnet crosses the water line in
/// either direction, and emits bubbles while it is below that line. Only reads
/// the magnet's position; the particle systems live outside the magnet hierarchy.
/// </summary>
public class MagnetWaterFeedback : MonoBehaviour
{
    [SerializeField] private MagnetLaunchController launchController;
    [SerializeField] private Transform magnet;
    [SerializeField] private ParticleSystem splashParticles;
    [SerializeField] private ParticleSystem bubbleParticles;

    // The magnet rests just under the drawn surface, so its centre never crosses
    // the surface itself. The trigger line sits below the lowest idle position
    // instead, which makes each launch produce exactly one entry and one exit.
    [Tooltip("World Y the magnet's centre must drop below to count as entering the water. Must stay below the magnet's idle position.")]
    [SerializeField] private float waterEntryY = 2.55f;

    [Tooltip("Extra height above waterEntryY the magnet must reach to count as leaving the water. Prevents retriggering near the line.")]
    [SerializeField] private float exitMargin = 0.05f;

    [Tooltip("World Y the splash is drawn at, along the bottom of the visible surface.")]
    [SerializeField] private float splashY = 2.95f;

    [Tooltip("Number of droplets per splash.")]
    [SerializeField] private int splashDropletCount = 8;

    private bool isUnderwater;

    private void Start()
    {
        SetBubblesEmitting(false);
    }

    // LateUpdate so the position read includes this frame's magnet movement.
    private void LateUpdate()
    {
        if (launchController == null || magnet == null)
        {
            return;
        }

        if (launchController.IsIdle)
        {
            // Covers a long frame carrying the magnet from underwater straight to rest.
            if (isUnderwater)
            {
                SetUnderwater(false);
            }

            return;
        }

        float magnetY = magnet.position.y;

        if (!isUnderwater && magnetY <= waterEntryY)
        {
            SetUnderwater(true);
        }
        else if (isUnderwater && magnetY >= waterEntryY + exitMargin)
        {
            SetUnderwater(false);
        }

        if (isUnderwater && bubbleParticles != null)
        {
            bubbleParticles.transform.position = magnet.position;
        }
    }

    private void SetUnderwater(bool underwater)
    {
        isUnderwater = underwater;
        PlaySplash();
        SetBubblesEmitting(underwater);
    }

    private void PlaySplash()
    {
        if (splashParticles == null)
        {
            return;
        }

        Vector3 position = magnet.position;
        position.y = splashY;
        splashParticles.transform.position = position;
        splashParticles.Emit(splashDropletCount);
    }

    private void SetBubblesEmitting(bool emitting)
    {
        if (bubbleParticles == null)
        {
            return;
        }

        if (emitting)
        {
            bubbleParticles.transform.position = magnet.position;
        }

        ParticleSystem.EmissionModule emission = bubbleParticles.emission;
        emission.enabled = emitting;
    }
}
