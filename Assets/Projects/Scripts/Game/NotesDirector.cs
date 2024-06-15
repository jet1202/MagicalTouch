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
    [SerializeField] private GameObject slideFieldPrefab;

    [SerializeField] private GameObject judgePool;
    [SerializeField] private GameObject paddlePool;
    [SerializeField] private GameObject effectPool;
    
    [SerializeField] private LineRenderer justFlame;
    [SerializeField] private GameObject laneMesh;

    [SerializeField] private GameObject mask;
    
    private List<KeyValuePair<GameObject, Note>> NotesData = new List<KeyValuePair<GameObject, Note>>();
    private List<Trash> TrashData = new List<Trash>();

    public BpmItem[] bpmData;

    private List<int> MaintainJudge;

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
    private SelectData.DifficultyMode difficulty;

    private float noteThickness;
    public bool isAuto;
    private bool isColor;
    private float tapOffset;

    private float lfsize = 1f;
    private float lfpos = 4.5f;

    // ノーツ数
    public int total;
    public int combo = 0;
    public int maxCombo = 0;
    
    // スコア
    public int score = 0;
    private int notesN10 = 0;
    private int totalN10 = 0;
    
    // 判定
    private int isFull = 2;
    public int[] resultJudge = new int[5];
    public int[] tapJudge = new int[31];
    public int[] pm = new int[6];
    public int gapSum = 0;

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
        
        var set = SaveData.setting.Game;
        noteThickness = set.NoteThickness / 10f;
        isAuto = set.IsAuto;
        isColor = set.IsColor;
        tapOffset = set.TapOffset / 1000f;
        
        mask.SetActive(true);
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        
        // データをImport
        Speed = SaveData.setting.Game.NoteSpeed;

        // Sheet
        IEnumerator corutine = importData.ImportSheet(id, difficulty.ToString());
        yield return StartCoroutine(corutine);
        List<Note> notesSheetA = (List<Note>)corutine.Current;
        
        // Slide
        corutine = importData.ImportSlide();
        yield return StartCoroutine(corutine);
        Dictionary<int, SlideSave> slideData = (Dictionary<int, SlideSave>)corutine.Current;
        
        // Bpm
        corutine = importData.ImportBpm(id, difficulty.ToString());
        yield return StartCoroutine(corutine);
        BpmSave bpmSaveData = (BpmSave)corutine.Current;
        bpmData = bpmSaveData.bpmItem;
        
        // Field
        corutine = importData.ImportField(id, difficulty.ToString());
        yield return StartCoroutine(corutine);
        FieldSave field = (FieldSave)corutine.Current;
        if (field == null) throw new Exception();
        
        List<KeyValuePair<Note, bool>> JudgeNotes = new List<KeyValuePair<Note, bool>>(); // note, isAppear
        List<Note> TrashNotes = new List<Note>();

        bool[] fieldIsDummy = new bool[field.item.Length];
        int lo = 0;
        foreach (var f in field.item)
        {
            GameObject obj = Instantiate(fieldPrefab);
            fieldObjects.Add(obj);
            
            if (f.transparencyItem == null)
                f.transparencyItem = new TransparencyItem[]{new TransparencyItem(){time = 0, alpha = 100, isVariation = false}};
            
            obj.GetComponent<FieldController>().ItemImport(f.speedItem, f.angleWork, f.transparencyItem);
            obj.SetActive(true);

            fieldIsDummy[lo] = f.isDummy;
            lo++;
        }
        
        Destroy(importData);

        cri.SetBgm(id);

        // LongMaintainの判定をリストに格納
        MaintainJudge = new List<int>();
        float t, nex, b;
        int leng = bpmData.Length;
        for (int i = 0; i < leng; i++)
        {
            b = bpmData[i].bpm / 1000f;
            t = bpmData[i].time / 1000f;
        
            if (i == leng - 1)
                nex = cri.GetLen() / 1000f;
            else
                nex = bpmData[i + 1].time / 1000f;

            for (float j = t; j < nex; j += 30f / b)
            {
                MaintainJudge.Add((int)Math.Round(j * 1000));
            }
        }
        
        // ノーツたちをJudgeNotesとTrashNotesに分類
        int lengt = notesSheetA.Count;
        for (int i = 0; i < lengt; i++)
        {
            Note n = notesSheetA[i];
            if (n.GetKind() == 'L')
            {
                if (fieldIsDummy[n.GetField()])
                    TrashNotes.Add(n);
                else
                {
                    JudgeNotes.Add(new KeyValuePair<Note, bool>(n, true));
                    
                    // LongMaintainの判定地点を作る
                    // 終点('T')の判定
                    if (n.GetLength() <= 100) continue;

                    JudgeNotes.Add(new KeyValuePair<Note, bool>(
                        new Note(n.GetNumber(), n.GetTime() + n.GetLength() - 100, n.GetStartLane(), n.GetEndLane(), 'T',
                            0, n.GetField()), false));

                    int fir;
                    for (int j = 0;; j++)
                    {
                        if (MaintainJudge[j] > n.GetTime())
                        {
                            fir = j;
                            break;
                        }
                    }

                    for (int j = fir;; j++)
                    {
                        if (MaintainJudge[j] >= n.GetTime() + n.GetLength() - 100)
                            break;

                        JudgeNotes.Add(new KeyValuePair<Note, bool>(new Note(n.GetNumber(), MaintainJudge[j],
                            n.GetStartLane(), n.GetEndLane(), 'M', 0, n.GetField()), false));
                    }
                }
            }
            else if (n.GetKind() == 'S')
            {
                var m = slideData[n.GetNumber()];

                TrashNotes.Add(new Note(n.GetNumber(), n.GetTime(), n.GetStartLane(), n.GetEndLane(), 'A',
                    m.isDummy && !fieldIsDummy[n.GetField()] ? 0 : 1, n.GetField()));

                if (fieldIsDummy[n.GetField()])
                {
                    if (!m.isDummy)
                        TrashNotes.Add(n);
                    continue;
                }
                else
                {
                    if (m.isDummy)
                        continue;
                    
                    JudgeNotes.Add(new KeyValuePair<Note, bool>(n, true));
                }
                
                
                foreach (var sm in m.item)
                {
                    if (sm.isJudge)
                    {
                        JudgeNotes.Add(new KeyValuePair<Note, bool>(
                            new Note(n.GetNumber(), n.GetTime() + sm.time, sm.startLane, sm.endLane, 'B', 0,
                                n.GetField()), false));
                    }
                    
                    if (sm == m.item.Last())
                    {
                        TrashNotes.Add(new Note(n.GetNumber(), n.GetTime() + sm.time, sm.startLane, sm.endLane, 'P', 0,
                            n.GetField()));
                    }
                }
            }
            else
            {
                if (fieldIsDummy[n.GetField()])
                    TrashNotes.Add(n);
                else
                    JudgeNotes.Add(new KeyValuePair<Note, bool>(n, true));
            }
        }
        
        // スコア計算
        foreach (var np in JudgeNotes)
        {
            Note n = np.Key;

            int score10 = 0;
            switch (n.GetKind())
            {
                case 'N':
                case 'F':
                case 'L':
                case 'S':
                    score10 = 10;
                    break;
                case 'H':
                    score10 = 4;
                    break;
                case 'M':
                case 'T':
                case 'B':
                    score10 = 2;
                    break;
            }

            total++;
            totalN10 += score10;
        }

        // 整列
        JudgeNotes = new List<KeyValuePair<Note, bool>>(JudgeNotes.OrderBy(x => x.Key.GetTime()));
        TrashNotes = new List<Note>(TrashNotes.OrderBy(x => x.GetTime()));

        // ノーツの生成 (判定のあるもの)
        int len = JudgeNotes.Count;
        for (int i = 0; i < len; i++)
        {
            Note n = JudgeNotes[i].Key;
            GameObject ins = Instantiate(NoteKind(n.GetKind()), fieldObjects[n.GetField()].transform.GetChild(0));
            _notesData = new KeyValuePair<GameObject, Note>(ins, n);
            NoteSettings(_notesData, JudgeNotes[i].Value);
            NotesData.Add(_notesData);
        }
        
        // ノーツの生成 (判定のないもの)
        len = TrashNotes.Count;
        for (int i = 0; i < len; i++)
        {
            Note n = TrashNotes[i];
            GameObject ins = Instantiate(NoteKind(n.GetKind()), fieldObjects[n.GetField()].transform.GetChild(0));
            _notesData = new KeyValuePair<GameObject, Note>(ins, n);
            NoteSettings(_notesData, true);
            if (_notesData.Value.GetKind() == 'A')
            {
                SlideSettings(_notesData.Key, _notesData.Value, slideData[_notesData.Value.GetNumber()]);
            }
            else
            {
                TrashData.Add(new Trash(_notesData.Key, _notesData.Value.GetTime() / 1000f,
                    _notesData.Value.GetStartLane(), _notesData.Value.GetEndLane(),
                    Char.ToLower(_notesData.Value.GetKind())));
            }
        }

        TrashData = new List<Trash>(TrashData.OrderBy(x => x.GetTime()));
        
        // 画面の設定
        if (isColor)
        {
            justFlame.startColor = new Color(1f, 1f, 0f, 1f);
            justFlame.endColor = new Color(1f, 1f, 0f, 1f);
        }
        else
        {
            justFlame.startColor = Color.white;
            justFlame.endColor = Color.white;
        }

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

    private void NoteSettings(KeyValuePair<GameObject, Note> noteData, bool isAppear)
    {
        int field = noteData.Value.GetField();

        if (noteData.Value.GetKind() == 'B')
        {
            var n = noteData.Value;
        }

        float posx = -6f + (noteData.Value.GetEndLane() + noteData.Value.GetStartLane()) * 0.25f;
        float sizex = (noteData.Value.GetEndLane() - noteData.Value.GetStartLane()) / 2f;
        float time = TimeTo(noteData.Value.GetTime() / 1000f, field) * Speed;
        
        noteData.Key.transform.localPosition = new Vector3(posx, 0f, time);
        if (noteData.Value.GetKind() != 'A')
            noteData.Key.GetComponent<SpriteRenderer>().size = new Vector2(sizex - 0.15f, noteThickness);
        
        // float rot = fieldObjects[field].GetComponent<FieldController>().TimeToAngle(noteData.Value.GetTime() / 1000f);
        //     
        // Quaternion r = noteData.Key.transform.rotation;
        // noteData.Key.transform.rotation = r * Quaternion.AngleAxis(rot, Vector3.right);
        // if (noteData.Value.GetKind() == 'S' || noteData.Value.GetKind() == 'L')
        // {
        //     Quaternion s = noteData.Key.transform.GetChild(0).rotation;
        //     noteData.Key.transform.GetChild(0).rotation = s * Quaternion.AngleAxis(-rot, Vector3.right);
        // }

        if (noteData.Value.GetKind() == 'L')
        {
            float length = TimeTo((noteData.Value.GetTime() + noteData.Value.GetLength()) / 1000f, field) * Speed - time;
            float y = length / 2;
            float z = 0f;
            
            // float c = length * (float)Math.Sin(rot * (Math.PI / 180));
            // float s = length * (float)Math.Cos(rot * (Math.PI / 180));
            // y = s / 2;
            // z = -c / 2;

            noteData.Key.transform.GetChild(0).localPosition = new Vector3(0f, y, z);
            noteData.Key.transform.GetChild(0).localScale = new Vector3(sizex, length, 1f);
            var n = noteData.Value;
            TrashData.Add(new Trash(noteData.Key.transform.GetChild(0).gameObject, (n.GetTime() + n.GetLength()) / 1000f, n.GetStartLane(), n.GetEndLane(), n.GetKind()));
        }
        
        noteData.Key.SetActive(isAppear);
    }

    private void SlideSettings(GameObject obj, Note slide, SlideSave slideSave)
    {
        SlideMaintain[] maintains = slideSave.item;
        int sColor = slideSave.color;
        
        // slideのFieldの描画
        if (maintains == null) return;

        int field = slide.GetField();

        List<Vector3> verts = new List<Vector3>();
        List<int> triangles = new List<int>();

        float lastTime = TimeTo(slide.GetTime() / 1000f, field);
        float lastLane = (slide.GetStartLane() + slide.GetEndLane()) / 2f;
        Vector3 lastPosF = new Vector3((slide.GetStartLane() - lastLane) / 2f, 0f, 0);
        Vector3 lastPosL = new Vector3((slide.GetEndLane() - lastLane) / 2f, 0f, 0);
        
        verts.Add(lastPosF);
        verts.Add(lastPosL);
        
        int leng = maintains.Length;
        if (leng == 0) return;
        
        for (int i = 0; i < leng; i++)
        {
            var m = maintains[i];

            Vector3 nextPosF = new Vector3((m.startLane - lastLane) / 2f, 0f, (TimeTo((m.time + slide.GetTime()) / 1000f, field) - lastTime) * Speed);
            Vector3 nextPosL = new Vector3((m.endLane - lastLane) / 2f, 0f, (TimeTo((m.time + slide.GetTime()) / 1000f, field) - lastTime) * Speed);

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

        obj.transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        TrashData.Add(new Trash(obj, (slide.GetTime() + maintains.Last().time) / 1000f, slide.GetStartLane(),
            slide.GetEndLane(), slide.GetKind()));

        float a = slide.GetLength() == 0 ? 0.3f : 0.6f;
        obj.transform.GetComponent<MeshRenderer>().material.color = SlideColor(sColor, a);
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
            case 'P':
                k = slideMaintainNote;
                break;
            case 'A':
                k = slideFieldPrefab;
                break;
            default:
                k = normalNote;
                break;
        }

        return k;
    }
    
    private Color SlideColor(int n, float a)
    {
        Color color = Color.white;
        switch (n)
        {
            case 0:
                color = new Color(153 / 255f, 204 / 255f, 255 / 255f, a);
                break;
            case 1:
                color = new Color(255 / 255f, 204 / 255f, 153 / 255f, a);
                break;
            case 2:
                color = new Color(153 / 255f, 255f / 255f, 153f / 255f, a);
                break;
            case 3:
                color = new Color(255 / 255f, 255f / 255f, 153f / 255f, a);
                break;
            case 4:
                color = new Color(255 / 255f, 153f / 255f, 153f / 255f, a);
                break;
            case 5:
                color = new Color(204f / 255f, 153f / 255f, 255f / 255f, a);
                break;
            case 6:
                color = new Color(255f / 255f, 255f / 255f, 255f / 255f, a);
                break;
        }

        return color;
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

            if (data.GetStartLane() <= laneNumber + 1 && laneNumber <= data.GetEndLane())
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
        Vector3 appearPos = new Vector3(-6f + (start + end) * 0.25f, 0f, 0);
        var wi = end - start;

        // Paddle
        GameObject Pins = paddlePool.GetComponent<MyObjectPool>().SetObject();
        Pins.transform.position = appearPos;
        Pins.transform.rotation = new Quaternion(0.7071068f, 0, 0, 0.7071068f);
        Pins.GetComponent<PaddleController>().width = wi / 2f;
        Color Pcolor = new Color();

        // Judge
        GameObject Jins = judgePool.GetComponent<MyObjectPool>().SetObject();
        Jins.transform.position = appearPos;
        Jins.transform.rotation = Quaternion.identity;
        char judgeKind = 'M';
        
        Color tColor = Color.black;
        
        int s = 0;
        int kindType = -1;
        switch (kind)
        {
            case 'N':
            case 'L':
            case 'S':
                s = 10;
                kindType = 0;
                break;
            case 'H':
                s = 4;
                kindType = 1;
                break;
            case 'M':
            case 'T':
            case 'B':
                s = 2;
                kindType = 2;
                break;
            case 'F':
                s = 10;
                kindType = 3;
                break;
        }

        float g = Math.Abs(gap);
        if (g < 0.03f)
        {
            judgeKind = 'P';
            Pcolor = new Color(1f, 1f, 0f, 1f);
            tColor = new Color(1f, 1f, 0f, 150f / 255f);
            resultJudge[0]++;
            combo++;
        }
        else if (g < 0.05f)
        {
            judgeKind = 'P';
            Pcolor = new Color(1f, 1f, 0f, 1f);
            tColor = new Color(1f, 1f, 1f, 150f / 255f);
            resultJudge[1]++;
            combo++;
        }
        else if (g < 0.10f)
        {
            judgeKind = 'G';
            Pcolor = new Color(95f / 255f, 184f / 255f, 1f, 1f);
            tColor = new Color(1f, 1f, 1f, 150f / 255f);
            // eColor = new Color(0f, 70f / 255f, 1f, 70f / 255f);
            resultJudge[2]++;
            s -= 4;
            combo++;
        }
        else
        {
            judgeKind = 'B';
            Pcolor = new Color(111f / 255f, 111f / 255f, 111f / 255f, 1f);
            tColor = Color.clear;
            resultJudge[3]++;
            s = 0;
            combo = 0;
        }
        Pins.GetComponent<SpriteRenderer>().color = Pcolor;
        Jins.GetComponent<JudgeController>().Setting(judgeKind);

        // スコア加算
        notesN10 += s;
        if (maxCombo < combo) maxCombo = combo;

        if (kindType == 0)
        {
            int gGroup = Math.Clamp((int)((gap + 0.15f) * 1000) / 10, 0, 29);
            tapJudge[gGroup]++;
            gapSum += (int)Math.Round(gap * 1000);
        }
        else
        {
            pm[kindType * 2 - 2]++;
        }

        if (tColor != Color.black)
        {
            // effectPool
            GameObject Tins = effectPool.GetComponent<MyObjectPool>().SetObject();
            
            Transform Tr = Tins.transform;
            Tr.position = appearPos;
            Tr.rotation = Quaternion.identity;
            
            // effect1 灰色のノーツの形をしたエフェクト
            Transform Tnote = Tr.GetChild(0);
            Tnote.GetComponent<MeshRenderer>().sortingLayerName = "Important";
            
            if (kindType == 0)
            {
                Tnote.gameObject.SetActive(true);
                Tnote.GetComponent<MeshRenderer>().material.color = tColor;
                Tnote.localScale = new Vector3(wi / 2f, 1f, 1f);
                Tnote.transform.localPosition = Vector3.zero;

                Sequence seq = DOTween.Sequence();
                seq.Append(Tnote.DOMoveY(6f, 0.4f).SetEase(Ease.OutQuad));
                seq.Join(Tnote.GetComponent<MeshRenderer>().material.DOFade(0f, 0.5f).SetEase(Ease.Linear));
                seq.Play().OnComplete(() => { effectPool.GetComponent<MyObjectPool>().RemoveObject(Tins); });
            }
            else
            {
                Tnote.gameObject.SetActive(false);
                Sequence seq = DOTween.Sequence();
                seq.AppendInterval(0.5f);
                seq.Play().OnComplete(() => { effectPool.GetComponent<MyObjectPool>().RemoveObject(Tins); });
            }

            // effect2 フリックエフェクト
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
            
            // effect3 全体タッチエフェクト
            Transform Tfade = Tr.GetChild(2);
            Tfade.GetComponent<MeshRenderer>().sortingLayerName = "Important";
            
            Tfade.gameObject.SetActive(true);
            Tfade.GetComponent<Renderer>().material.SetFloat("_Adapt", Time.time);
            Tfade.transform.localScale = new Vector3(wi / 2f, 6f, 1f);
            
            // effect4 LateFast
            Transform Tlf = Tr.GetChild(3);
            if (g > 0.05f)
            {
                Tlf.GetComponent<MeshRenderer>().sortingLayerName = "Important";
                Tlf.localPosition = new Vector3(0f, lfpos, 0f);
                Tlf.localScale = new Vector3(lfsize, lfsize / 2f, lfsize);

                Tlf.gameObject.SetActive(true);
                Tlf.GetComponent<Renderer>().material.SetFloat("_kind", gap > 0 ? 1f : 0f);
                Tlf.DOMoveY(lfpos + 1f, 0.5f).SetEase(Ease.OutQuart);
            }
            else
            {
                Tlf.gameObject.SetActive(false);
            }
        }
    }

    private void LaneEffect(int start, int end, Color color)
    {
        for (int i = start; i < end; i++)
        {
            MeshRenderer mesh = laneMesh.transform.GetChild(i).GetComponent<MeshRenderer>();
            mesh.material.DOKill();
            mesh.material.color = color;
            mesh.material.DOFade(0f, 0.5f).SetEase(Ease.InQuart);
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

                // Missエフェクト
                GameObject jIns = judgePool.GetComponent<MyObjectPool>().SetObject();
                int s = _notesData.Value.GetStartLane();
                int e = _notesData.Value.GetEndLane();
                jIns.transform.position =
                    new Vector3(-6f + (s + e) * 0.25f, 0.5f, 0f);
                LaneEffect(s, e, new Color(1f, 0f, 0f, 70f / 255f));
                jIns.transform.rotation = Quaternion.identity;
                jIns.GetComponent<JudgeController>().Setting('M');
                combo = 0;
                resultJudge[4]++;
                damageController.Damage();

                // pmにデータを詰める
                int kind = -1;
                switch (_notesData.Value.GetKind())
                {
                    case 'N':
                    case 'L':
                    case 'S':
                        kind = 0;
                        break;
                    case 'H':
                        s = 4;
                        kind = 1;
                        break;
                    case 'M':
                    case 'T':
                    case 'B':
                        kind = 2;
                        break;
                    case 'F':
                        kind = 3;
                        break;
                }
                if (kind == 0)
                    tapJudge[30]++;
                else
                    pm[kind * 2 - 1]++;

                if (NotesData.Count == 0) break;
                _notesData = NotesData[0];
            }

            // Hold, Flickの処理
            int index = 0;
            while (NotesData.Count > index && (NotesData[index].Value.GetTime() - 10) / 1000f < (gameDirector.musicTime + tapOffset))
            {
                char ki = NotesData[index].Value.GetKind();
                if (ki == 'H' || ki == 'M' || ki == 'T' || ki == 'B')
                {
                    var n = NotesData[index].Value;
                    var isTaps = touchDirector.laneTouching;

                    bool tap = false;
                    for (int i = Mathf.Max(n.GetStartLane() - 1, 0); i <= Mathf.Min(n.GetEndLane(), 23); i++)
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
            while (NotesData.Count > index && (NotesData[index].Value.GetTime() - 10) / 1000f < (gameDirector.musicTime + tapOffset))
            {
                char ki = NotesData[index].Value.GetKind();
                if (ki == 'F')
                {
                    var n = NotesData[index].Value;
                    var isFlicks = touchDirector.laneFlicking;

                    bool flick = false;
                    for (int i = Mathf.Max(n.GetStartLane() - 1, 0); i <= Mathf.Min(n.GetEndLane(), 23); i++)
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
            score = (int)((decimal)notesN10 / totalN10 * 1000000);
        }
        
        // AP, フルコン中のJustFlameの色
        if (isColor)
        {
            if (isFull == 2)
            {
                if (resultJudge[2] + resultJudge[3] + resultJudge[4] > 0)
                {
                    isFull = 1;
                    justFlame.startColor = new Color(0f, 59f / 255f, 1f, 1f);
                    justFlame.endColor = new Color(0f, 59f / 255f, 1f, 1f);
                }
            }
            else if (isFull == 1)
            {
                if (resultJudge[3] + resultJudge[4] > 0)
                {
                    isFull = 0;
                    justFlame.startColor = Color.white;
                    justFlame.endColor = Color.white;
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
                    int touchLane = (_notesData.Value.GetStartLane() + _notesData.Value.GetEndLane()) / 2;
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
