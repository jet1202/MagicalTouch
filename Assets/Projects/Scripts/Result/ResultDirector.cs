using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Texture defaultJacket;

    private Texture jacket;

    private Transform title, difficulty, score, scoreDetail, rank;
    
    IEnumerator Start()
    {

        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        mask.SetActive(true);
        
        title = resultCanvas.transform.GetChild(0);
        difficulty = resultCanvas.transform.GetChild(1);
        score = resultCanvas.transform.GetChild(2);
        scoreDetail = resultCanvas.transform.GetChild(3);
        rank = resultCanvas.transform.GetChild(4);
        
        // ロード

        int[] detail = ResultData.resultDetail;

        title.GetChild(0).GetComponent<TextMeshProUGUI>().text = ResultData.title;
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().text = ResultData.difficult.ToString();
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().color = setColor(ResultData.difficult);
        difficulty.GetChild(1).GetComponent<TextMeshProUGUI>().text = (ResultData.difficulty / 10).ToString();
        score.GetChild(0).GetComponent<TextMeshProUGUI>().text = ResultData.score.ToString("D7");
        scoreDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text = 
            $"{detail[3]}\n{detail[2] + detail[4]}\n{detail[1] + detail[5]}\n{detail[0] + detail[6]}\n{detail[7]}\n";
        
        IEnumerator corutine = GetComponent<ImportResult>().ImportJacket(ResultData.id);
        yield return StartCoroutine(corutine);
        if (corutine.Current == null)
            jacket = defaultJacket;
        else
            jacket = (Texture)corutine.Current;
        backImage.GetComponent<RawImage>().texture = jacket;
        backImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)jacket.width / jacket.height;

        var m = image.GetComponent<Renderer>().material;
        m.SetFloat("_ImageSize", 0.85f);
        m.SetTexture("_Image", jacket);
        m.SetColor("_color", setColor(ResultData.difficult));

        // 動く前の位置につく
        title.GetComponent<Image>().fillAmount = 0f;
        difficulty.GetComponent<RectTransform>().localPosition += new Vector3(800, 0, 0);
        score.GetComponent<RectTransform>().localPosition += new Vector3(800, 0, 0);
        scoreDetail.GetComponent<RectTransform>().localPosition += new Vector3(800, 0, 0);
        rank.GetComponent<TextMeshProUGUI>().color = new Color(0f, 0f, 0f, 0f);
        
        mask.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() =>
        {
            mask.SetActive(false);
            MoveAnimation();
        });
    }

    public void MoveAnimation()
    {
        title.GetComponent<Image>().DOFillAmount(1f, 1f).SetEase(Ease.OutExpo);
        difficulty.GetComponent<RectTransform>().DOLocalMove(new Vector3(-800, 0, 0), 1f).SetEase(Ease.OutExpo).SetDelay(0.2f).SetRelative(true);
        score.GetComponent<RectTransform>().DOLocalMove(new Vector3(-800, 0, 0), 1f).SetEase(Ease.OutExpo).SetDelay(0.5f).SetRelative(true);
        scoreDetail.GetComponent<RectTransform>().DOLocalMove(new Vector3(-800, 0, 0), 1f).SetEase(Ease.OutExpo).SetDelay(0.3f).SetRelative(true);

        var boardSeq = DOTween.Sequence();
        boardSeq.AppendInterval(0.2f);
        boardSeq.Append(image.transform.DOLocalMove(new Vector3(-4.5f, 1f, 0f), 1f).SetEase(Ease.OutExpo));
        boardSeq.Join(image.transform.DORotate(Vector3.up * 0f, 1f).SetEase(Ease.OutExpo));
        boardSeq.Play();
    }

    public Color setColor(SelectData.DifficultyMode difficult)
    {
        Color color = new Color();
        switch (difficult)
        {
            case SelectData.DifficultyMode.Free:
                color = new Color(100f / 255f, 255f / 255f, 100f / 255f);
                break;
            case SelectData.DifficultyMode.Normal:
                color = new Color(100f / 255f, 100f / 255f, 255f / 255f);
                break;
            case SelectData.DifficultyMode.Busy:
                color = new Color(255f / 255f, 255f / 255f, 100f / 255f);
                break;
            case SelectData.DifficultyMode.Impossible:
                color = new Color(255f / 255f, 100f / 255f, 100f / 255f);
                break;
            default:
                color = new Color(100f / 255f, 100f / 255f, 100f / 255f);
                break;
        }

        return color;
    }

    public void RestartButtonTap()
    {
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        mask.SetActive(true);
        mask.GetComponent<Image>().DOFade(1f, 1f).OnComplete(() =>
        {
            SceneManager.LoadScene("GameScene");
        });
    }

    public void NextButtonTap()
    {
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        mask.SetActive(true);
        mask.GetComponent<Image>().DOFade(1f, 1f).OnComplete(() =>
        {
            SceneManager.LoadScene("SelectScene");
        });
    }
}
