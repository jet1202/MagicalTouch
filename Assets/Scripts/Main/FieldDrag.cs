using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FieldDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollController scrollController;
    
    public void OnBeginDrag(PointerEventData e)
    {
        scrollController.isFieldDragging = true;
        Debug.Log("ScrollStart");
    }

    public void OnDrag(PointerEventData e)
    {
        
    }

    public void OnEndDrag(PointerEventData e)
    {
        scrollController.isFieldDragging = false;
        Debug.Log("ScrollEnd");
    }
    
}
