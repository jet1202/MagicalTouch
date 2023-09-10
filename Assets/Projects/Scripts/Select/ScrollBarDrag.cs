using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ScrollBarDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollController scrollController;
    
    public void OnBeginDrag(PointerEventData e)
    {
        scrollController.isScrollDragging = true;
        scrollController.isScrolling = false;
        scrollController.horizontalTweener.Kill();
        scrollController.verticalTweener.Complete();
    }

    public void OnEndDrag(PointerEventData e)
    {
        scrollController.isScrollDragging = false;
        scrollController.AdjustPosition();
    }
    
}
