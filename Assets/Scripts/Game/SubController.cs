using System;
using System.Collections;
using System.Collections.Generic;
using CriWare.CriTimeline.Atom;
using UnityEngine;

public class SubController : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    public GameObject subNotes;
    
    public CameraWork[] cameraWork = Array.Empty<CameraWork>();

    public int[] activeTime = Array.Empty<int>();
    private int timeProg;
    
    private bool isActive;

    private void Start()
    {
        transform.rotation = Quaternion.identity;
        subNotes = transform.GetChild(1).gameObject;
        subNotes.SetActive(true);
        isActive = true;
        timeProg = 0;
    }
    
    void Update()
    {
        if (gameDirector.isOk)
        {
            transform.rotation = Quaternion.AngleAxis(-TimeToAngle(gameDirector.musicTime), Vector3.right);
            ActiveCheck(gameDirector.musicTime);
        }
    }

    public void ActiveCheck(float time)
    {
        if (timeProg == activeTime.Length) return;
        
        if (activeTime[timeProg] / 100f < time)
        {
            isActive = !isActive;
            subNotes.SetActive(isActive);
            timeProg++;
        }
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
            int a = cameraWork[index].angle % 360;
            return a;
        }
        else if (index == -1)
        {
            int a = cameraWork[0].angle % 360;
            return a;
        }
        else
        {
            CameraWork before = cameraWork[index];
            CameraWork after = cameraWork[index + 1];
            
            float T = time - (float)before.time100 / 100;
            float t1 = (after.time100 - before.time100) / 100f;
            float a1 = after.angle - before.angle;
            int v = before.variation;

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
}
