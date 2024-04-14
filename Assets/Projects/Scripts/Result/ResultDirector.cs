using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private Transform titleT, difficultyT, scoreT, comboT, rankT, tab1T, tab2T;

    private int tab = 0;
    
    IEnumerator Start()
    {

        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        mask.SetActive(true);
        
        titleT = resultCanvas.transform.GetChild(0);
        difficultyT = resultCanvas.transform.GetChild(1);
        scoreT = resultCanvas.transform.GetChild(2);
        comboT = resultCanvas.transform.GetChild(3);
        rankT = resultCanvas.transform.GetChild(4);
        tab1T = resultCanvas.transform.GetChild(5);
        tab2T = resultCanvas.transform.GetChild(6);
        
        // ロード
        int score = ResultData.score;
        int[] tapJudge = ResultData.tapJudge; // 31
        int[] detail = ResultData.resultDetail; // 5
        int[] pm = ResultData.pm; // 6
        int tapGapSum = ResultData.tapGapSum;
        int combo = ResultData.combo;
        int maxCombo = ResultData.maxCombo;
        bool isAuto = ResultData.isAuto;
        
        string title = ResultData.title;
        string id = ResultData.id;
        SelectData.DifficultyMode difficult = ResultData.difficult;
        int difficulty = ResultData.difficulty;

        int[] tapdetail = new int[5];
        for (int i = 0; i < 31; i++)
        {
            if (12 <= i && i <= 17)
                tapdetail[0] += tapJudge[i];
            else if (10 <= i && i <= 19)
                tapdetail[1] += tapJudge[i];
            else if (5 <= i && i <= 24)
                tapdetail[2] += tapJudge[i];
            else if (0 <= i && i <= 29)
                tapdetail[3] += tapJudge[i];
            else if (i == 30)
                tapdetail[4] += tapJudge[i];
        }

        int tapSum = tapJudge.Sum() - tapJudge[30];
        float tapAve = (float)-tapGapSum / tapSum;

        // データを表示
        titleT.GetChild(0).GetComponent<TextMeshProUGUI>().text = title;
        difficultyT.GetChild(0).GetComponent<TextMeshProUGUI>().text = difficult.ToString();
        difficultyT.GetChild(0).GetComponent<TextMeshProUGUI>().color = setColor(difficult);
        difficultyT.GetChild(1).GetComponent<TextMeshProUGUI>().text = (difficulty / 10).ToString();
        scoreT.GetChild(0).GetComponent<TextMeshProUGUI>().text = score.ToString("D7");
        comboT.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            $"<align=left>MaxCombo<line-height=0>\n<align=right><size=30>{combo}</size>/{maxCombo}";
        comboT.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            "<size=30><b>PP</b></size>\n" + ((float)detail[0]/maxCombo * 100).ToString("F2") + "%";

        if (isAuto)
        {
            rankT.gameObject.SetActive(false);
            tab1T.gameObject.SetActive(false);
            tab2T.gameObject.SetActive(false);
        }
        else
        {
            rankT.gameObject.SetActive(true);
            tab1T.gameObject.SetActive(true);
            tab2T.gameObject.SetActive(false);
            
            // rank
            int r = GetRank(score);
            if (r != 0 || detail[0] + detail[1] + detail[2] + detail[4] != 0)
                r++;
            rankT.GetComponent<Image>().sprite = rankSprites[r];

            // tab1
            Transform scoreDetailT = tab1T.GetChild(0);
            scoreDetailT.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                $"{detail[0]}\n{detail[1]}\n{detail[2]}\n{detail[3]}\n{detail[4]}\n";

            Transform graphT = tab1T.GetChild(1);
            int[] tap2 = new int[30];
            Array.Copy(tapJudge, 0, tap2, 0, 30);
            int max = tap2.Max();
            if (max == 0) max = 1;
            for (int i = 0; i < 30; i++)
                graphT.GetChild(0).GetChild(i).GetComponent<Image>().fillAmount = (float)tapJudge[29 - i] / max;
            graphT.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                tapAve.ToString("F1");
            
            // tab2
            Transform chartDetailT = tab2T.GetChild(0);
            for (int i = 0; i < 5; i++)
                chartDetailT.GetChild(1).GetChild(i+1).GetChild(0).GetComponent<TextMeshProUGUI>().text = 
                    tapdetail[i].ToString();
            for (int i = 0; i < 3; i++)
            {
                chartDetailT.GetChild(i + 2).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    pm[i * 2].ToString();
                chartDetailT.GetChild(i + 2).GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = 
                    pm[i * 2 + 1].ToString();
            }
        }

        // ジャケットのデータ
        IEnumerator corutine = GetComponent<ImportResult>().ImportJacket(id);
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
        m.SetColor("_color", setColor(difficult));

        // 動く前の位置につく
        titleT.GetComponent<Image>().fillAmount = 0f;
        difficultyT.GetComponent<RectTransform>().localPosition += new Vector3(800, 0, 0);
        scoreT.GetComponent<RectTransform>().localPosition += new Vector3(800, 0, 0);
        comboT.GetComponent<RectTransform>().localPosition += new Vector3(800, 0, 0);
        rankT.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        rankT.localScale = new Vector3(2f, 2f, 2f);
        
        mask.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() =>
        {
            mask.SetActive(false);
            MoveAnimation();
        });
    }

    private void MoveAnimation()
    {
        titleT.GetComponent<Image>().DOFillAmount(1f, 1f).SetEase(Ease.OutExpo);
        difficultyT.GetComponent<RectTransform>().DOLocalMove(new Vector3(-800, 0, 0), 1f).SetEase(Ease.OutExpo).SetDelay(0.3f).SetRelative(true);
        scoreT.GetComponent<RectTransform>().DOLocalMove(new Vector3(-800, 0, 0), 1f).SetEase(Ease.OutExpo).SetDelay(0.6f).SetRelative(true);
        comboT.GetComponent<RectTransform>().DOLocalMove(new Vector3(-800, 0, 0), 1f).SetEase(Ease.OutExpo).SetDelay(0.9f).SetRelative(true);

        var boardSeq = DOTween.Sequence();
        boardSeq.AppendInterval(0.2f);
        boardSeq.Append(image.transform.DOLocalMove(new Vector3(-5f, 2f, 0f), 1f).SetEase(Ease.OutExpo));
        boardSeq.Join(image.transform.DORotate(Vector3.up * 0f, 1f).SetEase(Ease.OutExpo));
        boardSeq.Play();

        var rankSeq = DOTween.Sequence();
        rankSeq.AppendInterval(1.2f);
        rankSeq.Append(rankT.GetComponent<Image>().DOFade(1f, 1.5f).SetEase(Ease.OutQuad));
        rankSeq.Join(rankT.DOScale(new Vector3(1f, 1f, 1f), 1.5f).SetEase(Ease.InQuart));
        rankSeq.Play();
    }

    private void ChangeTab()
    {
        tab = 1 - tab;
        tab1T.gameObject.SetActive(tab == 0);
        tab2T.gameObject.SetActive(tab == 1);
    }

    private int GetRank(int s)
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
    
    public void TabButtonTap()
    {
        ChangeTab();
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
