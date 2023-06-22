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
        atomExPlayer.AttachFader();
        atomExPlayer.SetFadeInTime(1000);
        atomExPlayer.SetFadeOutTime(1000);
    }

    public IEnumerator SetMusic(string title, int s)
    {
        if (title == nowCheetName) yield break;
        
        _audioTweener.Kill();
        atomExPlayer.Stop();
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
            atomExPlayer.SetStartTime(startTime);
            atomExPlayer.Start();
        }).SetLoops(-1).SetLink(gameObject).Play();
    }

    public void StopBgm()
    {
        atomExPlayer.Stop();
        _audioTweener.Kill();
    }
}
