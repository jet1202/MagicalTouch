using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    [SerializeField] private Text timeText;

    private AudioSource audioSource;
    public bool isPlaying = false;
    public float musicTime;
    public float waitTime;
    private bool isAudio = false;

    public bool isOk = false;
    
    void Awake()
    {
        Time.timeScale = 0;
        musicTime = Time.fixedTime - waitTime;
    }

    private void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }

    void Update()
    {

        timeText.text = audioSource.time.ToString("F2");

        if (isAudio)
        {
            // musicTime = audioSource.time;
        }
        else
        {
            if (musicTime > 0)
            {
                isAudio = true;
                audioSource.Play();
                musicTime = 0;
            }
        }
        musicTime = Time.fixedTime - waitTime;
    }

    public void StartStopButtonTap()
    {
        if (isOk)
        {
            isPlaying = !isPlaying;
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
            if (isPlaying)
            {
                if (isAudio)
                    audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }
}
