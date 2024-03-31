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
        Free,
        Normal,
        Busy,
        Impossible,
        Break
    }
    
    public static string division = "Pack1";
    public static int number;
    public static SortMode mode = SortMode.Default;
    public static DifficultyMode difficulty = DifficultyMode.Normal;
}
