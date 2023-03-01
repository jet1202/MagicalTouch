using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    [SerializeField] private Text timeText;
    
    public bool isPlaying = false;
    public float musicTime;
    public float waitTime;
    
    void Awake()
    {
        Time.timeScale = 0;
        musicTime = Time.time - waitTime;
    }
    
    void Update()
    {
        musicTime = Time.time - waitTime;

        timeText.text = musicTime.ToString("F2");
    }

    public void StartStopButtonTap()
    {
        isPlaying = !isPlaying;
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }
}
