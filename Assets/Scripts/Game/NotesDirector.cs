using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    [SerializeField] private TouchDirector touchDirector;
    [SerializeField] private ImportData importData;
    [SerializeField] private NotesController notesController;
    [SerializeField] private DamageController damageController;
    [SerializeField] private Cri cri;

    [SerializeField] private GameObject subNotes;
    [SerializeField] private GameObject subLane;
    
    [SerializeField] private GameObject normalNote;
    [SerializeField] private GameObject holdNote;
    [SerializeField] private GameObject flickNote;
    [SerializeField] private GameObject longNote;
    [SerializeField] private GameObject slideNote;
    [SerializeField] private GameObject slideMaintainNote;
    [SerializeField] private GameObject maintainNote;
    [SerializeField] private GameObject pushLine;

    [SerializeField] private GameObject judgePool;
    [SerializeField] private GameObject paddlePool;
    
    [SerializeField] private SpriteRenderer justFlame;
    [SerializeField] private List<MeshRenderer> laneArray;
    [SerializeField] private GameObject twinkleEffect;

    [SerializeField] private GameObject mask;
    
    private List<KeyValuePair<GameObject, Note>> NotesData = new List<KeyValuePair<GameObject, Note>>();
    private List<KeyValuePair<GameObject, int>> LinesData = new List<KeyValuePair<GameObject, int>>();
    private List<Trash> TrashData = new List<Trash>();

    public SpeedItem[] speedData;
    public SpeedItem[] subSpeedData;
    public BpmItem[] bpmData;

    private int[] subNumber;

    private List<float> MaintainJudge;

    private float Speed;
    private const float missGap = 0.15f;
    private KeyValuePair<GameObject, Note> _notesData;
    private KeyValuePair<GameObject, int> _linesData;
    private Trash _trashData;
    private string _judgeMassage;

    // 引き継ぎ設定
    private string title;
    private string id;
    private string difficulty;
    [SerializeField] private bool isPushLine;
    [SerializeField] public bool isAuto;
    
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
    public int[] point = new int[5];

    private int bpmProg = 0;
    public int nowBpm = 0;

    private void Awake()
    {
        TouchSimulation.Enable();
    }

    IEnumerator Start()
    {
        title = GameData.title;
        id = GameData.id;
        difficulty = GameData.difficult;
        // TODO: SettingDataに合わせた設定
        
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        
        // データをImport
        Speed = GetComponent<NotesController>().Speed;

        // Sheet
        IEnumerator corutine = importData.ImportSheet(id, difficulty);
        yield return StartCoroutine(corutine);
        List<Note> notesSheetA = (List<Note>)corutine.Current;
        
        // Slide
        corutine = importData.ImportSlide();
        yield return StartCoroutine(corutine);
        List<KeyValuePair<Note, SlideMaintain[]>> slideData = (List<KeyValuePair<Note, SlideMaintain[]>>)corutine.Current;
        
        // Addition
        corutine = importData.ImportAddition(id, difficulty);
        yield return StartCoroutine(corutine);
        NoteAddition additionData = (NoteAddition)corutine.Current;
        speedData = additionData.speedItem;
        bpmData = additionData.bpmItem;
        
        // subLane
        corutine = importData.ImportSubLane(id, difficulty);
        yield return StartCoroutine(corutine);
        if (corutine.Current == null)
        {
            subNumber = Array.Empty<int>();
            subSpeedData = new[] { new SpeedItem() };
            subSpeedData[0].time100 = 0;
            subSpeedData[0].speed100 = 100;
            subSpeedData[0].isVariation = false;
        }
        else
        {
            var data = (SubLaneSave)corutine.Current;
            subNumber = data.number;
            subSpeedData = data.speedItem;
            subLane.GetComponent<SubController>().cameraWork = data.cameraWork;
            subLane.GetComponent<SubController>().activeTime = data.activeTime100;
        }
        
        Destroy(importData);
        
        notesController.BpmDataImport(speedData);
        subNotes.GetComponent<NotesController>().BpmDataImport(subSpeedData);

        cri.SetBgm(id);

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
            
            notesSheetA.Add(new Note(n.GetNumber(), n.GetTime() + n.GetLength() - 10, n.GetStartLane(), n.GetEndLane(), 'T', 0));
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
                
                notesSheetA.Add(new Note(n.GetNumber(), (int)Math.Floor(MaintainJudge[j] * 100), n.GetStartLane(), n.GetEndLane(), 'M', 0));
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
                notesSheetA.Add(new Note(n.GetNumber(), n.GetTime(), n.GetStartLane(), n.GetEndLane(), n.GetKind(), i));
                slideMaintains.Add(i, s.Value);
                total++;
                totalN10 += 10;

                foreach (var sm in s.Value)
                {
                    if (sm.isJudge)
                    {
                        notesSheetA.Add(new Note(n.GetNumber(), n.GetTime() + sm.time100, sm.startLine, sm.endLine, 'B', 0));
                        total++;
                        totalN10 += 2;
                    }
                    
                    if (sm == s.Value.Last())
                    {
                        GameObject ins = Instantiate(NoteKind('B'), this.transform);
                        NoteSettings(new KeyValuePair<GameObject, Note>(ins, new Note(n.GetNumber(), n.GetTime() + sm.time100, sm.startLine, sm.endLine, 'B', 0)), !Array.Exists(subNumber, j => j == n.GetNumber()));
                        ins.GetComponent<SpriteRenderer>().enabled = true;
                        TrashData.Add(new Trash(ins, (n.GetTime() + sm.time100) / 100f, n.GetStartLane(), n.GetEndLane(), 'P'));
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
            bool isMain = !Array.Exists(subNumber, j => j == notesSheet[i].GetNumber());

            GameObject ins = Instantiate(NoteKind(notesSheet[i].GetKind()), isMain ? this.transform : subNotes.transform);
            _notesData = new KeyValuePair<GameObject, Note>(ins, notesSheet[i]);
            NoteSettings(_notesData, isMain);
            if (_notesData.Value.GetKind() == 'S')
            {
                SlideSettings(_notesData.Key, _notesData.Value, slideMaintains[_notesData.Value.GetLength()], isMain);
            }
            NotesData.Add(_notesData);
        }

        TrashData = new List<Trash>(TrashData.OrderBy(x => x.GetTime()));
        
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
                bool isMain = !Array.Exists(subNumber, j => j == data.GetNumber());
                bool beforeIsMain = !Array.Exists(subNumber, j => j == beforeData.GetNumber());
                if (kind == 'M' || kind == 'T' || kind == 'B' || beforeKind == 'M' || beforeKind == 'T' || beforeKind == 'B' ||
                    beforeData.GetTime() != data.GetTime() || beforeData.GetEndLane() >= data.GetStartLane() || isMain != beforeIsMain)
                {
                    beforeData = data;
                    continue;
                }

                GameObject ins;

                if (isMain)
                    ins = Instantiate(pushLine, this.transform);
                else
                    ins = Instantiate(pushLine, subNotes.transform);

                float time = TimeTo(data.GetTime() / 100f, isMain) * Speed;
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
        Debug.Log($"finish total = {total}, N10 = {totalN10}");
        mask.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() => mask.SetActive(false));
    }

    private float TimeTo(float time, bool isMain)
    {
        SpeedItem[] data = isMain ? speedData : subSpeedData;
        
        int len = data.Length;
        int pro = 0;
        for (int i = 0; i < len; i++)
        {
            if (data[i].time100 >= time * 100)
            {
                break;
            }

            pro = i;
        }

        float pos;
        if (isMain) pos = notesController.accDis[pro];
        else pos = subNotes.GetComponent<NotesController>().accDis[pro];

        if (data[pro].isVariation)
        {
            float t = time - data[pro].time100 / 100f;
            pos += t * data[pro].speed100 / 100f;
            pos += t * ((data[pro + 1].speed100 - data[pro].speed100) /
                (float)(data[pro + 1].time100 - data[pro].time100) * t) / 2f;
        }
        else
        {
            float t = time - data[pro].time100 / 100f;
            pos += t * data[pro].speed100 / 100f;
        }

        return pos;
    }

    private void NoteSettings(KeyValuePair<GameObject, Note> noteData, bool isMain)
    {
        float posx = -6f + (noteData.Value.GetEndLane() + noteData.Value.GetStartLane()) * 0.5f;
        float sizex = noteData.Value.GetEndLane() - noteData.Value.GetStartLane();
        float time = TimeTo(noteData.Value.GetTime() / 100f, isMain) * Speed;
        
        noteData.Key.transform.localPosition = new Vector3(posx, 0f, time);
        noteData.Key.GetComponent<SpriteRenderer>().size = new Vector2(sizex, 1f);
        
        float rot = subLane.GetComponent<SubController>().TimeToAngle(noteData.Value.GetTime() / 100f);
        if (!isMain)
        {
            Quaternion r = noteData.Key.transform.rotation;
            noteData.Key.transform.rotation = r * Quaternion.AngleAxis(rot, Vector3.right);

            if (noteData.Value.GetKind() == 'S' || noteData.Value.GetKind() == 'L')
            {
                Quaternion s = noteData.Key.transform.GetChild(0).rotation;
                noteData.Key.transform.GetChild(0).rotation = s * Quaternion.AngleAxis(-rot, Vector3.right);
            }
        }

        if (noteData.Value.GetKind() == 'L')
        {
            float length = TimeTo((noteData.Value.GetTime() + noteData.Value.GetLength()) / 100f, isMain) * Speed - time;
            float y = length / 2;
            float z = 0f;
            if (!isMain)
            {
                float c = length * (float)Math.Sin(rot * (Math.PI / 180));
                float s = length * (float)Math.Cos(rot * (Math.PI / 180));
                y = s / 2;
                z = -c / 2;
            }

            noteData.Key.transform.GetChild(0).localPosition = new Vector3(0f, y, z);
            noteData.Key.transform.GetChild(0).localScale = new Vector3(sizex, length, 1f);
            var n = noteData.Value;
            TrashData.Add(new Trash(noteData.Key.transform.GetChild(0).gameObject, (n.GetTime() + n.GetLength()) / 100f, n.GetStartLane(), n.GetEndLane(), n.GetKind()));
        }
    }

    private void SlideSettings(GameObject obj, Note slide, SlideMaintain[] maintains, bool isMain)
    {
        // slideのFieldの描画
        
        if (maintains == null) return;

        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();

        float lastTime = TimeTo(slide.GetTime() / 100f, isMain);
        float lastLane = (slide.GetStartLane() + slide.GetEndLane()) / 2f;
        Vector3 lastPosF = new Vector3(slide.GetStartLane() - lastLane, 0f, 0);
        Vector3 lastPosL = new Vector3(slide.GetEndLane() - lastLane, 0f, 0);
        
        verts.Add(lastPosF);
        verts.Add(lastPosL);
        
        int leng = maintains.Length;
        if (leng == 0) return;
        
        for (int i = 0; i < leng; i++)
        {
            var m = maintains[i];

            Vector3 nextPosF = new Vector3(m.startLine - lastLane, 0f, (TimeTo((m.time100 + slide.GetTime()) / 100f, isMain) - lastTime) * Speed);
            Vector3 nextPosL = new Vector3(m.endLine - lastLane, 0f, (TimeTo((m.time100 + slide.GetTime()) / 100f, isMain) - lastTime) * Speed);

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
                verts.Add(new Vector3(lastPosF.x, 0f, (TimeTo((m.time100 + slide.GetTime()) / 100f, isMain) - lastTime) * Speed));
                verts.Add(new Vector3(lastPosL.x, 0f, (TimeTo((m.time100 + slide.GetTime()) / 100f, isMain) - lastTime) * Speed));
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
        TrashData.Add(new Trash(obj.transform.GetChild(0).gameObject, (slide.GetTime() + maintains.Last().time100) / 100f, slide.GetStartLane(), slide.GetEndLane(), slide.GetKind()));
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
            if (data.GetKind() != 'N' && data.GetKind() != 'L' && data.GetKind() != 'S') continue;

            if (data.GetStartLane() * 2 <= laneNumber + 1 && laneNumber - 1 < data.GetEndLane() * 2)
            {
                if (data.GetKind() != 'F')
                    isGetNote = true;
                break;
            }
        }
        
        // タップしたノーツの判定
        if (isGetNote)
        {
            char kind = NotesData[i].Value.GetKind();
            int s = NotesData[i].Value.GetStartLane();
            int e = NotesData[i].Value.GetEndLane();
            NotesData[i].Key.GetComponent<SpriteRenderer>().enabled = false;
            NotesData.RemoveAt(i);

            cri.se.Play(0);
            NoteJudge(Mathf.Abs(gap), s, e, kind);
        }
    }

    void NoteJudge(float gap, int start, int end, char kind)
    {
        // ノーツの判定、スコア加算、Effect(判定文字表示、twinkle)表示
        Vector3 appearPos = new Vector3(-6f + (start + end) * 0.5f, 0f, 0);
        var wi = end - start;

        // Paddle
        GameObject Pins = paddlePool.GetComponent<MyObjectPool>().SetObject();
        Pins.transform.position = appearPos;
        Pins.transform.rotation = new Quaternion(0.7071068f, 0, 0, 0.7071068f);
        Pins.GetComponent<PaddleController>().width = wi;
        Color Pcolor = new Color();

        // Judge
        GameObject Jins = judgePool.GetComponent<MyObjectPool>().SetObject();
        Jins.transform.position = appearPos;
        Jins.transform.rotation = Quaternion.identity;
        char judgeKind = 'M';

        Color eColor = Color.black;
        Color tColor = Color.black;
        
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
            judgeKind = 'P';
            Pcolor = new Color(1f, 1f, 0f, 1f);
            tColor = new Color(1f, 1f, 0f, 1f);
            point[0]++;
            combo++;
        }
        else if (gap < 0.05f)
        {
            judgeKind = 'P';
            Pcolor = new Color(1f, 1f, 0f, 1f);
            tColor = new Color(1f, 1f, 0f, 1f);
            point[1]++;
            combo++;
        }
        else if (gap < 0.10f)
        {
            judgeKind = 'G';
            Pcolor = new Color(95f / 255f, 184f / 255f, 1f, 1f);
            tColor = new Color(50f / 255f, 150f / 255f, 1f, 1f);
            // eColor = new Color(0f, 70f / 255f, 1f, 70f / 255f);
            point[2]++;
            s -= 4;
            combo++;
        }
        else
        {
            judgeKind = 'B';
            Pcolor = new Color(111f / 255f, 111f / 255f, 111f / 255f, 1f);
            eColor = new Color(0f, 1f, 0f, 70f / 255f);
            point[3]++;
            s = 0;
            combo = 0;
        }
        Pins.GetComponent<SpriteRenderer>().color = Pcolor;
        Jins.GetComponent<JudgeController>().Setting(judgeKind);

        // スコア加算
        notesN10 += s;
        if (maxCombo < combo) maxCombo = combo;
        
        if (eColor != Color.black)
            LaneEffect(start, end, new Color(0f, 1f, 0f, 70f / 255f));

        if (tColor != Color.black)
        {
            GameObject twinkleIns = Instantiate(twinkleEffect);
            twinkleIns.transform.position = appearPos;
            var sys = twinkleIns.GetComponent<ParticleSystem>();
            var main = sys.main;
            main.startColor = new ParticleSystem.MinMaxGradient(tColor);
            var shape = sys.shape;
            shape.scale = new Vector3(wi, 0.5f, 0.1f);
            sys.emission.SetBurst(0, new ParticleSystem.Burst(0, wi * 10));
            sys.Play();
        }
    }

    private void LaneEffect(int start, int end, Color color)
    {
        for (int i = start; i < end; i++)
        {
            laneArray[i].material.DOKill();
            laneArray[i].material.color = color;
            laneArray[i].material.DOFade(0f, 0.5f).SetEase(Ease.InQuart);
        }
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

                GameObject jIns = judgePool.GetComponent<MyObjectPool>().SetObject();
                int s = _notesData.Value.GetStartLane();
                int e = _notesData.Value.GetEndLane();
                jIns.transform.position =
                    new Vector3(-6f + (s + e) * 0.5f, 0.5f, 0f);
                LaneEffect(s, e, new Color(1f, 0f, 0f, 70f / 255f));
                jIns.transform.rotation = Quaternion.identity;
                jIns.GetComponent<JudgeController>().Setting('M');
                combo = 0;
                point[4]++;
                damageController.Damage();
                // Debug.Log("Damage");
                //Debug.Log("Miss");

                if (NotesData.Count == 0) break;
                _notesData = NotesData[0];
            }

            // Hold, Flickの処理
            int index = 0;
            while (NotesData.Count > index && (NotesData[index].Value.GetTime() - 3) / 100f < gameDirector.musicTime)
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
                        var note = NotesData[index];
                        float t = note.Value.GetTime() / 100f;
                        NotesData.RemoveAt(index);

                        if (ki == 'H' || ki == 'B' || ki == 'M' || ki == 'T')
                        {
                            int l = TrashData.Count;
                            int i;
                            for (i = 0; i > l; i++)
                            {
                                if (t < TrashData[i].GetTime())
                                    break;
                            }

                            TrashData.Insert(i,
                                new Trash(note.Key, t, note.Value.GetStartLane(), note.Value.GetEndLane(),
                                    note.Value.GetKind()));
                        }
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
            while (NotesData.Count > index && (NotesData[index].Value.GetTime() - 3) / 100f < gameDirector.musicTime)
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
                        var note = NotesData[index];
                        float t = note.Value.GetTime() / 100f;
                        NotesData.RemoveAt(index);
                        
                        int l = TrashData.Count;
                        int i;
                        for (i = 0; i > l; i++)
                        {
                            if (t < TrashData[i].GetTime())
                                break;
                        }
                        
                        TrashData.Insert(i,
                            new Trash(note.Key, t, note.Value.GetStartLane(), note.Value.GetEndLane(),
                                note.Value.GetKind()));
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
            while (_trashData.GetTime() < gameDirector.musicTime)
            {
                var sp = _trashData.GetObj().GetComponent<SpriteRenderer>();
                var me = _trashData.GetObj().GetComponent<MeshRenderer>();
                if (sp != null)
                    sp.enabled = false;
                if (me != null)
                    me.enabled = false;

                char k = _trashData.GetKind();
                if (k == 'H' || k == 'B' || k == 'M' || k == 'T')
                {
                    if (k == 'H' || k == 'B')
                        cri.se.Play(1);
                    NoteJudge(0f, _trashData.GetStartLane(), _trashData.GetEndLane(), k);
                }
                else if (k == 'F')
                {
                    cri.se.Play(2);
                    NoteJudge(0f, _trashData.GetStartLane(), _trashData.GetEndLane(), k);

                    _trashData.GetObj().transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                }

                TrashData.RemoveAt(0);

                if (TrashData.Count == 0) break;
                _trashData = TrashData[0];
            }
        }
        
        // AutoPlay
        if (isAuto)
            AutoPlay();

        if (gameDirector.isPlaying)
        {
            // スコア計算
            scoreN = (float)notesN10 / totalN10;
            scoreC = (float)maxCombo / total;
            score = (int)(scoreN * 950000 + scoreC * 50000);
        }
        
        // AP, フルコン中のJustFlameの色
        if (isFull == 2)
        {
            if (point[2] + point[3] + point[4] > 0)
            {
                isFull = 1;
                justFlame.color = new Color(0f, 59f / 255f, 1f, 1f);
            }
        }
        else if (isFull == 1)
        {
            if (point[3] + point[4] > 0)
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

    private void AutoPlay()
    {
        if (NotesData.Count != 0)
        {
            // AutoPlay
            int index = 0;
            
            _notesData = NotesData[index];
            while (_notesData.Value.GetTime() / 100f < gameDirector.musicTime)
            {
                char kind = _notesData.Value.GetKind();
                if (kind == 'N' || kind == 'L' || kind == 'S')
                {
                    int touchLane = _notesData.Value.GetStartLane() + _notesData.Value.GetEndLane();
                    BeginTouch(touchLane, _notesData.Value.GetTime() / 100f + gameDirector.waitTime);
                }

                index++;
                if (NotesData.Count == index) break;
                _notesData = NotesData[index];
            }
        }
    }
}

public class Trash
{
    private GameObject Obj { get; set; }
    private float Time { get; set; }
    private int StartLane { get; set; }
    private int EndLane { get; set; }
    private char Kind { get; set; }

    public Trash(GameObject obj, float time, int start, int end, char kind)
    {
        Obj = obj;
        Time = time;
        StartLane = start;
        EndLane = end;
        Kind = kind;
    }

    public GameObject GetObj()
    {
        return Obj;
    }

    public float GetTime()
    {
        return Time;
    }

    public int GetStartLane()
    {
        return StartLane;
    }

    public int GetEndLane()
    {
        return EndLane;
    }

    public char GetKind()
    {
        return Kind;
    }
}
