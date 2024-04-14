using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResultData
{
    public static int score = 0;
    public static int[] tapJudge = new int[31];
    public static int[] resultDetail = new int[5];
    public static int[] pm = new int[6];
    public static int tapGapSum = 0;
    public static int combo, maxCombo;
    public static bool isAuto;

    public static string title = "Test2";
    public static string id = "Test2";
    public static SelectData.DifficultyMode difficult = SelectData.DifficultyMode.Normal;
    public static int difficulty = 10;
}
