using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Vector2 = UnityEngine.Vector2;

public class TouchDirector : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    [SerializeField] private List<GameObject> laneArray;
    [SerializeField] private NotesDirector _notesDirector;
    public bool[] laneTouching = new bool[12];
    public bool[] laneFlicking = new bool[12];
    private bool[] lastLaneTouching = new bool[12];
    private ReadOnlyArray<Touch> activeTouchList;
    private int _height, _width, touchlane;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        _height = Screen.height;
        _width = Screen.width;
    }

    void Update()
    {
        if (gameDirector.isPlaying)
        {
            // それぞれのタッチがどのレーンをタッチしているのか認識
            activeTouchList = Touch.activeTouches;
            laneTouching = new bool[12];
            laneFlicking = new bool[12];
            foreach (var touch in activeTouchList)
            {
                touchlane = TouchLane(touch.screenPosition);
                if (touchlane != -1)
                {
                    laneTouching[touchlane] = true;
                    Vector2 move = touch.delta;
                    if (move.x * move.x + move.y * move.y >= 250)
                        laneFlicking[touchlane] = true;
                }
                if (touch.began)
                    _notesDirector.BeginTouch(touchlane, touch.startTime);
            }

            for (int i = 0; i < 12; i++)
            {
                if (laneTouching[i] != lastLaneTouching[i])
                {
                    if (laneTouching[i])
                        laneArray[i].GetComponent<MeshRenderer>().enabled = true;
                    else
                        laneArray[i].GetComponent<MeshRenderer>().enabled = false;
                }
            }

            lastLaneTouching = laneTouching;
        }
    }

    int TouchLane(Vector2 touchPos)
    {
        float posX = touchPos.x / _width * 26f;
        float posY = touchPos.y / _height;
        if (posY < 0.5)
        {
            switch ((int)posX)
            {
                case 0:
                case 1:
                case 2:
                    return 0;
                case 3:
                case 4:
                    return 1;
                case 5:
                case 6:
                    return 2;
                case 7:
                case 8:
                    return 3;
                case 9:
                case 10:
                    return 4;
                case 11:
                case 12:
                    return 5;
                case 13:
                case 14:
                    return 6;
                case 15:
                case 16:
                    return 7;
                case 17:
                case 18:
                    return 8;
                case 19:
                case 20:
                    return 9;
                case 21:
                case 22:
                    return 10;
                case 23:
                case 24:
                case 25:
                case 26:
                    return 11;
            }
        }
        return -1;
    }
}
