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
    public bool[] laneTouching = new bool[24];
    public bool[] laneFlicking = new bool[24];
    private bool[] lastLaneTouching = new bool[24];
    private ReadOnlyArray<Touch> activeTouchList;
    private int _height, _width, touchlane;
    private int moveAllow;
    
    private bool isAuto;

    private int basePoint, pW;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        _height = Screen.height;
        _width = Screen.width;

        int h;
        if ((float)_width / _height < 1.6f)
            h = (int)(_width / 1.6f);
        else
            h = _height;
        pW = h / 10;
        basePoint = _width / 2 - pW * 6;
        
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
        int r = ((int)touchPos.x - basePoint) / (pW / 2);
        return r;
    }
}
