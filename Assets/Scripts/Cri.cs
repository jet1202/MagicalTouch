using System.Collections;
using System.Collections.Generic;
using CriWare;
using UnityEngine;

public class Cri : MonoBehaviour
{
    public CriAtomSource se;
    
    void Start()
    {
        // acf設定
        string path = Application.streamingAssetsPath + "/MagicalTouch.acf";
        CriAtomEx.RegisterAcf(null, path);
        
        // CriAtom作成
        new GameObject().AddComponent<CriAtom>();
        
        // SE acb追加
        CriAtom.AddCueSheet("SE", "SE.acb", null, null);
        
        se = new GameObject().AddComponent<CriAtomSource>();
        se.loop = false;
        se.cueSheet = "SE";
    }
}
