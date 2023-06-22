using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SubDirector : MonoBehaviour
{
    [SerializeField] private GameObject subBoard;
    [SerializeField] private GameObject subCanvas;
    [SerializeField] private GameObject mask;
    [SerializeField] private ImportScore importScore;
    [SerializeField] private ScrollController scrollController;

    private SongInfo songDetail;
    private SongInfo songChart;

    private Transform back, difficulty, info, tips;
    
    public IEnumerator MoveGameFromSelect(SongDataList song)
    {
        IEnumerator corutine;
        var difficult = SelectData.difficulty;
        
        corutine = importScore.ImportInfo($"{song.id}/songDetail.json");
        yield return StartCoroutine(corutine);
        songDetail = (SongInfo)corutine.Current;

        corutine = importScore.ImportInfo($"{song.id}/{difficult.ToString()}/songChart.json");
        yield return StartCoroutine(corutine);
        songChart = (SongInfo)corutine.Current;

        back = subCanvas.transform.GetChild(0);
        difficulty = subCanvas.transform.GetChild(1);
        info = subCanvas.transform.GetChild(2);
        tips = subCanvas.transform.GetChild(3);

        back.GetChild(0).GetComponent<TextMeshProUGUI>().text = song.title;
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().text = difficult.ToString();
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().color = scrollController.GetColor(difficult);
        difficulty.GetChild(1).GetComponent<TextMeshProUGUI>().text = (song.constant[(int)difficult] / 10).ToString();

        var first = new List<string>();
        var second = new List<string>();
        string[] separate = $"{songDetail.data},{songChart.data}".Split(new char[]{',', ':'});
        for (int i = 0; i < separate.Length; i++)
        {
            if (i % 2 == 0) first.Add(separate[i]);
            else second.Add(separate[i]);
        }

        info.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            String.Join('\n', first);
        info.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            ": " + String.Join("\n: ", second);
        
        // tips
        
        // 動く前の位置につく
        back.GetComponent<Image>().fillAmount = 0f;
        difficulty.GetComponent<RectTransform>().localPosition += new Vector3(300, 0, 0);
        info.GetComponent<RectTransform>().localPosition += new Vector3(580, 0, 0);
        tips.GetComponent<RectTransform>().localPosition += new Vector3(0, -30, 0);
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
    }

    public void MoveAnimation()
    {
        back.GetComponent<Image>().DOFillAmount(1f, 1f).SetEase(Ease.OutQuint);
        difficulty.GetComponent<RectTransform>().DOLocalMove(new Vector3(-300, 0, 0), 1f).SetEase(Ease.OutQuint).SetDelay(0.2f).SetRelative(true);
        info.GetComponent<RectTransform>().DOLocalMove(new Vector3(-580, 0, 0), 1f).SetEase(Ease.OutQuint).SetDelay(0.5f).SetRelative(true);
        tips.GetComponent<RectTransform>().DOLocalMove(new Vector3(0, 30, 0), 1f).SetEase(Ease.OutQuint).SetDelay(0.3f).SetRelative(true);

        var boardSeq = DOTween.Sequence();
        boardSeq.Append(subBoard.transform.DOLocalMove(new Vector3(-8f, 5f, 0f), 1f).SetEase(Ease.OutQuint));
        boardSeq.Join(subBoard.transform.DORotate(Vector3.up * 0f + Vector3.right * 90f, 1f).SetEase(Ease.OutQuint));
        boardSeq.AppendInterval(3f);
        boardSeq.Append(mask.GetComponent<Image>().DOFade(1f, 1f));
        boardSeq.OnComplete(() => SceneManager.LoadScene("GameScene"));
        boardSeq.Play();
    }
}
