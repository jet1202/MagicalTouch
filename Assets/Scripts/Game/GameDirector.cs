using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
    
    int frameCount;
    float prevTime;
    float fps;

    private Tween tween;
    private Color panelColor;

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

        // 判定、現在スピード、BPMの表示
        float t = isStart ? cri.bgm.time / 1000f : 0;
        progressText.text = 
            $"Time : {t}\n" +
            $"BPM  : {notesDirector.nowBpm}\n" +
            $"Speed: {notesController.nowSpeed}\n\n" +
            $"Excellent: {notesDirector.perfectP}\n" +
            $"Perfect  : {notesDirector.perfect}\n" +
            $"Great    : {notesDirector.great}\n" +
            $"Bad      : {notesDirector.bad}\n" +
            $"Miss     : {notesDirector.miss}\n";
        
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
    }

    public void StartStopButtonTap()
    {
        if (isStart)
        {
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                if (isAudio)
                {
                    cri.bgm.Pause(false);
                }

                waitTime = Time.realtimeSinceStartup - musicTime;
            }
            else
            {
                if (isAudio)
                {
                    cri.bgm.Pause(true);
                }
            }
        }
    }

    public void StartButtonTap()
    {
        if (isOk)
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
}
