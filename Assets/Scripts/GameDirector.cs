using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    [SerializeField] private NotesDirector notesDirector;
    [SerializeField] private Cri cri;
    [SerializeField] private Text timeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    
    private AudioSource audioSource;
    public bool isPlaying = false;
    public float musicTime;
    public float waitTime;
    private bool isAudio = false;

    public bool isOk = false;

    void Awake()
    {
        Time.timeScale = 1;
        musicTime = -waitTime;
    }

    private void Start()
    {
        // audioSource = this.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isOk)
            timeText.text = $"{cri.bgm.time / 1000f}";

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
                cri.bgm.Pause(true);
            }
        }
    }
}
