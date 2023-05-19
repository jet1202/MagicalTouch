using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JudgeController : MonoBehaviour
{
    private GameObject text;
    
    private void OnEnable()
    {
        GetComponent<Animator>().SetTrigger("Anim");
    }

    public void Setting(char kind)
    {
        text = transform.GetChild(0).GetChild(0).gameObject;
        
        switch (kind)
        {
            case 'P':
                text.GetComponent<Text>().text = "Perfect";
                text.GetComponent<Text>().color = new Color(255f / 255f, 255f / 255f, 0f / 255f, 1f);
                text.GetComponent<Outline>().effectColor = new Color(255f / 255f, 98f / 255f, 255f / 255f, 1f);
                break;
            case 'G':
                text.GetComponent<Text>().text = "Great";
                text.GetComponent<Text>().color = new Color(255f / 255f, 143f / 255f, 38f / 255f, 1f);
                text.GetComponent<Outline>().effectColor = new Color(107f / 255f, 206f / 255f, 255f / 255f, 1f);
                break;
            case 'B':
                text.GetComponent<Text>().text = "Bad";
                text.GetComponent<Text>().color = new Color(0f / 255f, 183f / 255f, 32f / 255f, 1f);
                text.GetComponent<Outline>().effectColor = new Color(255f / 255f, 146f / 255f, 0f / 255f, 1f);
                break;
            case 'M':
                text.GetComponent<Text>().text = "Miss";
                text.GetComponent<Text>().color = new Color(108f / 255f, 108f / 255f, 108f / 255f, 1f);
                text.GetComponent<Outline>().effectColor = new Color(255f / 255f, 0f / 255f, 0f / 255f, 1f);
                break;
        }
    }

    public void FinishAnimJudge()
    {
        transform.parent.GetComponent<MyObjectPool>().RemoveObject(this.gameObject);
    }
}
