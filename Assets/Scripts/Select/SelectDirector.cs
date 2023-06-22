using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SelectDirector : MonoBehaviour
{
    [SerializeField] private GameObject contentCanvas;
    [SerializeField] private GameObject subCanvas;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject subCamera;

    [SerializeField] private ImportScore importScore;
    [SerializeField] private ScrollController scrollController;
    [SerializeField] private SubDirector subDirector;
    [SerializeField] private GameObject mask;

    public SongList[] songList;
    private string division;

    public bool isOk = false;
    
    void Awake()
    {
#if !UNITY_EDITOR && PLATFORM_ANDROID
        Application.targetFrameRate = 60;
#else
        Application.targetFrameRate = -1;
#endif
        
        Time.timeScale = 1;
    }

    IEnumerator Start()
    {
        contentCanvas.SetActive(true);
        subCanvas.SetActive(false);
        mainCamera.SetActive(true);
        subCamera.SetActive(false);
        
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

        songList = Array.Empty<SongList>();

        IEnumerator corutine = importScore.ImportSongData();
        yield return StartCoroutine(corutine);
        songList = (SongList[])corutine.Current;

        // division = SelectData.division;
        division = "official";

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

    public void MoveGame()
    {
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        mask.GetComponent<Image>().DOFade(1f, 0.7f)
            .OnComplete(() =>
                {
                    contentCanvas.SetActive(false);
                    subCanvas.SetActive(true);
                    mainCamera.SetActive(false);
                    subCamera.SetActive(true);
                    subDirector.MoveAnimation();
                });
    }
}
