using System;
using TMPro;
using UnityEngine;

/// <summary>
/// The How to Play pages. Knows nothing about gameplay: whoever opens it can
/// listen to Closed. Closing by either button counts as having seen it, which
/// also stops Level01 from showing it again.
/// </summary>
public class OnboardingOverlay : MonoBehaviour
{
    private const string CompletedKey = "Onboarding.Completed";

    [SerializeField] private GameObject[] pages;
    [SerializeField] private TextMeshProUGUI nextLabel;
    [SerializeField] private TextMeshProUGUI pageCounter;
    [SerializeField] private GameObject skipButton;
    [SerializeField] private string nextText = "NEXT";
    [SerializeField] private string finishText = "GOT IT";

    private int currentPage;

    public static bool IsCompleted => PlayerPrefs.GetInt(CompletedKey, 0) == 1;

    /// <summary>Raised when the player finishes or skips.</summary>
    public event Action Closed;

    public void ShowOnboarding()
    {
        ShowPage(0);
        gameObject.SetActive(true);
    }

    public void OnNextPressed()
    {
        if (currentPage < pages.Length - 1)
        {
            ShowPage(currentPage + 1);
        }
        else
        {
            Finish();
        }
    }

    public void OnSkipPressed()
    {
        Finish();
    }

    private void ShowPage(int index)
    {
        currentPage = index;
        bool isLastPage = index == pages.Length - 1;

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
        }

        nextLabel.text = isLastPage ? finishText : nextText;
        pageCounter.text = $"{index + 1} / {pages.Length}";
        skipButton.SetActive(!isLastPage);
    }

    private void Finish()
    {
        PlayerPrefs.SetInt(CompletedKey, 1);
        PlayerPrefs.Save();
        gameObject.SetActive(false);
        Closed?.Invoke();
    }
}
