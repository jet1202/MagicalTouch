using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollBarDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollController scrollController;
    
    public void OnBeginDrag(PointerEventData e)
    {
        scrollController.isScrollDragging = true;
        Debug.Log("ScrollStart");
    }

    public void OnDrag(PointerEventData e)
    {
        
    }

    public void OnEndDrag(PointerEventData e)
    {
        scrollController.isScrollDragging = false;
        scrollController.adjustPosition();
        Debug.Log("ScrollEnd");
    }
    
}
