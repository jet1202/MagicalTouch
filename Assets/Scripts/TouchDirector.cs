using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TouchDirector : MonoBehaviour
{
    [SerializeField] private List<GameObject> laneArray;
    [SerializeField] private NotesDirector _notesDirector;
    private bool[] laneTouching = new bool[6];
    private bool[] lastLaneTouching = new bool[6];
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
        // それぞれのタッチがどのレーンをタッチしているのか認識
        activeTouchList = Touch.activeTouches;
        laneTouching = new bool[6];
        foreach (var touch in activeTouchList)
        {
            touchlane = TouchLane(touch.screenPosition);
            if (touchlane != -1)
                laneTouching[touchlane] = true;
            if (touch.phase == TouchPhase.Began)
                _notesDirector.BeginTouch(touchlane);
        }

        for (int i = 0; i < 6; i++)
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

    int TouchLane(Vector2 touchPos)
    {
        float posX = touchPos.x / _width * 14f;
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
                case 13:
                case 14:
                    return 5;
            }
        }
        return -1;
    }
}
