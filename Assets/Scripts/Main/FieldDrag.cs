using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FieldDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollController scrollController;
    [SerializeField] private Scrollbar scrollbar;
    private int direction = -1;
    private Vector2 _delta;
    
    public void OnBeginDrag(PointerEventData e)
    {
        scrollController.isFieldDragging = true;
        scrollController.isScrolling = false;
        scrollController.horizontalTweener.Kill();
        scrollController.verticalTweener.Complete();
    }

    public void OnDrag(PointerEventData e)
    {
        _delta = e.delta;
        if (direction == -1)
        {
            if (Math.Abs(_delta.y) > Math.Abs(_delta.x))
                direction = 0; // 縦方向のスクロール
            else
                direction = 1; // 横方向のスクロール
        }

        if (direction == 1)
        {
            // 横
            float d = _delta.x;
            scrollbar.value -= scrollController.leng - 1 == 0 ? 0 : (d / 800f) / (scrollController.leng - 1);
        }
        else
        {
            // 縦
            float d = _delta.y;
            scrollController.CubeRotation(d / 6);
        }
    }

    public void OnEndDrag(PointerEventData e)
    {
        scrollController.isFieldDragging = false;

        if (direction == 1)
        {
            scrollController.inertia = _delta.x * (90f / 1200f);
            scrollController.isScrolling = true;
        }
        else
        {
            int d, n;
            if (_delta.y > 10f)
            {
                d = (int)Math.Ceiling(scrollController.cubeRotate / 90) * 90;
            }
            else if (_delta.y < -10f)
            {
                d = (int)Math.Floor(scrollController.cubeRotate / 90) * 90;
            }
            else
            {
                d = (int)Math.Round(scrollController.cubeRotate / 90) * 90;
            }

            n = -(d / 90) % 4;
            if (n < 0) n += 4;
            scrollController.ChangeDifficulty(n);
            scrollController.AdjustDifficulty(d, true);
        }

        direction = -1;
        _delta = Vector2.zero;
    }
    
}
