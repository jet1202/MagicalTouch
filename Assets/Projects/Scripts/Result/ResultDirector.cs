using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
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
    [SerializeField] private Sprite[] rankSprites;

    private Texture jacket;
    private Texture rankTexture;

    private Transform title, difficulty, score, combo, rank, tab1;
    
    IEnumerator Start()
    {

        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        mask.SetActive(true);
        
        title = resultCanvas.transform.GetChild(0);
        difficulty = resultCanvas.transform.GetChild(1);
        score = resultCanvas.transform.GetChild(2);
        combo = resultCanvas.transform.GetChild(3);
        rank = resultCanvas.transform.GetChild(4);
        tab1 = resultCanvas.transform.GetChild(5);
        
        // ロード
        int[] detail = ResultData.resultDetail;

        // データを表示
        title.GetChild(0).GetComponent<TextMeshProUGUI>().text = ResultData.title;
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().text = ResultData.difficult.ToString();
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().color = setColor(ResultData.difficult);
        difficulty.GetChild(1).GetComponent<TextMeshProUGUI>().text = (ResultData.difficulty / 10).ToString();
        score.GetChild(0).GetComponent<TextMeshProUGUI>().text = ResultData.score.ToString("D7");
        combo.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            $"<align=left>MaxCombo<line-height=0>\n<align=right><size=30>{ResultData.combo}</size>/{ResultData.maxCombo}";

        if (ResultData.isAuto)
        {
            rank.gameObject.SetActive(false);
            tab1.gameObject.SetActive(false);
        }
        else
        {
            // rank
            int r = GetRank(ResultData.score);
            if (r != 0 || detail[0] + detail[1] + detail[2] + detail[4] + detail[5] + detail[6] + detail[7] != 0)
                r++;
            rank.GetComponent<Image>().sprite = rankSprites[r];

            // tab1
            Transform scoreDetail = tab1.GetChild(0);
            scoreDetail.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                $"{detail[3]}\n{detail[2] + detail[4]}\n{detail[1] + detail[5]}\n{detail[0] + detail[6]}\n{detail[7]}\n";
        }

        // ジャケットのデータ
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
        combo.GetComponent<RectTransform>().localPosition += new Vector3(800, 0, 0);
        rank.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        rank.localScale = new Vector3(2f, 2f, 2f);
        
        mask.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() =>
        {
            mask.SetActive(false);
            MoveAnimation();
        });
    }

    public void MoveAnimation()
    {
        title.GetComponent<Image>().DOFillAmount(1f, 1f).SetEase(Ease.OutExpo);
        difficulty.GetComponent<RectTransform>().DOLocalMove(new Vector3(-800, 0, 0), 1f).SetEase(Ease.OutExpo).SetDelay(0.3f).SetRelative(true);
        score.GetComponent<RectTransform>().DOLocalMove(new Vector3(-800, 0, 0), 1f).SetEase(Ease.OutExpo).SetDelay(0.6f).SetRelative(true);
        combo.GetComponent<RectTransform>().DOLocalMove(new Vector3(-800, 0, 0), 1f).SetEase(Ease.OutExpo).SetDelay(0.9f).SetRelative(true);

        var boardSeq = DOTween.Sequence();
        boardSeq.AppendInterval(0.2f);
        boardSeq.Append(image.transform.DOLocalMove(new Vector3(-5f, 2f, 0f), 1f).SetEase(Ease.OutExpo));
        boardSeq.Join(image.transform.DORotate(Vector3.up * 0f, 1f).SetEase(Ease.OutExpo));
        boardSeq.Play();

        var rankSeq = DOTween.Sequence();
        rankSeq.AppendInterval(1.2f);
        rankSeq.Append(rank.GetComponent<Image>().DOFade(1f, 1.5f).SetEase(Ease.OutQuad));
        rankSeq.Join(rank.DOScale(new Vector3(1f, 1f, 1f), 1.5f).SetEase(Ease.InQuart));
        rankSeq.Play();
    }

    public int GetRank(int s)
    {
        int r = -1;
        if (s == 1000000) r = 0;
        else if (s >= 997500) r = 1;
        else if (s >= 995000) r = 2;
        else if (s >= 990000) r = 3;
        else if (s >= 980000) r = 4;
        else if (s >= 970000) r = 5;
        else if (s >= 950000) r = 6;
        else if (s >= 900000) r = 7;
        else if (s >= 800000) r = 8;
        else if (s >= 700000) r = 9;
        else if (s >= 600000) r = 10;
        else if (s >= 500000) r = 11;
        else r = 12;
        return r;
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
