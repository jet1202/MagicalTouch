using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;

public class MainDirector : MonoBehaviour
{
    [SerializeField] private ImportScore importScore;

    public SongList[] songList;
    
    public bool isOk = false;
    
    IEnumerator Start()
    {
        songList = Array.Empty<SongList>();
        
        IEnumerator corutine = importScore.ImportSongData();
        yield return StartCoroutine(corutine);
        songList = (SongList[])corutine.Current;
        
        Debug.Log(songList[0].title);

        isOk = true;
    }
}
