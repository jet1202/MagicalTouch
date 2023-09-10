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
    [SerializeField] private DamageController damageController;
    [SerializeField] private Cri cri;

    [SerializeField] private GameObject fieldPrefab;
    
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
    [SerializeField] private GameObject effectPool;
    
    [SerializeField] private SpriteRenderer justFlame;
    [SerializeField] private List<MeshRenderer> laneArray;

    [SerializeField] private GameObject mask;
    
    private List<KeyValuePair<GameObject, Note>> NotesData = new List<KeyValuePair<GameObject, Note>>();
    private List<KeyValuePair<GameObject, int>> LinesData = new List<KeyValuePair<GameObject, int>>();
    private List<Trash> TrashData = new List<Trash>();

    public BpmItem[] bpmData;

    private List<float> MaintainJudge;

    private List<GameObject> fieldObjects = new List<GameObject>();

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
    
    private bool isPushLine;
    private bool isAuto;
    private bool isColor;
    private float noteThickness;
    private float tapOffset;
    
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
    public int[] resultPoint = new int[8];

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
        
        var set = ScoreData.setting.Game;
        isPushLine = set.IsPushLine;
        isAuto = set.IsAuto;
        isColor = set.IsColor;
        noteThickness = set.NoteThickness / 10f;
        tapOffset = set.TapOffset / 1000f;
        
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        
        // データをImport
        Speed = ScoreData.setting.Game.NoteSpeed;

        // Sheet
        IEnumerator corutine = importData.ImportSheet(id, difficulty);
        yield return StartCoroutine(corutine);
        List<Note> notesSheetA = (List<Note>)corutine.Current;
        
        // Slide
        corutine = importData.ImportSlide();
        yield return StartCoroutine(corutine);
        List<KeyValuePair<Note, SlideMaintain[]>> slideData = (List<KeyValuePair<Note, SlideMaintain[]>>)corutine.Current;
        
        // Bpm
        corutine = importData.ImportBpm(id, difficulty);
        yield return StartCoroutine(corutine);
        BpmSave bpmSaveData = (BpmSave)corutine.Current;
        bpmData = bpmSaveData.bpmItem;
        
        // Field
        corutine = importData.ImportField(id, difficulty);
        yield return StartCoroutine(corutine);
        FieldSave field = (FieldSave)corutine.Current;
        if (field == null) throw new Exception();
        foreach (var f in field.item)
        {
            GameObject obj = Instantiate(fieldPrefab);
            fieldObjects.Add(obj);
            
            obj.GetComponent<FieldController>().ItemImport(f.speedItem, f.angleWork, f.activeTime);
            obj.SetActive(true);
        }
        
        Destroy(importData);

        cri.SetBgm(id);

        // Maintainの判定をリストに格納
        MaintainJudge = new List<float>();
        int b;
        float t, nex;
        int leng = bpmData.Length;
        for (int i = 0; i < leng; i++)
        {
            b = bpmData[i].bpm;
            t = bpmData[i].time / 1000f;
        
            if (i == leng - 1)
                nex = cri.GetLen() / 1000f;
            else
                nex = bpmData[i + 1].time / 1000f;

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
            
            notesSheetA.Add(new Note(n.GetNumber(), n.GetTime() + n.GetLength() - 10, n.GetStartLane(), n.GetEndLane(), 'T', 0, n.GetField()));
            total++;
            totalN10 += 2;

            int fir;
            for (int j = 0;; j++)
            {
                if (MaintainJudge[j] > (n.GetTime() + 11) / 1000f)
                {
                    fir = j;
                    break;
                }
            }
            
            for (int j = fir;; j++)
            {
                if (MaintainJudge[j] > (n.GetTime() + n.GetLength() - 10) / 1000f)
                    break;
                
                notesSheetA.Add(new Note(n.GetNumber(), (int)Math.Floor(MaintainJudge[j] * 1000), n.GetStartLane(), n.GetEndLane(), 'M', 0, n.GetField()));
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
                notesSheetA.Add(new Note(n.GetNumber(), n.GetTime(), n.GetStartLane(), n.GetEndLane(), n.GetKind(), i, n.GetField()));
                slideMaintains.Add(i, s.Value);
                total++;
                totalN10 += 10;

                foreach (var sm in s.Value)
                {
                    if (sm.isJudge)
                    {
                        notesSheetA.Add(new Note(n.GetNumber(), n.GetTime() + sm.time, sm.startLane, sm.endLane, 'B', 0, n.GetField()));
                        total++;
                        totalN10 += 2;
                    }
                    
                    if (sm == s.Value.Last())
                    {
                        GameObject ins = Instantiate(NoteKind('B'), fieldObjects[n.GetField()].transform.GetChild(1));
                        NoteSettings(new KeyValuePair<GameObject, Note>(ins, new Note(n.GetNumber(), n.GetTime() + sm.time, sm.startLane, sm.endLane, 'B', 0, n.GetField())));
                        ins.GetComponent<SpriteRenderer>().enabled = true;
                        TrashData.Add(new Trash(ins, (n.GetTime() + sm.time) / 1000f, n.GetStartLane(), n.GetEndLane(), 'P'));
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
            GameObject ins = Instantiate(NoteKind(notesSheet[i].GetKind()), fieldObjects[notesSheet[i].GetField()].transform.GetChild(1));
            _notesData = new KeyValuePair<GameObject, Note>(ins, notesSheet[i]);
            NoteSettings(_notesData);
            if (_notesData.Value.GetKind() == 'S')
            {
                SlideSettings(_notesData.Key, _notesData.Value, slideMaintains[_notesData.Value.GetLength()]);
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
                int nowField = data.GetField();
                int beforeField = beforeData.GetField();
                if (kind == 'M' || kind == 'T' || kind == 'B' || beforeKind == 'M' || beforeKind == 'T' || beforeKind == 'B' ||
                    beforeData.GetTime() != data.GetTime() || beforeData.GetEndLane() >= data.GetStartLane() || nowField != beforeField)
                {
                    beforeData = data;
                    continue;
                }

                GameObject ins = Instantiate(pushLine, fieldObjects[nowField].transform.GetChild(1));

                float time = TimeTo(data.GetTime() / 1000f, nowField) * Speed;
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
        if (isColor)
            justFlame.color = new Color(1f, 1f, 0f, 1f);
        else
            justFlame.color = Color.white;
        justFlame.size = new Vector2(12.06f, noteThickness);

        gameDirector.isOk = true;
        Debug.Log($"finish total = {total}, N10 = {totalN10}");
        mask.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() => mask.SetActive(false));
    }

    private float TimeTo(float time, int fieldNumber)
    {
        FieldController fieldController = fieldObjects[fieldNumber].GetComponent<FieldController>();
        SpeedItem[] data = fieldController.speedItem;
        
        int len = data.Length;
        int pro = 0;
        for (int i = 0; i < len; i++)
        {
            if (data[i].time >= time * 1000)
            {
                break;
            }

            pro = i;
        }

        float pos = fieldController.accDis[pro];

        if (data[pro].isVariation)
        {
            float t = time - data[pro].time / 1000f;
            pos += t * data[pro].speed / 100f;
            pos += t * ((data[pro + 1].speed - data[pro].speed) /
                (float)(data[pro + 1].time - data[pro].time) * t) / 2f * 10f;
        }
        else
        {
            float t = time - data[pro].time / 1000f;
            pos += t * data[pro].speed / 100f;
        }

        return pos;
    }

    private void NoteSettings(KeyValuePair<GameObject, Note> noteData)
    {
        int field = noteData.Value.GetField();

        if (noteData.Value.GetKind() == 'B')
        {
            var n = noteData.Value;
            Debug.Log($"{n.GetNumber()}, {n.GetStartLane()}, {n.GetEndLane()}, {n.GetTime()}");
        }

        float posx = -6f + (noteData.Value.GetEndLane() + noteData.Value.GetStartLane()) * 0.5f;
        float sizex = noteData.Value.GetEndLane() - noteData.Value.GetStartLane();
        float time = TimeTo(noteData.Value.GetTime() / 1000f, field) * Speed;
        
        noteData.Key.transform.localPosition = new Vector3(posx, 0f, time);
        noteData.Key.GetComponent<SpriteRenderer>().size = new Vector2(sizex, noteThickness);
        
        float rot = fieldObjects[field].GetComponent<FieldController>().TimeToAngle(noteData.Value.GetTime() / 1000f);
            
        Quaternion r = noteData.Key.transform.rotation;
        noteData.Key.transform.rotation = r * Quaternion.AngleAxis(rot, Vector3.right);
        if (noteData.Value.GetKind() == 'S' || noteData.Value.GetKind() == 'L')
        {
            Quaternion s = noteData.Key.transform.GetChild(0).rotation;
            noteData.Key.transform.GetChild(0).rotation = s * Quaternion.AngleAxis(-rot, Vector3.right);
        }

        if (noteData.Value.GetKind() == 'L')
        {
            float length = TimeTo((noteData.Value.GetTime() + noteData.Value.GetLength()) / 1000f, field) * Speed - time;
            float y = length / 2;
            float z = 0f;
            
            float c = length * (float)Math.Sin(rot * (Math.PI / 180));
            float s = length * (float)Math.Cos(rot * (Math.PI / 180));
            y = s / 2;
            z = -c / 2;

            noteData.Key.transform.GetChild(0).localPosition = new Vector3(0f, y, z);
            noteData.Key.transform.GetChild(0).localScale = new Vector3(sizex, length, 1f);
            var n = noteData.Value;
            TrashData.Add(new Trash(noteData.Key.transform.GetChild(0).gameObject, (n.GetTime() + n.GetLength()) / 1000f, n.GetStartLane(), n.GetEndLane(), n.GetKind()));
        }
    }

    private void SlideSettings(GameObject obj, Note slide, SlideMaintain[] maintains)
    {
        // slideのFieldの描画
        if (maintains == null) return;

        int field = slide.GetField();

        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();

        float lastTime = TimeTo(slide.GetTime() / 1000f, field);
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

            Vector3 nextPosF = new Vector3(m.startLane - lastLane, 0f, (TimeTo((m.time + slide.GetTime()) / 1000f, field) - lastTime) * Speed);
            Vector3 nextPosL = new Vector3(m.endLane - lastLane, 0f, (TimeTo((m.time + slide.GetTime()) / 1000f, field) - lastTime) * Speed);

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
                verts.Add(new Vector3(lastPosF.x, 0f, (TimeTo((m.time + slide.GetTime()) / 1000f, field) - lastTime) * Speed));
                verts.Add(new Vector3(lastPosL.x, 0f, (TimeTo((m.time + slide.GetTime()) / 1000f, field) - lastTime) * Speed));
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
        TrashData.Add(new Trash(obj.transform.GetChild(0).gameObject, (slide.GetTime() + maintains.Last().time) / 1000f, slide.GetStartLane(), slide.GetEndLane(), slide.GetKind()));
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
        touchTime += tapOffset;
        
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
            gap = (float)(touchTime - gameDirector.waitTime - data.GetTime() / 1000f);
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
            NoteJudge(gap, s, e, kind);
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

        float g = Math.Abs(gap);
        if (g < 0.03f)
        {
            judgeKind = 'P';
            Pcolor = new Color(1f, 1f, 0f, 1f);
            tColor = new Color(1f, 1f, 0f, 150f / 255f);
            resultPoint[3]++;
            combo++;
        }
        else if (g < 0.05f)
        {
            judgeKind = 'P';
            Pcolor = new Color(1f, 1f, 0f, 1f);
            tColor = new Color(1f, 1f, 1f, 150f / 255f);
            if (gap > 0)
                resultPoint[4]++;
            else
                resultPoint[2]++;
            combo++;
        }
        else if (g < 0.10f)
        {
            judgeKind = 'G';
            Pcolor = new Color(95f / 255f, 184f / 255f, 1f, 1f);
            tColor = new Color(1f, 1f, 1f, 150f / 255f);
            // eColor = new Color(0f, 70f / 255f, 1f, 70f / 255f);
            if (gap > 0)
                resultPoint[5]++;
            else
                resultPoint[1]++;
            s -= 4;
            combo++;
        }
        else
        {
            judgeKind = 'B';
            Pcolor = new Color(111f / 255f, 111f / 255f, 111f / 255f, 1f);
            eColor = new Color(0f, 1f, 0f, 70f / 255f);
            if (gap > 0)
                resultPoint[6]++;
            else
                resultPoint[0]++;
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
            // effectPool
            GameObject Tins = effectPool.GetComponent<MyObjectPool>().SetObject();
            
            Transform Tr = Tins.transform;
            
            Transform Tnote = Tr.GetChild(0);
            Tnote.GetComponent<MeshRenderer>().sortingLayerName = "Important";
            Tr.position = appearPos;
            Tr.rotation = Quaternion.identity;
            Tnote.GetComponent<MeshRenderer>().material.color = tColor;
            Tnote.localScale = new Vector3(wi, 1f, 1f);
            Tnote.transform.localPosition = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(Tnote.DOMoveY(6f, 0.4f).SetEase(Ease.OutQuad));
            seq.Join(Tnote.GetComponent<MeshRenderer>().material.DOFade(0f, 0.5f).SetEase(Ease.Linear));
            seq.Play().OnComplete(() =>
            {
                effectPool.GetComponent<MyObjectPool>().RemoveObject(Tins);
            });
            
            Transform Tflick = Tr.GetChild(1);
            Tflick.GetComponent<MeshRenderer>().sortingLayerName = "Important";

            if (kind == 'F')
            {
                Tflick.gameObject.SetActive(true);
                Tflick.GetComponent<Renderer>().material.SetFloat("_Adapt", Time.time);
            }
            else
            {
                Tflick.gameObject.SetActive(false);
            }
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
            while (_notesData.Value.GetTime() / 1000f + missGap < (gameDirector.musicTime + tapOffset))
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
                resultPoint[7]++;
                damageController.Damage();
                // Debug.Log("Damage");
                //Debug.Log("Miss");

                if (NotesData.Count == 0) break;
                _notesData = NotesData[0];
            }

            // Hold, Flickの処理
            int index = 0;
            while (NotesData.Count > index && (NotesData[index].Value.GetTime() - 3) / 1000f < (gameDirector.musicTime + tapOffset))
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
                        float t = note.Value.GetTime() / 1000f;
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
            while (NotesData.Count > index && (NotesData[index].Value.GetTime() - 3) / 1000f < (gameDirector.musicTime + tapOffset))
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
                        float t = note.Value.GetTime() / 1000f;
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
            while (_linesData.Value / 1000f < gameDirector.musicTime)
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
        if (isColor)
        {
            if (isFull == 2)
            {
                if (resultPoint[0] + resultPoint[1] + resultPoint[5] + resultPoint[6] + resultPoint[7] > 0)
                {
                    isFull = 1;
                    justFlame.color = new Color(0f, 59f / 255f, 1f, 1f);
                }
            }
            else if (isFull == 1)
            {
                if (resultPoint[0] + resultPoint[6] + resultPoint[7] > 0)
                {
                    isFull = 0;
                    justFlame.color = Color.white;
                }
            }
        }

        // 現在BPM
        if (bpmProg < bpmData.Length && bpmData[bpmProg].time / 1000f < gameDirector.musicTime)
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
            while (_notesData.Value.GetTime() / 1000f < gameDirector.musicTime)
            {
                char kind = _notesData.Value.GetKind();
                if (kind == 'N' || kind == 'L' || kind == 'S')
                {
                    int touchLane = _notesData.Value.GetStartLane() + _notesData.Value.GetEndLane();
                    BeginTouch(touchLane, _notesData.Value.GetTime() / 1000f + gameDirector.waitTime - tapOffset);
                }

                index++;
                if (NotesData.Count <= index) break;
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
