using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;

[Serializable]
public class ScoreData
{
    public List<SongData> item;
}


//[MessagePackObject]
[Serializable]
public class SongData
{
    /*[Key(0)]*/ public string Id;
    /*[Key(1)]*/ public List<ScoreDetail> Detail;
}

// [MessagePackObject]
[Serializable]
public class ScoreDetail
{
    /*[Key(0)]*/ public int Difficulty;
    /*[Key(1)]*/ public int Score;
    /*[Key(2)]*/ public int Rank;
}

// [MessagePackObject]
[Serializable]
public class Setting
{
    /*[Key(0)]*/
    public GameSetting Game;
    /*[Key(1)]*/
    public ProfileSetting Profile;
    /*[Key(2)]*/
    public SessionSetting Session;

    public Setting()
    {
        Game = new GameSetting();
        Profile = new ProfileSetting();
        Session = new SessionSetting();
    }
}

// [MessagePackObject]
[Serializable]
public class GameSetting
{
    /*[Key(0)]*/ public int NoteSpeed;
    /*[Key(1)]*/ public bool IsPushLine;
    /*[Key(2)]*/ public bool IsAuto;
    /*[Key(3)]*/ public bool IsLateFast;
    /*[Key(4)]*/ public bool IsColor;
    /*[Key(5)]*/ public int SongOffset;
    /*[Key(6)]*/ public int TapOffset;
    /*[Key(7)]*/ public int MusicVolume;
    /*[Key(8)]*/ public int SeVolume;
    /*[Key(9)]*/ public int NoteThickness;
    /*[Key(10)]*/ public bool FPSMode;

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

// [MessagePackObject]
[Serializable]
public class ProfileSetting
{
    /*[Key(0)]*/ public string Name;
    /*[Key(1)]*/ public int Rate;

    public ProfileSetting()
    {
        Name = "K-Player";
        Rate = 0;
    }
}

// [MessagePackObject]
[Serializable]
public class SessionSetting
{
    /*[Key(0)]*/ public int Difficulty;
    /*[Key(1)]*/ public int SortMode;
    /*[Key(2)]*/ public string Pack;

    public SessionSetting()
    {
        Difficulty = 0;
        SortMode = 0;
        Pack = "Pack1";
    }
}
