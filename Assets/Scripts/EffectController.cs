using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public float mov = 0;
    public float width;

    private SpriteRenderer spriteRenderer;
    private float len;
    private float cur = 0;

    private void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        spriteRenderer.size = new Vector2(width, 1);

        AnimatorStateInfo animatorStateInfo = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
        len = animatorStateInfo.length;
    }

    void Update()
    {
        spriteRenderer.size = new Vector2(width + mov, 1 + mov);

        cur += Time.deltaTime;
        if (len <= cur)
        {
            Destroy(this.gameObject);
        }
    }
}
