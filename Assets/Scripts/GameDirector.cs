using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    [SerializeField] private NotesDirector notesDirector;
    [SerializeField] private NotesController notesController;
    [SerializeField] private Cri cri;
    [SerializeField] private Text timeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Text progressText;
    
    private AudioSource audioSource;
    public bool isPlaying = false;
    public float musicTime;
    public float waitTime;
    private bool isAudio = false;

    public bool isOk = false;
    
    int frameCount;
    float prevTime;
    float fps;

    void Awake()
    {
        Application.targetFrameRate = 60;
        
        Time.timeScale = 1;
        musicTime = -waitTime;
    }

    private void Start()
    {
        frameCount = 0;
        prevTime = 0.0f;
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
        float t = isOk ? cri.bgm.time / 1000f : 0;
        progressText.text = 
            $"Time : {t}\n" +
            $"BPM  : {notesDirector.nowBpm}\n" +
            $"Speed: {notesController.nowSpeed}\n\n" +
            $"Excellent: {notesDirector.excellent}\n" +
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
        if (isOk)
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
}
