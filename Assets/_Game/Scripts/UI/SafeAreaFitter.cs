using UnityEngine;
// Device.Screen matches Screen on a real device, and also reports simulated values in the Device Simulator.
using DeviceScreen = UnityEngine.Device.Screen;

/// <summary>
/// Stretches this RectTransform to the device safe area and keeps it fitted when the
/// safe area or resolution changes. Children anchored inside it stay clear of notches.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private Rect appliedSafeArea;
    private Vector2Int appliedScreenSize;

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        Rect safeArea = DeviceScreen.safeArea;
        Vector2Int screenSize = new Vector2Int(DeviceScreen.width, DeviceScreen.height);

        if (safeArea != appliedSafeArea || screenSize != appliedScreenSize)
        {
            ApplySafeArea(safeArea, screenSize);
        }
    }

    public void ApplySafeArea(Rect safeArea, Vector2Int screenSize)
    {
        if (screenSize.x <= 0 || screenSize.y <= 0)
        {
            return;
        }

        appliedSafeArea = safeArea;
        appliedScreenSize = screenSize;

        RectTransform rectTransform = (RectTransform)transform;
        rectTransform.anchorMin = new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
        rectTransform.anchorMax = new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
