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
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            isPlaying = !isPlaying;
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        }

        musicTime = Time.time - waitTime;
    }
}
