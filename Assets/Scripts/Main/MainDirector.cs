using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainDirector : MonoBehaviour
{
    [SerializeField] private ImportScore importScore;
    [SerializeField] private ScrollController scrollController;
    [SerializeField] private GameObject mask;

    public SongList[] songList;
    private string division;

    public bool isOk = false;

    IEnumerator Start()
    {
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

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

        mask.GetComponent<Image>().DOFade(0f, 0.7f)
            .OnComplete(() => mask.SetActive(false));
}
}
