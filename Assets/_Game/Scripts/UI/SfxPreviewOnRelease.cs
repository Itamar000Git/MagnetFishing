using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Goes on the SFX volume slider's GameObject. Plays one preview when the press
/// ends, including a release outside the slider, because Unity sends the release
/// to the object that received the press.
/// </summary>
public class SfxPreviewOnRelease : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        // Slider only responds to the left button, which is also what a touch reports.
        if (eventData.button == PointerEventData.InputButton.Left && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfxPreview();
        }
    }
}
