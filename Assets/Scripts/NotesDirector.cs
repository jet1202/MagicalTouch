using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    [SerializeField] private TouchDirector touchDirector;
    [SerializeField] private ImportData importData;
    [SerializeField] private Cri cri;
    
    [SerializeField] private GameObject normalNote;
    [SerializeField] private GameObject holdNote;
    [SerializeField] private GameObject flickNote;
    [SerializeField] private GameObject longNote;
    [SerializeField] private GameObject maintainNote;
    
    [SerializeField] private GameObject judgePerfect;
    [SerializeField] private GameObject judgeGreat;
    [SerializeField] private GameObject judgeGood;
    [SerializeField] private GameObject judgeMiss;
    private List<KeyValuePair<GameObject, Note>> NotesData = new List<KeyValuePair<GameObject, Note>>();
    public int bpm;
    public float offset;
    public float timing;
    
    private float Speed;
    private const float missGap = 0.2f;
    private KeyValuePair<GameObject, Note> _notesData;
    private string _judgeMassage;

    private string title = "Test";
    private string difficulty = "Expert";
    private bool isPushLine = true;

    IEnumerator Start()
    {
        Speed = GetComponent<NotesController>().Speed;

        IEnumerator corutine = importData.ImportSheet(title, difficulty);
        yield return StartCoroutine(corutine);
        List<Note> notesSheetA = (List<Note>)corutine.Current;

        corutine = importData.ImportBase(title);
        yield return StartCoroutine(corutine);
        Base baseData = (Base)corutine.Current;
        bpm = baseData.bpm;
        timing = 30f / bpm;
        offset = baseData.offset;
        
        cri.SetBgm(title);
        
        // ノーツの設定(Longノーツの中継判定地点を作る)
        int leng = notesSheetA.Count;
        Note n;
        for (int i = 0; i < leng; i++)
        {
            n = notesSheetA[i];
            if (n.GetKind() != 'L') continue;
            
            // 終点('T')の判定
            notesSheetA.Add(new Note(n.GetTime() + n.GetLength(), n.GetStartLane(), n.GetEndLane(), 'T', 0));

            float nextTiming = (float)(Math.Round((n.GetTime() / 100f - offset) / timing + 1.1f) * timing) + offset;
            for (float j = nextTiming; j < (n.GetTime() + n.GetLength()) / 100f - 0.1f; j += timing)
            {
                notesSheetA.Add(new Note((int)Math.Floor(j * 100), n.GetStartLane(), n.GetEndLane(), 'M', 0));
            }
        }
        
        // 整列
        var notesData = notesSheetA.OrderBy(x => x.GetTime()).ThenBy(x => x.GetStartLane());
        List<Note> notesSheet = new List<Note>();
        foreach (var data in notesData)
        {
            notesSheet.Add(data);
        }

        int len = notesSheet.Count;
        for (int i = 0; i < len; i++)
        {
            GameObject ins = Instantiate(NoteKind(notesSheet[i].GetKind()), this.transform);
            _notesData = new KeyValuePair<GameObject, Note>(ins, notesSheet[i]);
            NoteSettings(_notesData);
            NotesData.Add(_notesData);
        }

        gameDirector.isOk = true;
        Debug.Log("Load Finished.");
    }

    private void NoteSettings(KeyValuePair<GameObject, Note> noteData)
    {
        float posx = -6f + (noteData.Value.GetEndLane() + noteData.Value.GetStartLane()) * 0.5f;
        float sizex = noteData.Value.GetEndLane() - noteData.Value.GetStartLane();
        float time = noteData.Value.GetTime() * Speed / 100;
        
        noteData.Key.transform.localPosition = new Vector3(posx, 0f, time);
        noteData.Key.GetComponent<SpriteRenderer>().size = new Vector2(sizex, 1f);

        if (noteData.Value.GetKind() == 'L')
        {
            float length = noteData.Value.GetLength() / 100f;
            noteData.Key.transform.GetChild(0).transform.localPosition = new Vector3(0f, length / 2 * Speed, 0f);
            noteData.Key.transform.GetChild(0).transform.localScale = new Vector3(sizex, length * Speed, 1f);
        }
    }

    GameObject NoteKind(char kind)
    {
        GameObject k;
        switch (kind)
        {
            case 'N':
                k = normalNote;
                break;
            case 'H':
                k = holdNote;
                break;
            case 'F':
                k = flickNote;
                break;
            case 'L':
                k = longNote;
                break;
            case 'M':
            case 'T':
                k = maintainNote;
                break;
            default:
                k = normalNote;
                break;
        }

        return k;
    }

    public void BeginTouch(int laneNumber, double touchTime)
    {
        // NormalNote, LongNoteの始点
        int con = NotesData.Count;
        if (con == 0) return;

        // どのノーツをタップしたか判定
        bool isGetNote = false;
        float gap = 1f;
        int i;
        for (i = 0; i < con; i++)
        {
            Note data = NotesData[i].Value;
            gap = (float)(touchTime - gameDirector.waitTime - data.GetTime() / 100f);
            if (Mathf.Abs(gap) > missGap)
            {
                break;
            }
            
            if (data.GetKind() != 'N' && data.GetKind() != 'L') continue;

            if (data.GetStartLane() - 1 <= laneNumber && laneNumber <= data.GetEndLane())
            {
                isGetNote = true;
                break;
            }
        }
        
        // タップしたノーツの判定
        if (isGetNote)
        {
            Vector3 notePos = new Vector3(-6f + (NotesData[i].Value.GetEndLane() + NotesData[i].Value.GetStartLane()) * 0.5f, 0.5f, 0);
            NotesData[i].Key.GetComponent<SpriteRenderer>().enabled = false;
            NotesData.RemoveAt(i);

            cri.se.Play(1);
            NoteJudge(Mathf.Abs(gap), notePos);
        }
    }

    void NoteJudge(float gap, Vector3 appearPos)
    {
        if (gap < 0.05f)
        {
            Instantiate(judgePerfect, appearPos, Quaternion.identity);
            _judgeMassage = gap + " Perfect";
        }
        else if (gap < 0.1f)
        {
            Instantiate(judgeGreat, appearPos, Quaternion.identity);
            _judgeMassage = gap + " Great";
        }
        else
        {
            Instantiate(judgeGood, appearPos, Quaternion.identity);
            _judgeMassage = gap + " Good";
        }
        //Debug.Log(_judgeMassage);
    }
    

    private void Update()
    {
        if (NotesData.Count == 0) return;

        _notesData = NotesData[0];
        while (_notesData.Value.GetTime() / 100f + missGap < gameDirector.musicTime)
        {
            Destroy(_notesData.Key);
            NotesData.RemoveAt(0);

            Instantiate(judgeMiss,
                new Vector3(-6f + (_notesData.Value.GetStartLane() + _notesData.Value.GetEndLane()) * 0.5f, 0.5f,
                    0), Quaternion.identity);
            //Debug.Log("Miss");

            if (NotesData.Count == 0) return;
            _notesData = NotesData[0];
        }

        int index = 0;
        while (NotesData[index].Value.GetTime() / 100f < gameDirector.musicTime)
        {
            char ki = NotesData[index].Value.GetKind();
            if (ki == 'H' || ki == 'M' || ki == 'T')
            {
                var n = NotesData[index].Value;
                var isTaps = touchDirector.laneTouching;

                bool tap = false;
                for (int i = Mathf.Max(n.GetStartLane() - 1, 0); i <= Mathf.Min(n.GetEndLane(), 11); i++)
                {
                    if (isTaps[i])
                    {
                        tap = true;
                        break;
                    }
                }

                if (tap)
                {
                    Vector3 notePos = new Vector3(-6f + (n.GetEndLane() + n.GetStartLane()) * 0.5f, 0.5f, 0);
                    if (ki == 'H' || ki == 'T')
                        NotesData[index].Key.GetComponent<SpriteRenderer>().enabled = false;
                    NotesData.RemoveAt(index);

                    cri.se.Play(1);
                    NoteJudge(0f, notePos);
                }
                else
                {
                    index++;
                }
            }
            else if (ki == 'F')
            {
                var n = NotesData[index].Value;
                var isFlicks = touchDirector.laneFlicking;
                
                bool flick = false;
                for (int i = Mathf.Max(n.GetStartLane() - 1, 0); i <= Mathf.Min(n.GetEndLane(), 11); i++)
                {
                    if (isFlicks[i])
                    {
                        flick = true;
                        break;
                    }
                }
                
                if (flick)
                {
                    Vector3 notePos = new Vector3(-6f + (n.GetEndLane() + n.GetStartLane()) * 0.5f, 0.5f, 0);
                    NotesData[index].Key.GetComponent<SpriteRenderer>().enabled = false;
                    NotesData.RemoveAt(index);

                    cri.se.Play(0);
                    NoteJudge(0f, notePos);
                }
                else
                {
                    index++;
                }
            }
            else
            {
                index++;
            }

            if (NotesData.Count <= index) return;
        }
    }
}
