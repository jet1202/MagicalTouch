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
    [SerializeField] private GameObject slideNote;
    [SerializeField] private GameObject slideMaintainNote;
    [SerializeField] private GameObject maintainNote;
    [SerializeField] private GameObject pushLine;

    [SerializeField] private GameObject judgeExcellent;
    [SerializeField] private GameObject judgePerfect;
    [SerializeField] private GameObject judgeGreat;
    [SerializeField] private GameObject judgeBad;
    [SerializeField] private GameObject judgeMiss;
    [SerializeField] private GameObject paddleObject;
    
    [SerializeField] private SpriteRenderer justFlame;
    
    [SerializeField] private List<MeshRenderer> laneArray;
    
    private List<KeyValuePair<GameObject, Note>> NotesData = new List<KeyValuePair<GameObject, Note>>();
    private List<KeyValuePair<GameObject, int>> LinesData = new List<KeyValuePair<GameObject, int>>();
    private List<KeyValuePair<GameObject, float>> TrashData = new List<KeyValuePair<GameObject, float>>();

    public SpeedItem[] speedData;
    public BpmItem[] bpmData;

    private List<float> MaintainJudge;

    private float Speed;
    private const float missGap = 0.15f;
    private KeyValuePair<GameObject, Note> _notesData;
    private KeyValuePair<GameObject, int> _linesData;
    private KeyValuePair<GameObject, float> _trashData;
    private string _judgeMassage;

    [SerializeField] private string title;
    [SerializeField] private string difficulty;
    [SerializeField] private bool isPushLine;
    
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
        
        // Slide
        corutine = importData.ImportSlide();
        yield return StartCoroutine(corutine);
        List<KeyValuePair<Note, SlideMaintain[]>> slideData = (List<KeyValuePair<Note, SlideMaintain[]>>)corutine.Current;
        
        // Addition
        corutine = importData.ImportAddition(title, difficulty);
        yield return StartCoroutine(corutine);
        NoteAddition additionData = (NoteAddition)corutine.Current;
        speedData = additionData.speedItem;
        bpmData = additionData.bpmItem;
        
        Destroy(importData);
        
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
            t = bpmData[i].time100 / 100f;
        
            if (i == leng - 1)
                nex = cri.GetLen() / 1000f;
            else
                nex = bpmData[i + 1].time100 / 100f;
        
            for (float j = t; j < nex; j += 60f / b)
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
            if (n.GetLength() <= 10) continue;
            
            notesSheetA.Add(new Note(n.GetTime() + n.GetLength() - 10, n.GetStartLane(), n.GetEndLane(), 'T', 0));
            total++;
            totalN10 += 2;

            int fir;
            for (int j = 0;; j++)
            {
                if (MaintainJudge[j] > (n.GetTime() + 11) / 100f)
                {
                    fir = j;
                    break;
                }
            }
            
            for (int j = fir;; j++)
            {
                if (MaintainJudge[j] > (n.GetTime() + n.GetLength() - 1) / 100f)
                    break;
                
                notesSheetA.Add(new Note((int)Math.Floor(MaintainJudge[j] * 100), n.GetStartLane(), n.GetEndLane(), 'M', 0));
                total++;
                totalN10 += 2;
            }
        }
        
        // slideの設定、ノーツ数計算
        Dictionary<int, SlideMaintain[]> slideMaintains = new Dictionary<int, SlideMaintain[]>();
        if (slideData != null)
        {
            int i = 0;
            foreach (var s in slideData)
            {
                n = s.Key;
                notesSheetA.Add(new Note(n.GetTime(), n.GetStartLane(), n.GetEndLane(), n.GetKind(), i));
                slideMaintains.Add(i, s.Value);
                total++;
                totalN10 += 10;

                foreach (var sm in s.Value)
                {
                    if (sm.isJudge)
                    {
                        notesSheetA.Add(new Note(n.GetTime() + sm.time100, sm.startLine, sm.endLine, 'B', 0));
                        total++;
                        totalN10 += 2;
                    }
                    else
                    {
                        // GameObject ins = Instantiate(NoteKind('B'), this.transform);
                        // NoteSettings(new KeyValuePair<GameObject, Note>(ins, new Note(n.GetTime() + sm.time100, sm.startLine, sm.endLine, 'B', 0)));
                        // TrashData.Add(new KeyValuePair<GameObject, float>(ins, (n.GetTime() + sm.time100) / 100f));
                    }
                }

                i++;
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
            if (_notesData.Value.GetKind() == 'S')
            {
                SlideSettings(_notesData.Key, _notesData.Value, slideMaintains[_notesData.Value.GetLength()]);
            }
            NotesData.Add(_notesData);
        }

        TrashData = new List<KeyValuePair<GameObject, float>>(TrashData.OrderBy(x => x.Value));
        
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
                if (kind == 'M' || kind == 'T' || kind == 'B' || beforeKind == 'M' || beforeKind == 'T' || beforeKind == 'B' ||
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
            pos += t * speedData[pro].speed100 / 100f;
            pos += t * ((speedData[pro + 1].speed100 - speedData[pro].speed100) /
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
            noteData.Key.transform.GetChild(0).localPosition = new Vector3(0f, length / 2, 0f);
            noteData.Key.transform.GetChild(0).localScale = new Vector3(sizex, length, 1f);
            TrashData.Add(new KeyValuePair<GameObject, float>(noteData.Key.transform.GetChild(0).gameObject, (noteData.Value.GetTime() + noteData.Value.GetLength()) / 100f));
        }
    }

    private void SlideSettings(GameObject obj, Note slide, SlideMaintain[] maintains)
    {
        // slideのFieldの描画
        
        if (maintains == null) return;

        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();

        float lastTime = TimeTo(slide.GetTime() / 100f);
        float lastLane = (slide.GetStartLane() + slide.GetEndLane()) / 2f;
        Vector3 lastPosF = new Vector3(slide.GetStartLane() - lastLane, 0f, 0);
        Vector3 lastPosL = new Vector3(slide.GetEndLane() - lastLane, 0f, 0);
        
        verts.Add(lastPosF);
        verts.Add(lastPosL);
        
        int leng = maintains.Length;
        for (int i = 0; i < leng; i++)
        {
            var m = maintains[i];

            Vector3 nextPosF = new Vector3(m.startLine - lastLane, 0f, (TimeTo((m.time100 + slide.GetTime()) / 100f) - lastTime) * Speed);
            Vector3 nextPosL = new Vector3(m.endLine - lastLane, 0f, (TimeTo((m.time100 + slide.GetTime()) / 100f) - lastTime) * Speed);

            int l = verts.Count;
            List<int> parallelogram = new List<int>();
            parallelogram.Add(l - 2);
            parallelogram.Add(l);
            parallelogram.Add(l - 1);
            parallelogram.Add(l - 1);
            parallelogram.Add(l);
            parallelogram.Add(l + 1);
            if (m.isVariation)
            {
                verts.Add(nextPosF);
                verts.Add(nextPosL);
            }
            else
            {
                verts.Add(new Vector3(lastPosF.x, 0f, (TimeTo((m.time100 + slide.GetTime()) / 100f) - lastTime) * Speed));
                verts.Add(new Vector3(lastPosL.x, 0f, (TimeTo((m.time100 + slide.GetTime()) / 100f) - lastTime) * Speed));
                if (i != leng - 1)
                {
                    verts.Add(nextPosF);
                    verts.Add(nextPosL);
                }
            }

            if (lastPosF.z - nextPosF.z > 0)
                parallelogram.Reverse();
            triangles.AddRange(parallelogram);

            lastPosF = nextPosF;
            lastPosL = nextPosL;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        obj.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = mesh;
        TrashData.Add(new KeyValuePair<GameObject, float>(obj.transform.GetChild(0).gameObject, (slide.GetTime() + maintains.Last().time100) / 100f));
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
            case 'S':
                k = slideNote;
                break;
            case 'B':
                k = slideMaintainNote;
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

            if (data.GetKind() == 'F') break;
            if (data.GetKind() != 'N' && data.GetKind() != 'L' && data.GetKind() != 'S') continue;

            if (data.GetStartLane() * 2 <= laneNumber + 1 && laneNumber - 1 < data.GetEndLane() * 2)
            {
                isGetNote = true;
                break;
            }
        }
        
        // タップしたノーツの判定
        if (isGetNote)
        {
            char kind = NotesData[i].Value.GetKind();
            int wi = NotesData[i].Value.GetEndLane() - NotesData[i].Value.GetStartLane();
            NotesData[i].Key.GetComponent<SpriteRenderer>().enabled = false;
            NotesData.RemoveAt(i);

            cri.se.Play(1);
            NoteJudge(Mathf.Abs(gap), NotesData[i].Value.GetEndLane(), NotesData[i].Value.GetStartLane(), kind, wi);
        }
    }

    void NoteJudge(float gap, int start, int end, char kind, int wi)
    {
        // ノーツの判定、スコア加算、Effect(判定文字表示、エフェクト)表示
        Vector3 appearPos = new Vector3(-6f + (start + end) * 0.5f, 0f, 0);;
        
        GameObject ins = Instantiate(paddleObject, appearPos, quaternion.identity);
        ins.transform.rotation = new Quaternion(0.7071068f, 0, 0, 0.7071068f);
        ins.GetComponent<PaddleController>().width = wi;
        Color color = new Color();
        
        int s = 0;
        switch (kind)
        {
            case 'N':
            case 'F':
            case 'L':
            case 'S':
                s = 10;
                break;
            case 'H':
                s = 6;
                break;
            case 'M':
            case 'T':
            case 'B':
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
        else if (gap < 0.05f)
        {
            Instantiate(judgePerfect, appearPos, Quaternion.identity);
            color = new Color(1f, 1f, 0f, 1f);
            _judgeMassage = gap + " Perfect";
            perfect++;
            combo++;
        }
        else if (gap < 0.10f)
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
                if (_notesData.Value.GetKind() == 'F')
                    _notesData.Key.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
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
                if (ki == 'H' || ki == 'M' || ki == 'T' || ki == 'B')
                {
                    var n = NotesData[index].Value;
                    var isTaps = touchDirector.laneTouching;

                    bool tap = false;
                    for (int i = Mathf.Max(n.GetStartLane() * 2 - 1, 0); i <= Mathf.Min(n.GetEndLane() * 2, 23); i++)
                    {
                        if (isTaps[i])
                        {
                            tap = true;
                            break;
                        }
                    }

                    if (tap)
                    {
                        NotesData[index].Key.GetComponent<SpriteRenderer>().enabled = false;
                        char kind = NotesData[index].Value.GetKind();
                        int wi = NotesData[index].Value.GetEndLane() - NotesData[index].Value.GetStartLane();
                        NotesData.RemoveAt(index);

                        if (ki == 'H' || ki == 'B')
                            cri.se.Play(1);
                        NoteJudge(0f, n.GetStartLane(), n.GetEndLane(), kind, wi);
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

            index = 0;
            while (NotesData.Count > index && (NotesData[index].Value.GetTime() - 6) / 100f < gameDirector.musicTime)
            {
                char ki = NotesData[index].Value.GetKind();
                if (ki == 'F')
                {
                    var n = NotesData[index].Value;
                    var isFlicks = touchDirector.laneFlicking;

                    bool flick = false;
                    for (int i = Mathf.Max(n.GetStartLane() * 2 - 1, 0); i <= Mathf.Min(n.GetEndLane() * 2, 23); i++)
                    {
                        if (isFlicks[i])
                        {
                            flick = true;
                            break;
                        }
                    }

                    if (flick)
                    {
                        NotesData[index].Key.GetComponent<SpriteRenderer>().enabled = false;
                        NotesData[index].Key.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                        int wi = NotesData[index].Value.GetEndLane() - NotesData[index].Value.GetStartLane();
                        NotesData.RemoveAt(index);

                        cri.se.Play(0);
                        NoteJudge(0f, n.GetStartLane(), n.GetEndLane(), 'F', wi);
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

        if (TrashData.Count != 0)
        {
            _trashData = TrashData[0];
            while (_trashData.Value + missGap < gameDirector.musicTime)
            {
                var sp = _trashData.Key.GetComponent<SpriteRenderer>();
                var me = _trashData.Key.GetComponent<MeshRenderer>();
                if (sp != null)
                    sp.enabled = false;
                if (me != null)
                    me.enabled = false;
                TrashData.RemoveAt(0);

                if (TrashData.Count == 0) break;
                _trashData = TrashData[0];
            }
        }

        if (gameDirector.isPlaying)
        {
            // スコア計算
            scoreN = (float)notesN10 / totalN10;
            scoreC = (float)maxCombo / total;
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
        if (bpmProg < bpmData.Length && bpmData[bpmProg].time100 / 100f < gameDirector.musicTime)
        {
            nowBpm = bpmData[bpmProg].bpm;
            bpmProg++;
        }
    }
}
