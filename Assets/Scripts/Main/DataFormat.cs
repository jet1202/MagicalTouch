using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveSongData
{
    public string Difficult { get; set; }
    public int Score { get; set; }
    public int Accuracy { get; set; } // 100倍
}

[Serializable]
public class SaveSong
{
    public string Title { get; set; }
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
}

public class SongDataList
{
    public string title;
    public string id;
    public int[] constant;
    public string division;
    public string composer;
    public int number;

    public int[] score;
    public Texture image;
}