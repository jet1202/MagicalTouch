using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;

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

[MessagePackObject]
[Serializable]
public class Setting
{
    [Key(0)] public GameSetting Game { get; set; }
    [Key(1)] public ProfileSetting Profile { get; set; }
    [Key(2)] public SessionSetting Session { get; set; }

    public Setting()
    {
        Game = new GameSetting();
        Profile = new ProfileSetting();
        Session = new SessionSetting();
    }
}

[MessagePackObject]
[Serializable]
public class GameSetting
{
    [Key(0)] public int NoteSpeed { get; set; }
    [Key(1)] public bool IsPushLine { get; set; }
    [Key(2)] public bool IsAuto { get; set; }
    [Key(3)] public bool IsLateFast { get; set; }
    [Key(4)] public bool IsColor { get; set; }
    [Key(5)] public int SongOffset { get; set; }
    [Key(6)] public int TapOffset { get; set; }
    [Key(7)] public int MusicVolume { get; set; }
    [Key(8)] public int SeVolume { get; set; }
    [Key(9)] public int NoteThickness { get; set; }
    [Key(10)] public bool FPSMode { get; set; }

    public GameSetting()
    {
        NoteSpeed = 50;
        IsPushLine = true;
        IsAuto = false;
        IsLateFast = false;
        IsColor = false;
        SongOffset = 0;
        TapOffset = 0;
        MusicVolume = 100;
        SeVolume = 100;
        NoteThickness = 7;
        FPSMode = false;
    }
}

[MessagePackObject]
[Serializable]
public class ProfileSetting
{
    [Key(0)] public string Name { get; set; }
    [Key(1)] public int Rate { get; set; }

    public ProfileSetting()
    {
        Name = "K-Player";
        Rate = 0;
    }
}

[MessagePackObject]
[Serializable]
public class SessionSetting
{
    [Key(0)] public int Difficulty { get; set; }
    [Key(1)] public int SortMode { get; set; }
    [Key(2)] public string Pack { get; set; }

    public SessionSetting()
    {
        Difficulty = 0;
        SortMode = 0;
        Pack = "Pack1";
    }
}
