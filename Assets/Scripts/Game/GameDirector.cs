using System;
using System.Collections;
using System.Collections.Generic;
using CriWare;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    [SerializeField] private NotesDirector notesDirector;
    [SerializeField] private NotesController notesController;
    [SerializeField] private Cri cri;
    [SerializeField] private Text timeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Text progressText;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject mask;
    
    private AudioSource audioSource;
    public bool isPlaying = false;
    public float musicTime;
    public float waitTime;
    private bool isAudio = false;

    public bool isOk = false;
    public bool isStart = false;
    public bool isEnd = false;
    
    int frameCount;
    float prevTime;
    float fps;

    private Tween tween;
    private Color panelColor;

    private CriAtomSourceBase.Status status;

    void Awake()
    {
#if !UNITY_EDITOR && PLATFORM_ANDROID
        Application.targetFrameRate = 60;
#else
        Application.targetFrameRate = -1;
#endif
        
        Time.timeScale = 1;
        musicTime = -waitTime;
    }

    private void Start()
    {
        frameCount = 0;
        prevTime = 0.0f;

        infoPanel.SetActive(true);
        infoPanel.transform.GetChild(1).gameObject.SetActive(true);
        infoPanel.transform.GetChild(2).gameObject.SetActive(false);
        panelColor = infoPanel.GetComponent<Image>().color;
        tween = infoPanel.GetComponent<Image>().DOFade(endValue: 100f / 255f, duration: 1f).SetEase(Ease.InQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void Update()
    {
        // fps表示
        frameCount++;
        float time = Time.realtimeSinceStartup - prevTime;
 
        if (time >= 0.5f) {
            fps = frameCount / time;
            timeText.text = $"{fps} fps";
 
            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;
        }

        status = cri.bgm == null ? CriAtomSourceBase.Status.Stop : cri.bgm.status;

        // 判定、現在スピード、BPMの表示
        float t = isStart ? cri.bgm.time / 1000f : 0;
        progressText.text = 
            $"Time : {t}\n" +
            $"BPM  : {notesDirector.nowBpm}\n" +
            $"Speed: {notesController.nowSpeed}\n\n" +
            $"Excellent: {notesDirector.point[0]}\n" +
            $"Perfect  : {notesDirector.point[1]}\n" +
            $"Great    : {notesDirector.point[2]}\n" +
            $"Bad      : {notesDirector.point[3]}\n" +
            $"Miss     : {notesDirector.point[4]}\n" +
            $"Status   : {status.ToString()}";
        
        if (!isAudio)
        {
            if (musicTime > 0)
            {
                isAudio = true;
                cri.bgm.Play(0);
                waitTime = Time.realtimeSinceStartup;
            }
        }
        
        if (isPlaying)
            musicTime = Time.realtimeSinceStartup - waitTime;

        scoreText.text = notesDirector.score.ToString("D7");
        comboText.text = notesDirector.combo.ToString();
        
        // 終了判定
        if (status == CriAtomSourceBase.Status.PlayEnd && !isEnd)
        {
            isEnd = true;
            ResultData.point = notesDirector.point;
            ResultData.score = notesDirector.score;
            ResultData.difficult = GameData.difficult;
            ResultData.difficulty = GameData.difficulty;
            ResultData.title = GameData.title;
            ResultData.id = GameData.id;
            mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            mask.SetActive(true);
            mask.GetComponent<Image>().DOFade(1f, 1f).OnComplete(() =>
            {
                SceneManager.LoadScene("ResultScene");
            });
        }
    }

    public void StartStopButtonTap()
    {
        if (isStart && isPlaying)
        {
            if (isAudio)
                cri.bgm.Pause(true);
            
            isPlaying = !isPlaying;
            
            infoPanel.GetComponent<Image>().color = panelColor;
            infoPanel.SetActive(true);
            infoPanel.transform.GetChild(1).gameObject.SetActive(false);
            infoPanel.transform.GetChild(2).gameObject.SetActive(true);
        }
    }

    public void StartButtonTap()
    {
        if (isOk && !isStart)
        {
            isStart = true;
            isPlaying = true;
            
            waitTime = Time.realtimeSinceStartup - musicTime;

            tween.Kill();
            infoPanel.GetComponent<RectTransform>().GetChild(0).gameObject.SetActive(false);
            infoPanel.GetComponent<RectTransform>().GetChild(1).gameObject.SetActive(false);
            infoPanel.GetComponent<Image>().DOFade(endValue: 0f, duration: 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                infoPanel.GetComponent<Image>().color = panelColor;
                infoPanel.SetActive(false);
            });
        }
    }

    public void BackButtonTap()
    {
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        mask.SetActive(true);
        mask.GetComponent<Image>().DOFade(1f, 2f).OnComplete(() =>
        {
            SceneManager.LoadScene("SelectScene");
        });
    }

    public void RestartButtonTap()
    {
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        mask.SetActive(true);
        mask.GetComponent<Image>().DOFade(1f, 2f).OnComplete(() =>
        {
            SceneManager.LoadScene("GameScene");
        });
    }

    public void PlayButtonTap()
    {
        if (isAudio)
        {
            cri.bgm.Pause(false);
        }
        waitTime = Time.realtimeSinceStartup - musicTime;
        
        infoPanel.GetComponent<Image>().color = panelColor;
        infoPanel.SetActive(false);
        isPlaying = true;
    }
    

    //バックグラウンドに行っているか
    private bool _isBackground = false;
  
    private void OnApplicationPause(bool pauseStatus) {
        ChangeBackgroundStatus(pauseStatus);
    }

    private void OnApplicationFocus(bool hasFocus) {
        ChangeBackgroundStatus(!hasFocus);
    }

    //アプリがバックグラウンドにいるかのステータスを変更
    private void ChangeBackgroundStatus(bool isBackground) {
        if (isBackground == _isBackground) {
            return;
        }

        if (isBackground) {
            Debug.Log($"アプリがバックグラウンドへ");
            if (isStart && isPlaying)
            {
                StartStopButtonTap();
            }
        }
        else{
            Debug.Log($"アプリがバックグラウンドから復帰");
        }

        _isBackground = isBackground;
    }
}
