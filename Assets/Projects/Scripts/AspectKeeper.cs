using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AspectKeeper : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private List<RectTransform> targetCanvases;
    
    [SerializeField] private Vector2 targetAspect = new Vector2(1.6f, 2f);

    private void Update()
    {
        Vector2 scVec = new Vector2(Screen.width, Screen.height);
        var screenAspect = scVec.x / scVec.y;

        var viewportRect = new Rect(0, 0, 1, 1);
        var uiAdjust = 0f;
        var uiScale = 1f;

        float magRate = 1;
        if (screenAspect < targetAspect.x)
        {
            magRate = targetAspect.x / screenAspect;
            viewportRect.height = 1 / magRate;
            viewportRect.y = (1 - viewportRect.height) / 2;

            uiAdjust = 600 * (screenAspect - targetAspect.x) / 2;
            uiScale = 1 / magRate;
        }
        else if (screenAspect > targetAspect.y)
        {
            magRate = targetAspect.y / screenAspect;
            viewportRect.width = magRate;
            viewportRect.x = (1 - viewportRect.width) / 2;

            uiAdjust = 600 * (screenAspect - targetAspect.y) / 2;
        }
        
        targetCamera.rect = viewportRect;

        foreach (var t in targetCanvases)
        {
            t.offsetMin = new Vector2(uiAdjust, 0);
            t.offsetMax = new Vector2(-uiAdjust, 0);
            t.localScale = new Vector3(uiScale, uiScale, 1);
        }
    }
}
