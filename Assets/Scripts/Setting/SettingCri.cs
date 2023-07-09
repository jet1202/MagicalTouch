using System.Collections;
using System.Collections.Generic;
using CriWare;
using UnityEngine;

public class SettingCri : MonoBehaviour
{
    public CriAtomSource metronome;
    
    void Start()
    {
        // acf設定
        string path = Application.streamingAssetsPath + "/Main/K-Rhythm.acf";
        CriAtomEx.RegisterAcf(null, path);
        
        // CriAtom作成
        new GameObject().AddComponent<CriAtom>();
        
        // SE acb追加
        CriAtom.AddCueSheet("Metronome", "Main/Metronome.acb", null, null);
        
        metronome = new GameObject().AddComponent<CriAtomSource>();
        metronome.loop = false;
        metronome.cueSheet = "Metronome";

        metronome.volume = 0.5f;
    }
}
