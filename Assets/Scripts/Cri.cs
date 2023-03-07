using System.Collections;
using System.Collections.Generic;
using CriWare;
using UnityEngine;

public class Cri : MonoBehaviour
{
    public CriAtomSource se;
    public CriAtomSource bgm;
    
    void Start()
    {
        // acf設定
        string path = Application.streamingAssetsPath + "/Audio/MagicalTouch.acf";
        CriAtomEx.RegisterAcf(null, path);
        
        // CriAtom作成
        new GameObject().AddComponent<CriAtom>();
        
        // SE acb追加
        CriAtom.AddCueSheet("SE", "Audio/SE.acb", null, null);
        
        se = new GameObject().AddComponent<CriAtomSource>();
        se.loop = false;
        se.cueSheet = "SE";
    }
    
    public void SetBgm(string title)
    {
        // bgm acb追加
        CriAtom.AddCueSheet(title, $"Audio/{title}/{title}.acb", $"Audio/{title}/{title}.awb", null);

        bgm = new GameObject().AddComponent<CriAtomSource>();
        bgm.loop = false;
        bgm.cueSheet = title;
    }
}
