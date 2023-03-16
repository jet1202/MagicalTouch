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
    [SerializeField] private GameObject pushLine;
    
    [SerializeField] private GameObject judgePerfect;
    [SerializeField] private GameObject judgeGreat;
    [SerializeField] private GameObject judgeGood;
    [SerializeField] private GameObject judgeMiss;
    private List<KeyValuePair<GameObject, Note>> NotesData = new List<KeyValuePair<GameObject, Note>>();
    private List<KeyValuePair<GameObject, int>> LinesData = new List<KeyValuePair<GameObject, int>>();
    public int bpm;
    public float offset;
    public float timing;
    
    private float Speed;
    private const float missGap = 0.2f;
    private KeyValuePair<GameObject, Note> _notesData;
    private KeyValuePair<GameObject, int> _linesData;
    private string _judgeMassage;

    private string title = "Test";
    private string difficulty = "Expert";
    private bool isPushLine = true;
    
    // ノーツ数
    private int total;
    public int combo = 0;
    private int maxCombo = 0;
    
    // スコア
    public int score = 0;
    private float scoreN = 0;
    private float scoreC = 0;
    private int notesN10 = 0;
    private int totalN10 = 0;
    
    // 判定
    private int excellent = 0;
    private int perfect = 0;
    private int good = 0;
    private int bad = 0;
    private int miss = 0;

    IEnumerator Start()
    {
        // データをImport
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
        
        // ノーツの設定(Longノーツの中継判定地点を作る), ノーツ数計算
        int leng = notesSheetA.Count;
        Note n;
        for (int i = 0; i < leng; i++)
        {
            n = notesSheetA[i];
            switch (n.GetKind())
            {
                case 'N':
                case 'F':
                case 'L':
                    total++;
                    totalN10 += 10;
                    break;
                case 'H':
                    total++;
                    totalN10 += 6;
                    break;
            }
            
            if (n.GetKind() != 'L') continue;
            
            // 終点('T')の判定
            notesSheetA.Add(new Note(n.GetTime() + n.GetLength(), n.GetStartLane(), n.GetEndLane(), 'T', 0));
            total++;
            totalN10 += 2;

            float nextTiming = (float)(Math.Round((n.GetTime() / 100f - offset) / timing + 1.1f) * timing) + offset;
            for (float j = nextTiming; j < (n.GetTime() + n.GetLength()) / 100f - 0.1f; j += timing)
            {
                notesSheetA.Add(new Note((int)Math.Floor(j * 100), n.GetStartLane(), n.GetEndLane(), 'M', 0));
                total++;
                totalN10 += 2;
            }
        }
        
        // 整列
        var notesData = notesSheetA.OrderBy(x => x.GetTime()).ThenBy(x => x.GetStartLane());
        List<Note> notesSheet = new List<Note>();
        foreach (var data in notesData)
        {
            notesSheet.Add(data);
        }

        // ノーツの生成
        int len = notesSheet.Count;
        for (int i = 0; i < len; i++)
        {
            GameObject ins = Instantiate(NoteKind(notesSheet[i].GetKind()), this.transform);
            _notesData = new KeyValuePair<GameObject, Note>(ins, notesSheet[i]);
            NoteSettings(_notesData);
            NotesData.Add(_notesData);
        }
        
        // 同時押しラインの生成
        if (isPushLine)
        {
            LinesData = new List<KeyValuePair<GameObject, int>>();
            Note beforeData = null;
            foreach (var data in notesSheet)
            {
                if (beforeData == null)
                {
                    beforeData = data;
                    continue;
                }
                char kind = data.GetKind();
                char beforeKind = beforeData.GetKind();
                if (kind == 'M' || kind == 'T' || beforeKind == 'M' || beforeKind == 'T' ||
                    beforeData.GetTime() != data.GetTime() || beforeData.GetEndLane() >= data.GetStartLane())
                {
                    beforeData = data;
                    continue;
                }

                GameObject ins = Instantiate(pushLine, this.transform);

                float time = data.GetTime() * Speed / 100;
                var positions = new Vector3[]
                {
                    new Vector3(-6f + beforeData.GetEndLane(), 0f, time),
                    new Vector3(-6f + data.GetStartLane(), 0f, time)
                };
                ins.GetComponent<LineRenderer>().SetPositions(positions);
                LinesData.Add(new KeyValuePair<GameObject, int>(ins, data.GetTime()));

                beforeData = data;
            }
        }

        gameDirector.isOk = true;
        Debug.Log($"Load Finished. Total:{total}, TotalN:{totalN10}");
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
            char kind = NotesData[i].Value.GetKind();
            NotesData[i].Key.GetComponent<SpriteRenderer>().enabled = false;
            NotesData.RemoveAt(i);

            cri.se.Play(1);
            NoteJudge(Mathf.Abs(gap), notePos, kind);
        }
    }

    void NoteJudge(float gap, Vector3 appearPos, char kind)
    {
        int s = 0;
        switch (kind)
        {
            case 'N':
            case 'F':
            case 'L':
                s = 10;
                break;
            case 'H':
                s = 6;
                break;
            case 'M':
            case 'T':
                s = 2;
                break;
        }
        
        if (gap < 0.02f)
        {
            Instantiate(judgePerfect, appearPos, Quaternion.identity);
            _judgeMassage = gap + " Excellent";
            excellent++;
            combo++;
        }
        else if (gap < 0.05f)
        {
            Instantiate(judgePerfect, appearPos, Quaternion.identity);
            _judgeMassage = gap + " Perfect";
            perfect++;
            combo++;
        }
        else if (gap < 0.10f)
        {
            Instantiate(judgeGreat, appearPos, Quaternion.identity);
            _judgeMassage = gap + " Good";
            good++;
            s -= s * 4;
            combo++;
        }
        else
        {
            Instantiate(judgeGood, appearPos, Quaternion.identity);
            _judgeMassage = gap + " Bad";
            bad++;
            s = 0;
            combo = 0;
        }
        // Debug.Log(_judgeMassage);

        // スコア加算
        notesN10 += s;
        if (maxCombo < combo) maxCombo = combo;
    }
    

    private void Update()
    {
        if (NotesData.Count != 0)
        {

            _notesData = NotesData[0];
            while (_notesData.Value.GetTime() / 100f + missGap < gameDirector.musicTime)
            {
                Destroy(_notesData.Key);
                NotesData.RemoveAt(0);

                Instantiate(judgeMiss,
                    new Vector3(-6f + (_notesData.Value.GetStartLane() + _notesData.Value.GetEndLane()) * 0.5f, 0.5f,
                        0), Quaternion.identity);
                combo = 0;
                miss++;
                //Debug.Log("Miss");

                if (NotesData.Count == 0) break;
                _notesData = NotesData[0];
            }

            int index = 0;
            while (NotesData.Count > index && NotesData[index].Value.GetTime() / 100f < gameDirector.musicTime)
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
                        NotesData[index].Key.GetComponent<SpriteRenderer>().enabled = false;
                        char kind = NotesData[index].Value.GetKind();
                        NotesData.RemoveAt(index);

                        if (ki == 'H')
                            cri.se.Play(1);
                        NoteJudge(0f, notePos, kind);
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
                        NoteJudge(0f, notePos, 'F');
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
            }
        }

        if (LinesData.Count != 0)
        {
            _linesData = LinesData[0];
            while (_linesData.Value / 100f < gameDirector.musicTime)
            {
                _linesData.Key.GetComponent<LineRenderer>().enabled = false;
                LinesData.RemoveAt(0);

                if (LinesData.Count == 0) break;
                _linesData = LinesData[0];
            }
        }

        if (gameDirector.isPlaying)
        {
            // スコア計算
            scoreN = (float)notesN10 / totalN10;
            // Debug.Log($"{notesN10 / 10f} / {totalN10 / 10f} = {scoreN}");
            scoreC = (float)maxCombo / total;
            // Debug.Log($"{maxCombo} / {total} = {scoreC}");
            score = (int)(scoreN * 900000 + scoreC * 100000);
        }
    }
}
