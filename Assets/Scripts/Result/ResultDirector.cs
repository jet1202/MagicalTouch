using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultDirector : MonoBehaviour
{
    [SerializeField] private GameObject resultCanvas;
    [SerializeField] private GameObject backImage;
    [SerializeField] private GameObject image;
    [SerializeField] private GameObject mask;

    private Texture jacket;

    private Transform title, difficulty, score, scoreDetail, rank;
    
    void Start()
    {
        title = resultCanvas.transform.GetChild(0);
        difficulty = resultCanvas.transform.GetChild(1);
        score = resultCanvas.transform.GetChild(2);
        scoreDetail = resultCanvas.transform.GetChild(3);
        rank = resultCanvas.transform.GetChild(4);
        
        // 値を代入
        title.GetChild(0).GetComponent<TextMeshProUGUI>().text = ResultData.title;
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().text = ResultData.difficult;
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().color = setColor(ResultData.difficult);
        difficulty.GetChild(1).GetComponent<TextMeshProUGUI>().text = (ResultData.difficulty / 10).ToString();
        score.GetComponent<TextMeshProUGUI>().text = ResultData.score.ToString("D8");
        scoreDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text = String.Join('\n', ResultData.point);

        // 動く前の位置につく
        title.GetComponent<Image>().fillAmount = 0f;
        difficulty.GetComponent<RectTransform>().localPosition += new Vector3(500, 0, 0);
        score.GetComponent<RectTransform>().localPosition += new Vector3(500, 0, 0);
        scoreDetail.GetComponent<RectTransform>().localPosition += new Vector3(500, 0, 0);

        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        mask.SetActive(true);
        mask.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() =>
        {
            mask.SetActive(false);
            MoveAnimation();
        });
    }

    public void MoveAnimation()
    {
        title.GetComponent<Image>().DOFillAmount(1f, 1f).SetEase(Ease.OutQuint);
        difficulty.GetComponent<RectTransform>().DOLocalMove(new Vector3(-500, 0, 0), 1f).SetEase(Ease.OutQuint).SetDelay(0.2f).SetRelative(true);
        score.GetComponent<RectTransform>().DOLocalMove(new Vector3(-500, 0, 0), 1f).SetEase(Ease.OutQuint).SetDelay(0.5f).SetRelative(true);
        scoreDetail.GetComponent<RectTransform>().DOLocalMove(new Vector3(-500, 0, 0), 1f).SetEase(Ease.OutQuint).SetDelay(0.3f).SetRelative(true);

        var boardSeq = DOTween.Sequence();
        boardSeq.Append(image.transform.DOLocalMove(new Vector3(-4.5f, 1f, 0f), 1f).SetEase(Ease.OutQuint));
        boardSeq.Join(image.transform.DORotate(Vector3.up * 0f, 1f).SetEase(Ease.OutQuint));
        boardSeq.Play();
    }

    public Color setColor(string difficult)
    {
        Color color = new Color();
        switch (difficult)
        {
            case "Normal":
                color = new Color(100f / 255f, 255f / 255f, 100f / 255f);
                break;
            case "Hard":
                color = new Color(100f / 255f, 100f / 255f, 255f / 255f);
                break;
            case "Expert":
                color = new Color(255f / 255f, 255f / 255f, 100f / 255f);
                break;
            case "Impossible":
                color = new Color(255f / 255f, 100f / 255f, 100f / 255f);
                break;
            default:
                color = new Color(100f / 255f, 100f / 255f, 100f / 255f);
                break;
        }

        return color;
    }
}
