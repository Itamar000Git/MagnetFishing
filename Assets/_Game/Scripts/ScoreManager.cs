using System;
using UnityEngine;

/// <summary>
/// Tracks the player's score for the current level. Local to Level01 — not a
/// singleton, not persisted across scenes. Score starts at 0.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private int score;

    public int Score => score;

    /// <summary>Raised whenever the score changes, with the new total. HUD subscribes to this.</summary>
    public event Action<int> ScoreChanged;

    public void AddScore(int amount)
    {
        score += amount;
        ScoreChanged?.Invoke(score);
    }
}
