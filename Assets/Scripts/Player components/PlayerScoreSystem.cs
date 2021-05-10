using System.Collections;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    [Header("Score system")]
    public int Score;
    public int ScoreBuffer;

    public int Streak = 0;

    int _Multiplication;
    public int Multiplication
    {
        get { return !MultiplicationEnabled ? 1 : _Multiplication; }
        set { _Multiplication = value; }
    }
    public bool MultiplicationEnabled = true;

    /// <summary>
    /// Adds a given amount of score to the player, respecting the Multiplication value. <br/>
    /// Default is 1 score.
    /// </summary>
    public void AddScore(int score = 1)
    {
        Score += score * Multiplication;
        // TODO: UI & anim!
    }

    /// <summary>
    /// Adds an amount of score to the buffer. <br/>
    /// This does not add it to the global score yet. <br/> 
    /// Used to display the score the player will get if they succeed in clearaing a sequence. <br/>
    /// NOTE: This does not need multiplication - give regular values! <br/>
    /// NOTE: -1 clears the buffer and removes buffer UI!
    /// </summary>
    public void BufferScore(int score)
    {
        if (score == -1)
            ScoreBuffer = 0;
        else
            ScoreBuffer += score;

        // UI & anim!
    }
    public void ClearBuffer() => BufferScore(-1);

    /// <summary>
    /// Subtracts a given amount of score from the player.
    /// </summary>
    public void RemoveScore(int score, bool multiplication = false)
    {
        Score -= score * (multiplication ? Multiplication : 1);
        // TODO: UI & anim!
    }
}