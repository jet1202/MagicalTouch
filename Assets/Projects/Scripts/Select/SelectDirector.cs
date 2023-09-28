using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SelectDirector : MonoBehaviour
{
    [SerializeField] private GameObject contentCanvas;
    [SerializeField] private GameObject subCanvas;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject subCamera;
    [SerializeField] private AudioPlayer audioPlayer;

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
        // canvasを表示
        contentCanvas.SetActive(true);
        subCanvas.SetActive(false);
        mainCamera.SetActive(true);
        subCamera.SetActive(false);
        
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

        songList = Array.Empty<SongList>();

        // 曲データの読み込み
        IEnumerator corutine = importScore.ImportSongData();
        yield return StartCoroutine(corutine);
        songList = (SongList[])corutine.Current;

        // division = SelectData.division;
        division = "Pack1";
        
        List<SongList> displaySong = new List<SongList>();
        int leng = songList.Length;
        for (int i = 0; i < leng; i++)
        {
            if (songList[i].division == division)
                displaySong.Add(songList[i]);
        }

        // スクロールの設定
        yield return StartCoroutine(scrollController.Setting(displaySong));

        isOk = true;

        // マスクを消す
        mask.GetComponent<Image>().DOFade(0f, 0.7f)
            .OnComplete(() => mask.SetActive(false));
    }

    /// <summary>
    ///     曲確定、subシーンに移動
    /// </summary>
    public void MoveGame()
    {
        audioPlayer.StopBgm();
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

    /// <summary>
    ///     設定画面に移動
    /// </summary>
    public void MoveSetting()
    {
        SettingData.fromScene = SceneManager.GetActiveScene().name;
        audioPlayer.StopBgm();
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        mask.GetComponent<Image>().DOFade(1f, 0.7f)
            .OnComplete(() =>
            {
                SceneManager.LoadScene("SettingScene");
            });
    }
}
