using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameDirector : MonoBehaviour
{
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
    }

    public void StartStopButtonTap()
    {
        isPlaying = !isPlaying;
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }
}
