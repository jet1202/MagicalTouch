using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TouchDirector : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    [SerializeField] private List<GameObject> laneArray;
    [SerializeField] private NotesDirector _notesDirector;
    private bool[] laneTouching = new bool[12];
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
            Debug.Log(activeTouchList.Count);
            foreach (var touch in activeTouchList)
            {
                touchlane = TouchLane(touch.screenPosition);
                if (touchlane != -1)
                    laneTouching[touchlane] = true;
                if (touch.phase == TouchPhase.Began)
                    _notesDirector.BeginTouch(touchlane);
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
        float posX = touchPos.x / _width * 14f;
        float posY = touchPos.y / _height;
        if (posY < 0.5)
        {
            switch ((int)posX)
            {
                case 0:
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 3:
                    return 2;
                case 4:
                    return 3;
                case 5:
                    return 4;
                case 6:
                    return 5;
                case 7:
                    return 6;
                case 8:
                    return 7;
                case 9:
                    return 8;
                case 10:
                    return 9;
                case 11:
                    return 10;
                case 12:
                case 13:
                case 14:
                    return 11;
            }
        }
        return -1;
    }
}
