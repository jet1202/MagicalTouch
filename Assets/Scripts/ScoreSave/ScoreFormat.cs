using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;

[MessagePackObject]
[Serializable]
public class ScoreData
{
    [Key(0)] public SongData[] Data { get; set; }
}

[MessagePackObject]
[Serializable]
public class SongData
{
    [Key(0)] public string Id { get; set; }
    [Key(1)] public ScoreDetail[] Detail { get; set; }
}

[MessagePackObject]
[Serializable]
public class ScoreDetail
{
    [Key(0)] public int Difficulty { get; set; }
    [Key(1)] public int Score { get; set; }
    [Key(2)] public int Rank { get; set; }
    [Key(3)] public int Accuracy { get; set; }
}
