using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubDirector : MonoBehaviour
{
    [SerializeField] private GameObject subBoard;
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

        back = transform.GetChild(0);
        difficulty = transform.GetChild(1);
        info = transform.GetChild(2);
        tips = transform.GetChild(3);

        back.GetChild(0).GetComponent<TextMeshProUGUI>().text = song.title;
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().text = difficult.ToString();
        difficulty.GetChild(0).GetComponent<TextMeshProUGUI>().color = scrollController.GetColor(difficult);
        difficulty.GetChild(1).GetComponent<TextMeshProUGUI>().text = (song.constant[(int)difficult] / 10).ToString();

        var first = new List<string>();
        var second = new List<string>();
        string[] separate = ($"{songDetail.data},{songChart.data}").Split(',');
        for (int i = 0; i < separate.Length; i++)
        {
            if (i % 2 == 0) first.Add(separate[i]);
            else second.Add(separate[i]);
        }

        info.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            String.Join('\n', first);
        info.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            String.Join("\n: ", second);
    }
}
