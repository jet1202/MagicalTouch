using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CriWare.CriTimeline.Atom;
using UnityEngine;
using UnityEngine.UIElements;

public class FieldController : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    public GameObject subNotes;
    private LineRenderer rline;
    private LineRenderer lline;
    
    // 引継ぎ設定
    public float Speed;

    public SpeedItem[] speedItem = Array.Empty<SpeedItem>();
    public float[] accDis;
    private int speedProgress = -1;
    public float nowSpeed = 0;

    public AngleWork[] angleWork = Array.Empty<AngleWork>();
    public TransparencyItem[] transparencyItem = Array.Empty<TransparencyItem>();
    
    private int timeProg;
    private bool isActive;

    private void Start()
    {
        transform.rotation = Quaternion.identity;
        subNotes = transform.GetChild(0).gameObject;
        rline = transform.GetChild(1).GetComponent<LineRenderer>();
        lline = transform.GetChild(2).GetComponent<LineRenderer>();
        isActive = true;
        timeProg = 0;
        
        Speed = ScoreData.setting.Game.NoteSpeed;
        subNotes.transform.position = new Vector3(0, 0, 0);
        subNotes.SetActive(true);
    }
    
    void Update()
    {
        if (gameDirector.isOk)
        {
            transform.rotation = Quaternion.AngleAxis(-TimeToAngle(gameDirector.musicTime), Vector3.right);
            
            float t = TimeToTransparency(gameDirector.musicTime);
            rline.startColor = new Color(1, 1, 1, t);
            rline.endColor = new Color(1, 1, 1, t);
            lline.startColor = new Color(1, 1, 1, t);
            lline.endColor = new Color(1, 1, 1, t);
            
            if (t == 0f)
                subNotes.SetActive(false);
            else
                subNotes.SetActive(true);
        }

        if (gameDirector.isOk)
        {
            subNotes.transform.localPosition = new Vector3(0, 0, -TimeToPos(gameDirector.musicTime) * Speed);
        }
    }
    
    public float TimeToPos(float time)
    {
        float pos;
        if (speedProgress == -1)
        {
            pos = speedItem[0].speed * time / 100f;
            if (time > 0)
                speedProgress = 0;
        }
        else
        {
            if (speedProgress < speedItem.Length - 1)
            {
                if (time > speedItem[speedProgress + 1].time / 1000f)
                    speedProgress++;
            }

            pos = accDis[speedProgress];

            if (speedItem[speedProgress].isVariation)
            {
                float t = time - speedItem[speedProgress].time / 1000f;
                pos += t * speedItem[speedProgress].speed / 100f;
                float s = (speedItem[speedProgress + 1].speed - speedItem[speedProgress].speed) / 
                    (float)(speedItem[speedProgress + 1].time - speedItem[speedProgress].time) * t * 10f;
                pos += t * s / 2f;

                if (gameDirector.isOk)
                {
                    nowSpeed = speedItem[speedProgress].speed / 100f + s;
                }
            }
            else
            {
                float t = time - speedItem[speedProgress].time / 1000f;
                pos += t * speedItem[speedProgress].speed / 100f;
                
                if (gameDirector.isOk)
                {
                    nowSpeed = speedItem[speedProgress].speed / 100f;
                }
            }
        }

        return pos;
    }
    
    public float TimeToTransparency(float time)
    {
        if (timeProg == transparencyItem.Length)
            return transparencyItem[timeProg - 1].alpha / 100f;
        
        if (transparencyItem[timeProg].time / 1000f < time)
            timeProg++;
        
        if (timeProg == transparencyItem.Length)
            return transparencyItem[timeProg - 1].alpha / 100f;
        
        if (timeProg == 0)
            return transparencyItem[0].alpha / 100f;
        
        TransparencyItem before = transparencyItem[timeProg - 1];
        TransparencyItem after = transparencyItem[timeProg];

        float t;
        if (before.isVariation)
        {
            float T = time - before.time / 1000f;
            float t1 = (after.time - before.time) / 1000f;
            float v = (after.alpha - before.alpha) / 100f;
            
            t = before.alpha / 100f + v * T / t1;
        }
        else
        {
            t = before.alpha / 100f;
        }

        return t;
    }

    public float TimeToAngle(float time)
    {
        int leng = angleWork.Length;
        if (leng == 0) return 0;
        
        int index = leng - 1;
        for (int i = 0; i < leng; i++)
        {
            if (angleWork[i].time > time * 1000)
            {
                index = i - 1;
                break;
            }
        }

        if (index == leng - 1)
        {
            int a = angleWork[index].angle % 360;
            return a;
        }
        else if (index == -1)
        {
            int a = angleWork[0].angle % 360;
            return a;
        }
        else
        {
            AngleWork before = angleWork[index];
            AngleWork after = angleWork[index + 1];
            
            float T = time - (float)before.time / 1000;
            float t1 = (after.time - before.time) / 1000f;
            float a1 = after.angle - before.angle;
            float v = before.variation / 10f;

            float a;
            if (v > 0)
            {
                a = a1 * (float)Math.Pow(T / t1, v);
            }
            else if (v < 0)
            {
                a = a1 * (float)Math.Pow(T / t1, -1.0f / v);
            }
            else
            {
                a = 0;
            }

            float angle = (before.angle + a) % 360;

            return angle;
        }
    }
    
    public void ItemImport(SpeedItem[] data, AngleWork[] angleData, TransparencyItem[] transparencyData)
    {
        angleWork = angleData;
        transparencyItem = transparencyData;
        
        speedItem = data;
        int leng = speedItem.Length;
        accDis = new float[leng];
        accDis[0] = 0;

        float acc = 0;
        for (int i = 0; i < leng - 1; i++)
        {
            if (speedItem[i].isVariation)
            {
                float time = (speedItem[i + 1].time - speedItem[i].time) / 1000f;
                float j = time * Math.Min(speedItem[i].speed, speedItem[i + 1].speed) / 100f;
                j += time * Math.Abs(speedItem[i].speed - speedItem[i + 1].speed) / 200f;

                acc += j;
                accDis[i + 1] = acc;
            }
            else
            {
                acc += ((speedItem[i + 1].time - speedItem[i].time) / 1000f) * (speedItem[i].speed / 100f);
                accDis[i + 1] = acc;
            }
        }
    }
}
