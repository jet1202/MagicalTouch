using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageController : MonoBehaviour
{
    private Color color;

    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        color = GetComponent<Image>().color;
        color = new Color(1f, 1f, 1f, 0f);
    }

    public void Damage()
    {
        animator.Play("DamageAnimation", 0, 0);
    }
}
