using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    public float mov = 0;
    public float width;

    private SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        
        spriteRenderer.size = new Vector2(width, 1);
        GetComponent<Animator>().SetTrigger("Anim");
    }

    void Update()
    {
        spriteRenderer.size = new Vector2(width + mov, 1 + mov);
    }

    public void FinishAnimPaddle()
    {
        transform.parent.GetComponent<MyObjectPool>().RemoveObject(this.gameObject);
    }
}
