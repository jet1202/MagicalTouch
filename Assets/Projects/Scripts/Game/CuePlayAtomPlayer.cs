using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CriWare;

public class CuePlayAtomPlayer : MonoBehaviour
{
    private CriAtomEx.CueInfo[] cueInfoList;
    private CriAtomExPlayer atomExPlayer;
    private CriAtomExAcb atomExAcb;
    private CriAtomExAcb seAcb;
    
    IEnumerator Start()
    {
        while (CriAtom.CueSheetsAreLoading)
        {
            yield return null;
        }

        atomExPlayer = new CriAtomExPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
