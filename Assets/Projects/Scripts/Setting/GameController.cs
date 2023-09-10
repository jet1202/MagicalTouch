using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private SettingCri cri;
    [SerializeField] private GameObject justFlame;
    [SerializeField] private GameObject filter;
    
    private float musicTime;
    private float waitTime;
    private float musicOffset;
    
    private float speed;
    private float thickness;

    private GameObject first, second, third;

    private int metronomeNumber;
    private bool isFade;
    
    private void Start()
    {
        metronomeNumber = 0;
        
        first = transform.GetChild(0).gameObject;
        second = transform.GetChild(1).gameObject;
        third = transform.GetChild(2).gameObject;
        
        ChangePosition();
        ChangeThickness();
        ChangeMusicOffset();
        
        waitTime = Time.realtimeSinceStartup + 1f;
    }

    private void Update()
    {
        musicTime = Time.realtimeSinceStartup - waitTime;
        
        transform.localPosition = new Vector3(0f, 0f, -(musicTime % 2f) * speed);

        if ((musicTime + musicOffset) > metronomeNumber * 0.5f)
        {
            if ((metronomeNumber - 3) % 4 == 0)
            {
                cri.metronome.Play(1);
            }
            else
                cri.metronome.Play(0);

            metronomeNumber++;
        }

        if (isFade)
        {
            if (musicTime % 2f < 1f)
                isFade = false;
        }
        else
        {
            if (musicTime % 2f > 1.5f)
            {
                filter.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 100f / 255f);
                filter.GetComponent<Renderer>().material.DOFade(0f, 0.5f);
                isFade = true;
            }
        }
    }

    public void ChangePosition()
    {
        speed = ScoreData.setting.Game.NoteSpeed;
        
        first.transform.localPosition = new Vector3(-3f, 0f, speed * 1.5f);
        second.transform.localPosition = new Vector3(-3f, 0f, speed * 3.5f);
        third.transform.localPosition = new Vector3(-3f, 0f, speed * 5.5f);
    }

    public void ChangeThickness()
    {
        thickness = ScoreData.setting.Game.NoteThickness / 10f;

        first.GetComponent<SpriteRenderer>().size = new Vector2(4, thickness);
        second.GetComponent<SpriteRenderer>().size = new Vector2(4, thickness);
        third.GetComponent<SpriteRenderer>().size = new Vector2(4, thickness);
        justFlame.GetComponent<SpriteRenderer>().size = new Vector2(6.06f, thickness);
    }

    public void ChangeMusicOffset()
    {
        musicOffset = ScoreData.setting.Game.SongOffset / 1000f;
    }
}
