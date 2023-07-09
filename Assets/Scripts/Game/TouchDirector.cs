using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Vector2 = UnityEngine.Vector2;

public class TouchDirector : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    [SerializeField] private NotesDirector _notesDirector;
    [SerializeField] private NotesController notesController;
    public bool[] laneTouching = new bool[24];
    public bool[] laneFlicking = new bool[24];
    private bool[] lastLaneTouching = new bool[24];
    private ReadOnlyArray<Touch> activeTouchList;
    private int _height, _width, touchlane;
    private int moveAllow;
    
    private bool isAuto;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        _height = Screen.height;
        _width = Screen.width;
        moveAllow = (_height / 80) * (_height / 80);

        isAuto = ScoreData.setting.Game.IsAuto;
        if (isAuto)
        {
            laneTouching = Enumerable.Repeat<bool>(true, 24).ToArray();
            laneFlicking = Enumerable.Repeat<bool>(true, 24).ToArray();
        }
    }

    void Update()
    {
        if (gameDirector.isPlaying && !isAuto)
        {
            // それぞれのタッチがどのレーンをタッチしているのか認識
            activeTouchList = Touch.activeTouches;
            laneTouching = new bool[24];
            laneFlicking = new bool[24];
            foreach (var touch in activeTouchList)
            {
                touchlane = TouchLane(touch.screenPosition);
                if (touchlane != -1)
                {
                    laneTouching[touchlane] = true;
                    Vector2 move = touch.delta;
                    if (move.x * move.x + move.y * move.y >= moveAllow)
                        laneFlicking[touchlane] = true;
                }
                if (touch.began)
                    _notesDirector.BeginTouch(touchlane, touch.startTime);
            }
            
            // for (int i = 0; i < 24; i++)
            // {
            //     if (laneTouching[i] != lastLaneTouching[i])
            //     {
            //         if (laneTouching[i])
            //             laneArray[i/2].GetComponent<MeshRenderer>().enabled = true;
            //         else
            //             laneArray[i/2].GetComponent<MeshRenderer>().enabled = false;
            //     }
            // }

            lastLaneTouching = laneTouching;
        }
    }

    int TouchLane(Vector2 touchPos)
    {
        float posX = touchPos.x / _width * 28f;
        float posY = touchPos.y / _height;
        // if (notesController.cameraMode == 0)
        // {
            switch ((int)posX)
            {
                case 0:
                case 1:
                case 2:
                    return 0;
                case 3:
                    return 1;
                case 4:
                    return 2;
                case 5:
                    return 3;
                case 6:
                    return 4;
                case 7:
                    return 5;
                case 8:
                    return 6;
                case 9:
                    return 7;
                case 10:
                    return 8;
                case 11:
                    return 9;
                case 12:
                    return 10;
                case 13:
                    return 11;
                case 14:
                    return 12;
                case 15:
                    return 13;
                case 16:
                    return 14;
                case 17:
                    return 15;
                case 18:
                    return 16;
                case 19:
                    return 17;
                case 20:
                    return 18;
                case 21:
                    return 19;
                case 22:
                    return 20;
                case 23:
                    return 21;
                case 24:
                    return 22;
                case 25:
                case 26:
                case 27:
                case 28:
                    return 23;
            }
        // }
        return -1;
    }
}
