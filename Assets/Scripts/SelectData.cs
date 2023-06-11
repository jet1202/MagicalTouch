using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SelectData
{
    public enum SortMode
    {
        Default,
        Name,
        Difficulty,
        Score,
    }

    public enum DifficultyMode
    {
        Normal,
        Hard,
        Expert,
        Impossible,
        Joke
    }
    
    public static string division;
    public static SortMode mode = SortMode.Default;
    public static DifficultyMode difficulty = DifficultyMode.Normal;
}
