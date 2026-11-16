using System;
using UnityEngine;

/// <summary>
/// Tracks the running score for the current attempt. Local to Level01 — not a
/// singleton, not persisted across scenes by itself. The score starts at
/// whatever LevelManager seeds via Initialize (the campaign's cumulative
/// score at level start); AddScore then adds catch rewards on top of that
/// exactly as before.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private int score;

    public int Score => score;

    /// <summary>Raised whenever the score changes, with the new total. HUD subscribes to this.</summary>
    public event Action<int> ScoreChanged;

    /// <summary>Sets the starting score directly (not an addition). Called once at level start.</summary>
    public void Initialize(int startingScore)
    {
        score = startingScore;
        ScoreChanged?.Invoke(score);
    }

    public void AddScore(int amount)
    {
        score += amount;
        ScoreChanged?.Invoke(score);
    }
}
