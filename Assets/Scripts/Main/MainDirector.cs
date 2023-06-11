using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using Unity.VisualScripting;
using UnityEngine;

public class MainDirector : MonoBehaviour
{
    [SerializeField] private ImportScore importScore;
    [SerializeField] private ScrollController scrollController;

    public SongList[] songList;
    private string division;

    public bool isOk = false;
    
    IEnumerator Start()
    {
        songList = Array.Empty<SongList>();
        
        IEnumerator corutine = importScore.ImportSongData();
        yield return StartCoroutine(corutine);
        songList = (SongList[])corutine.Current;

        // division = SelectData.division;
        division = "Test";

        List<SongList> displaySong = new List<SongList>();
        int leng = songList.Length;
        for (int i = 0; i < leng; i++)
        {
            if (songList[i].division == division)
                displaySong.Add(songList[i]);
        }
        
        yield return StartCoroutine(scrollController.Setting(displaySong));

        isOk = true;
    }
}
