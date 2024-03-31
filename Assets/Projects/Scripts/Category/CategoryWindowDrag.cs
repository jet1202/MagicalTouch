using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CategoryWindowDrag : MonoBehaviour, IDragHandler
{
    public int h = 0;
    
    private int _width;
    private int _height;

    private void Start()
    {
        _width = Screen.width;
        _height = Screen.height;
    }

    public void OnDrag(PointerEventData e)
    {
        var dy = e.delta.y / _height * 600;

        var posy = transform.GetChild(0).GetComponent<RectTransform>().localPosition.y;
        posy = Math.Clamp(posy + dy, 200f, Math.Max(h - 200f, 200f));
        transform.GetChild(0).GetComponent<RectTransform>().localPosition = new Vector3(0f, posy, 0f);
    }
}