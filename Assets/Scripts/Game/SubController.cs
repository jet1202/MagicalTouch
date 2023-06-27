using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubController : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    
    public CameraWork[] cameraWork;

    public float nowAngle;

    private void Start()
    {
        transform.rotation = Quaternion.identity;
    }
    
    void Update()
    {
        if (gameDirector.isOk)
            transform.rotation = Quaternion.AngleAxis(-TimeToAngle(gameDirector.musicTime), Vector3.right);
    }

    public float TimeToAngle(float time)
    {
        int leng = cameraWork.Length;
        int index = leng - 1;
        for (int i = 0; i < leng; i++)
        {
            if (cameraWork[i].time100 > time * 100)
            {
                index = i - 1;
                break;
            }
        }

        if (index == leng - 1)
        {
            return cameraWork[index].angle;
        }
        else if (index == -1)
        {
            return cameraWork[0].angle;
        }
        else
        {
            CameraWork before = cameraWork[index];
            CameraWork after = cameraWork[index + 1];

            float T = time - (float)before.time100 / 100;
            float slope = (after.angle - before.angle) / ((float)(after.time100 - before.time100) / 100);

            float angle = before.angle + slope * T;

            return angle;
        }
    }
}
