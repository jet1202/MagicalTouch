using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    [SerializeField] private TouchDirector touchDirector;
    [SerializeField] private ImportData importData;
    [SerializeField] private NotesController notesController;
    [SerializeField] private DamageController damageController;
    [SerializeField] private Cri cri;
    
    [SerializeField] private GameObject normalNote;
    [SerializeField] private GameObject holdNote;
    [SerializeField] private GameObject flickNote;
    [SerializeField] private GameObject longNote;
    [SerializeField] private GameObject maintainNote;
    [SerializeField] private GameObject pushLine;

    [SerializeField] private GameObject judgeExcellent;
    [SerializeField] private GameObject judgePerfect;
    [SerializeField] private GameObject judgeGreat;
    [SerializeField] private GameObject judgeBad;
    [SerializeField] private GameObject judgeMiss;
    [SerializeField] private GameObject effectObject;
    
    [SerializeField] private SpriteRenderer justFlame;
    
    private List<KeyValuePair<GameObject, Note>> NotesData = new List<KeyValuePair<GameObject, Note>>();
    private List<KeyValuePair<GameObject, int>> LinesData = new List<KeyValuePair<GameObject, int>>();

    public Speed[] speedData;
    public Bpm[] bpmData;

    private List<float> MaintainJudge;

    private float Speed;
    private const float missGap = 0.20f;
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
    private int isFull = 2;
    public int excellent = 0;
    public int perfect = 0;
    public int great = 0;
    public int bad = 0;
    public int miss = 0;

    private int bpmProg = 0;
    public int nowBpm = 0;

    IEnumerator Start()
    {
        // データをImport
        Speed = GetComponent<NotesController>().Speed;

        // Sheet
        IEnumerator corutine = importData.ImportSheet(title, difficulty);
        yield return StartCoroutine(corutine);
        List<Note> notesSheetA = (List<Note>)corutine.Current;
        
        // Addition
        corutine = importData.ImportAddition(title, difficulty);
        yield return StartCoroutine(corutine);
        AdditionData additionData = (AdditionData)corutine.Current;
        speedData = additionData.speedItem;
        bpmData = additionData.bpmItem;
        
        notesController.BpmDataImport(speedData);

        cri.SetBgm(title);

        // Maintainの判定をリストに格納
        MaintainJudge = new List<float>();
        int b;
        float t, nex;
        int leng = bpmData.Length;
        for (int i = 0; i < leng; i++)
        {
            b = bpmData[i].bpm;
            t = bpmData[i].time;
        
            if (i == leng - 1)
                nex = cri.GetLen() / 1000f;
            else
                nex = bpmData[i + 1].time;
        
            for (float j = t; j < nex; j += 30f / b)
            {
                MaintainJudge.Add(j);
            }
        }
        
        // ノーツの設定(Longノーツの中継判定地点を作る), ノーツ数計算
        leng = notesSheetA.Count;
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

            int fir;
            for (int j = 0;; j++)
            {
                if (MaintainJudge[j] > (n.GetTime() + 10) / 100f)
                {
                    fir = j;
                    break;
                }
            }
            
            for (int j = fir;; j++)
            {
                if (MaintainJudge[j] > (n.GetTime() + n.GetLength() - 10) / 100f)
                    break;
                
                notesSheetA.Add(new Note((int)Math.Floor(MaintainJudge[j] * 100), n.GetStartLane(), n.GetEndLane(), 'M', 0));
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

                float time = TimeTo(data.GetTime() / 100f) * Speed;
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
        
        // 画面の設定
        justFlame.color = new Color(1f, 1f, 0f, 1f);

        gameDirector.isOk = true;
        Debug.Log($"Load Finished. Total:{total}, TotalN:{totalN10}");
    }

    private float TimeTo(float time)
    {
        int len = speedData.Length;
        int pro = 0;
        for (int i = 0; i < len; i++)
        {
            if (speedData[i].time100 >= time * 100)
            {
                break;
            }

            pro = i;
        }
        
        float pos = notesController.accDis[pro];

        if (speedData[pro].isVariation)
        {
            float t = time - speedData[pro].time100 / 100f;
            pos += t * Math.Min(speedData[pro].speed100 / 100f, time);
            pos += t * (Math.Abs(speedData[pro].speed100 - speedData[pro + 1].speed100) /
                (float)(speedData[pro + 1].time100 - speedData[pro].time100) * t) / 2f;
        }
        else
        {
            float t = time - speedData[pro].time100 / 100f;
            pos += t * speedData[pro].speed100 / 100f;
        }

        return pos;
    }

    private void NoteSettings(KeyValuePair<GameObject, Note> noteData)
    {
        float posx = -6f + (noteData.Value.GetEndLane() + noteData.Value.GetStartLane()) * 0.5f;
        float sizex = noteData.Value.GetEndLane() - noteData.Value.GetStartLane();
        float time = TimeTo(noteData.Value.GetTime() / 100f) * Speed;
        
        noteData.Key.transform.localPosition = new Vector3(posx, 0f, time);
        noteData.Key.GetComponent<SpriteRenderer>().size = new Vector2(sizex, 1f);

        if (noteData.Value.GetKind() == 'L')
        {
            float length = TimeTo((noteData.Value.GetTime() + noteData.Value.GetLength()) / 100f) * Speed - time;
            noteData.Key.transform.GetChild(0).transform.localPosition = new Vector3(0f, length / 2, 0f);
            noteData.Key.transform.GetChild(0).transform.localScale = new Vector3(sizex, length, 1f);
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
            if (gap < -missGap)
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
            int wi = NotesData[i].Value.GetEndLane() - NotesData[i].Value.GetStartLane();
            NotesData[i].Key.GetComponent<SpriteRenderer>().enabled = false;
            NotesData.RemoveAt(i);

            cri.se.Play(1);
            NoteJudge(Mathf.Abs(gap), notePos, kind, wi);
        }
    }

    void NoteJudge(float gap, Vector3 appearPos, char kind, int wi)
    {
        appearPos = new Vector3(appearPos.x, 0f, 0f);
        
        GameObject ins = Instantiate(effectObject, appearPos, quaternion.identity);
        ins.transform.rotation = new Quaternion(0.7071068f, 0, 0, 0.7071068f);
        ins.GetComponent<EffectController>().width = wi;
        Color color = new Color();
        
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
            Instantiate(judgeExcellent, appearPos, Quaternion.identity);
            color = new Color(1f, 1f, 0f, 1f);
            _judgeMassage = gap + " Excellent";
            excellent++;
            combo++;
        }
        else if (gap < 0.06f)
        {
            Instantiate(judgePerfect, appearPos, Quaternion.identity);
            color = new Color(1f, 1f, 0f, 1f);
            _judgeMassage = gap + " Perfect";
            perfect++;
            combo++;
        }
        else if (gap < 0.15f)
        {
            Instantiate(judgeGreat, appearPos, Quaternion.identity);
            color = new Color(95f / 255f, 184f / 255f, 1f, 1f);
            _judgeMassage = gap + " Great";
            great++;
            s -= 4;
            combo++;
        }
        else
        {
            Instantiate(judgeBad, appearPos, Quaternion.identity);
            color = new Color(111f / 255f, 111f / 255f, 111f / 255f, 1f);
            _judgeMassage = gap + " Bad";
            bad++;
            s = 0;
            combo = 0;
        }
        // Debug.Log(_judgeMassage);
        ins.GetComponent<SpriteRenderer>().color = color;

        // スコア加算
        notesN10 += s;
        if (maxCombo < combo) maxCombo = combo;
    }
    

    private void Update()
    {
        if (NotesData.Count != 0)
        {
            // 見逃したノーツの削除
            _notesData = NotesData[0];
            while (_notesData.Value.GetTime() / 100f + missGap < gameDirector.musicTime)
            {
                _notesData.Key.GetComponent<SpriteRenderer>().enabled = false;
                NotesData.RemoveAt(0);

                Instantiate(judgeMiss,
                    new Vector3(-6f + (_notesData.Value.GetStartLane() + _notesData.Value.GetEndLane()) * 0.5f, 0.5f,
                        0), Quaternion.identity);
                combo = 0;
                miss++;
                damageController.Damage();
                // Debug.Log("Damage");
                //Debug.Log("Miss");

                if (NotesData.Count == 0) break;
                _notesData = NotesData[0];
            }

            // Hold, Flickの処理
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
                        int wi = NotesData[index].Value.GetEndLane() - NotesData[index].Value.GetStartLane();
                        NotesData.RemoveAt(index);

                        if (ki == 'H')
                            cri.se.Play(1);
                        NoteJudge(0f, notePos, kind, wi);
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
                        int wi = NotesData[index].Value.GetEndLane() - NotesData[index].Value.GetStartLane();
                        NotesData.RemoveAt(index);

                        cri.se.Play(0);
                        NoteJudge(0f, notePos, 'F', wi);
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
        
        // AP, フルコン中のJustFlameの色
        if (isFull == 2)
        {
            if (great + bad + miss > 0)
            {
                isFull = 1;
                justFlame.color = new Color(0f, 59f / 255f, 1f, 1f);
            }
        }
        else if (isFull == 1)
        {
            if (bad + miss > 0)
            {
                isFull = 0;
                justFlame.color = new Color(1f, 71f / 255f, 208f / 255f, 1f);
            }
        }
        
        // 現在BPM
        if (bpmProg < bpmData.Length && bpmData[bpmProg].time / 100f < gameDirector.musicTime)
        {
            nowBpm = bpmData[bpmProg].bpm;
            bpmProg++;
        }
    }
}
