using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JudgeController : MonoBehaviour
{
    private GameObject image;
    [SerializeField] private Sprite perfectImage;
    [SerializeField] private Sprite greatImage;
    [SerializeField] private Sprite badImage;
    [SerializeField] private Sprite missImage;
    
    private void OnEnable()
    {
        GetComponent<Animator>().SetTrigger("Anim");
    }

    public void Setting(char kind)
    {
        image = transform.GetChild(0).GetChild(0).gameObject;
        
        switch (kind)
        {
            case 'P':
                image.GetComponent<Image>().sprite = perfectImage;
                break;
            case 'G':
                image.GetComponent<Image>().sprite = greatImage;
                break;
            case 'B':
                image.GetComponent<Image>().sprite = badImage;
                break;
            case 'M':
                image.GetComponent<Image>().sprite = missImage;
                break;
        }
    }

    public void FinishAnimJudge()
    {
        transform.parent.GetComponent<MyObjectPool>().RemoveObject(this.gameObject);
    }
}
