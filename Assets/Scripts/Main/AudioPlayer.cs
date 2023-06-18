using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CriWare;
using DG.Tweening;
using UnityEngine.Experimental.Rendering;

public class AudioPlayer : MonoBehaviour
{
    private CriAtomExAcb acb;
    private CriAtomExPlayer atomExPlayer;
    private string nowCheetName;
    private int startTime;

    private Sequence _audioTweener;
    
    void Start()
    {
        // acf設定
        string path = Application.streamingAssetsPath + "/Main/K-Rhythm.acf";
        CriAtomEx.RegisterAcf(null, path);
        atomExPlayer = new CriAtomExPlayer();
        atomExPlayer.SetVolume(1.0f);

        // _audioTweener = DOTween.Sequence();
        // _audioTweener.Append(DOTween.To(() => 0f, (x) => atomExPlayer.SetVolume(x), 1f, 1f));
        // _audioTweener.AppendInterval(10f);
        // _audioTweener.Append(DOTween.To(() => 1f, (x) => atomExPlayer.SetVolume(x), 0f, 1f));
        // _audioTweener.AppendCallback(() =>
        // {
        //     atomExPlayer.Stop();
        //     atomExPlayer.SetStartTime(startTime);
        //     atomExPlayer.Start();
        //     Debug.Log("実行");
        // });
        // _audioTweener.SetLoops(-1, LoopType.Restart);
    }

    public IEnumerator SetMusic(string title, int s)
    {
        var cueSheet = CriAtom.AddCueSheet(title, $"SongData/{title}/{title}.acb", $"SongData/{title}/{title}.awb", null);
        while (cueSheet.IsLoading)
        {
            yield return null;
        }

        acb = cueSheet.acb;
        startTime = s;

        atomExPlayer.SetCue(acb, 0);
        atomExPlayer.SetStartTime(startTime);
        atomExPlayer.Start();

        if (nowCheetName != "")
            CriAtom.RemoveCueSheet(nowCheetName);
        nowCheetName = title;

        _audioTweener = DOTween.Sequence().AppendInterval(10f).OnStepComplete(() => {
            atomExPlayer.Stop();
            atomExPlayer.SetStartTime(startTime);
            atomExPlayer.Start();
        }).SetLoops(-1);
    }
}
