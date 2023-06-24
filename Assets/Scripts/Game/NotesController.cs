using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesController : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    
    // 引継ぎ設定
    [SerializeField] public float Speed;
    
    // public CameraItem[] cameraData;
    // private int cameraProgress = -1;
    public int cameraMode;

    public SpeedItem[] speedData;
    public float[] accDis;

    private int speedProgress = -1;

    public float nowSpeed = 0;
    
    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
    }

    void Update()
    {
        if (gameDirector.isOk)
            transform.position = new Vector3(0, 0, -TimeToPos(gameDirector.musicTime) * Speed);
    }
    
    public float TimeToPos(float time)
    {
        float pos;
        if (speedProgress == -1)
        {
            pos = speedData[0].speed100 * time / 100f;
            if (time > 0)
                speedProgress = 0;
        }
        else
        {
            if (speedProgress < speedData.Length - 1)
            {
                if (time > speedData[speedProgress + 1].time100 / 100f)
                    speedProgress++;
            }

            pos = accDis[speedProgress];

            if (speedData[speedProgress].isVariation)
            {
                float t = time - speedData[speedProgress].time100 / 100f;
                pos += t * speedData[speedProgress].speed100 / 100f;
                float s = (speedData[speedProgress + 1].speed100 - speedData[speedProgress].speed100) / 
                          (float)(speedData[speedProgress + 1].time100 - speedData[speedProgress].time100) * t;
                pos += t * s / 2f;

                if (gameDirector.isOk)
                {
                    nowSpeed = speedData[speedProgress].speed100 / 100f + s;
                }
            }
            else
            {
                float t = time - speedData[speedProgress].time100 / 100f;
                pos += t * speedData[speedProgress].speed100 / 100f;
                
                if (gameDirector.isOk)
                {
                    nowSpeed = speedData[speedProgress].speed100 / 100f;
                }
            }
        }

        return pos;
    }

    public void BpmDataImport(SpeedItem[] data)
    {
        speedData = data;
        int leng = speedData.Length;
        accDis = new float[leng];
        accDis[0] = 0;

        float acc = 0;
        for (int i = 0; i < leng - 1; i++)
        {
            if (speedData[i].isVariation)
            {
                int time = speedData[i + 1].time100 - speedData[i].time100;
                float j = time * Math.Min(speedData[i].speed100, speedData[i + 1].speed100) / 10000f;
                j += time * Math.Abs(speedData[i].speed100 - speedData[i + 1].speed100) / 20000f;

                acc += j;
                accDis[i + 1] = acc;
            }
            else
            {
                acc += (speedData[i + 1].time100 - speedData[i].time100) * speedData[i].speed100 / 10000f;
                accDis[i + 1] = acc;
            }
        }
    }
}
