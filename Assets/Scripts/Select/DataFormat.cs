using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveSongData
{
    public int Score { get; set; }
    public int Accuracy { get; set; } // 100倍
}

[Serializable]
public class SaveSong
{
    public string Id { get; set; }
    public SaveSongData[] Data { get; set; }
}

[Serializable]
public class ListSaveData
{
    public SongList[] item;
}

[Serializable]
public class SongList
{
    public string title;
    public string id;
    public int[] constant;
    public string division;
    public string composer;
    public int number;
    public int chorus;
}

public class SongDataList
{
    public string title;
    public string id;
    public int[] constant;
    public string division;
    public string composer;
    public int number;
    public int chorus;

    public int[] score;
    public Texture image;
}

[Serializable]
public class SongInfo
{
    public string data;
}