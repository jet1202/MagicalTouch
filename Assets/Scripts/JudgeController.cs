using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeController : MonoBehaviour
{
    private float len;
    private float cur = 0;
    
    private void Start()
    {
        Animator anim = GetComponent<Animator>();
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        len = stateInfo.length;
    }

    private void Update()
    {
        cur += Time.deltaTime;
        if (cur >= len)
        {
            Destroy(this.gameObject);
        }
    }
}
