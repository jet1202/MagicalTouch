using System;
using System.Collections;
using System.Collections.Generic;
using CriWare;
using UnityEngine;

public class Cri : MonoBehaviour
{
    public CriAtomSource se;
    public CriAtomSource bgm;
    public CriAtomEx.CueInfo bgmInfo;
    
    void Start()
    {
        // acf設定
        string path = Application.streamingAssetsPath + "/Main/K-Rhythm.acf";
        CriAtomEx.RegisterAcf(null, path);
        
        // CriAtom作成
        new GameObject().AddComponent<CriAtom>();
        
        // SE acb追加
        CriAtom.AddCueSheet("SE", "Main/SE.acb", null, null);
        
        se = new GameObject().AddComponent<CriAtomSource>();
        se.loop = false;
        se.cueSheet = "SE";

        se.volume = ScoreData.setting.Game.SeVolume / 100f;
    }
    
    public void SetBgm(string title)
    {
        // bgm acb追加
        CriAtom.AddCueSheet(title, $"SongData/{title}/{title}.acb", $"SongData/{title}/{title}.awb", null);

        bgm = new GameObject().AddComponent<CriAtomSource>();
        bgm.loop = false;
        bgm.cueSheet = title;

        bgm.volume = ScoreData.setting.Game.MusicVolume / 100f;
        
        CriAtomExAcb _exAcb = CriAtom.GetAcb(title);
        
        if (!_exAcb.GetCueInfo(0, out bgmInfo))
        {
            throw new Exception("取得できない");
        }
    }

    public float GetLen()
    {
        return bgmInfo.length;
    }
}
