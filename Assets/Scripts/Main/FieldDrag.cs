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
        scrollController.t.Kill();
        Debug.Log("FieldDragStart");
    }

    public void OnDrag(PointerEventData e)
    {
        _delta = e.delta;
        if (direction == -1)
        {
            // if (_delta.y > _delta.x)
            //     direction = 0; // 縦方向のスクロール
            // else
                direction = 1; // 横方向のスクロール
        }

        if (direction == 1)
        {
            // 縦
            float d = _delta.x;
            scrollbar.value -= d * (90f / 1200f) / 360f;
        }
        else
        {
            // 横
        }
    }

    public void OnEndDrag(PointerEventData e)
    {
        scrollController.isFieldDragging = false;
        
        if (direction == 1)
            scrollController.inertia = _delta.x * (90f / 1200f);
        
        direction = -1;
        _delta = Vector2.zero;
    }
    
}
