using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using static SelectData;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private SelectDirector selectDirector;
    [SerializeField] private SubDirector subDirector;
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private GameObject CenterBoxes;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private GameObject songData;
    [SerializeField] private GameObject detailData;
    [SerializeField] private TextMeshProUGUI sortText;
    
    [SerializeField] private ImportScore importScore;
    [SerializeField] private List<GameObject> boxes;
    private float boxAngle;
    private int[] boxDisplay;
    private List<SongDataList> songList;

    [SerializeField] private Texture defaultImage;

    [SerializeField] private GameObject subCube;
    
    [SerializeField] private Sprite[] rankSprites;

    private float _scrollNumber;
    private int number;
    public int leng;
    public Tweener horizontalTweener;
    public Tweener verticalTweener;

    [Header("Scrollの摩擦力")]
    public float friction = 0.1f;
    [Header("Scrollが端に行ったときの戻る強さ")]
    public float returnForce = 1f;
    [Header("Scrollが止まるライン")]
    public float stopForce = 0.3f;
    [Header("慣性")]
    public float inertia = 0;
    [Header("cubeの回転")]
    public float cubeRotate = 0f;

    public bool isScrollDragging = false;
    public bool isFieldDragging = false;
    public bool isScrolling = false;
    
    private static readonly int Color1 = Shader.PropertyToID("_color");
    private static readonly int Image1 = Shader.PropertyToID("_Image");
    private static readonly int ImageSize = Shader.PropertyToID("_ImageSize");

    // 初期設定
    public IEnumerator Setting(List<SongList> list)
    {
        boxAngle = 360f / boxes.Count;
        boxDisplay = Enumerable.Repeat<int>(-2, boxes.Count).ToArray();
        
        // songListに基本情報(SongList) + Score + Jacketを追加、管理する
        songList = new List<SongDataList>();
        foreach (var s in list)
        {
            SongDataList n = new SongDataList();
            n.title = s.title;
            n.id = s.id;
            n.constant = s.constant;
            n.division = s.division;
            n.composer = s.composer;
            n.number = s.number;
            n.chorus = s.chorus;
            
            IEnumerator corutine = importScore.ImportJacket(n.id);
            yield return StartCoroutine(corutine);
            Texture t;
            if (corutine.Current == null)
                t = defaultImage;
            else
                t = (Texture)corutine.Current;
            n.image = t;
            n.detail = importScore.GetScore(s.id); // Score反映
            
            songList.Add(n);
        }

        // いずれどこにおいていたかを保存できるようにしたい
        AdjustNumber(0);

        ListSort();
        AdjustNumber(SelectData.number);
        StartCoroutine(audioPlayer.SetMusic(songList[number].id, songList[number].chorus));
        AdjustDifficulty(-90 * (int)difficulty, false);
        SongChange();
    }

    /// <summary>
    ///     Scrollの初期設定
    /// </summary>
    /// <param name="num"></param>
    private void AdjustNumber(int num)
    {
        number = num;
        leng = songList.Count;
        _scrollNumber = number;
        // Debug.Log($"number: {number}, leng: {leng}");
        scrollbar.size = 1f / (leng - 1);
        scrollbar.value =
            leng - 1 == 0 ? 0f : (float)number / (leng - 1);
        
        sortText.text = mode.ToString();
    }
    
    /// <summary>
    ///     Sortの変更
    /// </summary>
    public void SortChange()
    {
        int m = ((int)mode + 1) % 4;
        SortMode modeEnum = (SortMode)Enum.ToObject(typeof(SortMode), m);
        mode = modeEnum;
        
        ListSort();
    }
    
    /// <summary>
    ///     難易度の変更
    /// </summary>
    /// <param name="d"></param>
    public void ChangeDifficulty(int d)
    {
        difficulty = (DifficultyMode)Enum.ToObject(typeof(DifficultyMode), d);
        // Debug.Log($"difficulty = {(int)difficulty}, {difficulty.ToString()}");
        ListSort();
    }
    
    /// <summary>
    ///     Listのソート
    /// </summary>
    private void ListSort()
    {
        if (songList.Count == 0) return;
        
        string nowSong = songList[number].id;
        int num;
        
        // sort
        IOrderedEnumerable<SongDataList> s;
        switch (mode)
        {
            case SortMode.Default:
                s = songList.OrderBy(x => x.number);
                break;
            case SortMode.Name:
                s = songList.OrderBy(x => x.title).ThenBy(x => x.number);
                break;
            case SortMode.Difficulty:
                s = songList.OrderBy(x => x.constant[(int)difficulty]).ThenBy(x => x.number);
                break;
            case SortMode.Score:
                s = songList.OrderBy(x => x.detail[(int)difficulty].score); // Scoreの反映
                break;
            default:
                s = songList.OrderBy(x => x.number);
                break;
        }
        songList = new List<SongDataList>(s);

        num = songList.FindIndex(n => nowSong == n.id);
        
        boxDisplay = Enumerable.Repeat<int>(-2, boxes.Count).ToArray();
        
        AdjustNumber(num);
        SongChange();
    }
    
    /// <summary>
    ///     Cubeの画像を変更
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="number"></param>
    private void CubeChange(int cube, int number)
    {
        if (cube < 0) cube += boxes.Count;
        
        float size;
        Texture image;
        
        if (number < 0 || leng <= number)
        {
            size = 1f;
            image = defaultImage;
            number = -1;
        }
        else
        {
            size = 0.9f;
            image = songList[number].image;
        }

        if (boxDisplay[cube] == number) return;
        
        GameObject box = boxes[cube];
        Material front = box.transform.GetChild(1).GetComponent<Renderer>().material;
        Material top = box.transform.GetChild(2).GetComponent<Renderer>().material;
        Material back = box.transform.GetChild(3).GetComponent<Renderer>().material;
        Material bottom = box.transform.GetChild(0).GetComponent<Renderer>().material;
        
        SetCube(front, DifficultyMode.Free, image, size);
        SetCube(top, DifficultyMode.Normal, image, size);
        SetCube(back, DifficultyMode.Busy, image, size);
        SetCube(bottom, DifficultyMode.Impossible, image, size);

        boxDisplay[cube] = number;
    }
    
    /// <summary>
    ///     Songの変更を反映
    /// </summary>
    private void SongChange()
    {
        // 反対側のCubeの画像を変更
        for (int i = number - boxes.Count / 2; i < boxes.Count; i++)
        {
            int c = i % boxes.Count;
            CubeChange(c, i);
        }
        
        // Song詳細情報を表示
        var song = songList[number];
        
        var title = songData.transform.GetChild(1);
        var difficulty2 = songData.transform.GetChild(2);
        var composer = songData.transform.GetChild(3);
        var score = detailData.transform.GetChild(1).GetChild(0);
        var rank = detailData.transform.GetChild(1).GetChild(1);
        var countT = detailData.transform.GetChild(1).GetChild(3);
        var countT2 = detailData.transform.GetChild(1).GetChild(5);
        
        Color color;
        color = GetColor(difficulty);
        
        title.GetComponent<TextMeshProUGUI>().text = song.title;
        difficulty2.GetChild(0).GetComponent<Image>().color = color;
        difficulty2.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            song.constant[(int)difficulty] != 0
                ? (song.constant[(int)difficulty] / 10).ToString()
                : "-";
        composer.GetComponent<TextMeshProUGUI>().text = song.composer;

        var s = song.detail[(int)difficulty];
        score.GetComponent<TextMeshProUGUI>().text = s.score.ToString("D7");
        if (s.tryCount == 0)
        {
            rank.gameObject.SetActive(false);
            countT.GetComponent<TextMeshProUGUI>().text = " NULL\n NULL\n";
            countT2.GetComponent<TextMeshProUGUI>().text = " NULL\n NULL\n";
        }
        else
        {
            rank.GetComponent<Image>().sprite = rankSprites[ResultDirector.GetRank(s.score) + (s.isStar ? 0 : 1)];
            rank.gameObject.SetActive(true);
            countT.GetComponent<TextMeshProUGUI>().text = $" {s.tryCount}\n {s.compCount}";
            countT2.GetComponent<TextMeshProUGUI>().text = $" {s.fcCount}\n {s.apCount}";
        }
    }

    /// <summary>
    ///     Scrollのドラッグ開始
    /// </summary>
    /// <param name="position"></param>
    public void BarChange(float position)
    {
        if (position > 1f)
        {
            inertia /= 1f + returnForce;
            position = 0.5f * (float)Math.Atan(2f * (position - 1)) + 1f;
            // scrollbar.value = 1f;
            // return;
        }
        if (position < 0f)
        {
            inertia /= 1f + returnForce;
            position = 0.5f * (float)Math.Atan(2f * position);
            // scrollbar.value = 0f;
            // return;
        }
        
        _scrollNumber = position * (leng - 1);
        int Bnumber = number;
        number = Math.Clamp((int)Math.Round(_scrollNumber), 0, leng - 1);

        Quaternion rot = Quaternion.AngleAxis(-_scrollNumber * boxAngle, Vector3.up);
        CenterBoxes.transform.rotation = rot;
        
        if (Bnumber != number)
            SongChange();
    }

    /// <summary>
    ///     Scrollの位置を調整
    /// </summary>
    public void AdjustPosition()
    {
        float ini = leng - 1 == 0 ? 0f : _scrollNumber / (leng - 1);
        float end = leng - 1 == 0 ? 0f : number / (float)(leng - 1);
        horizontalTweener = DOTween.To(
            () => ini,
            (x) =>
            {
                scrollbar.value = x;
                _scrollNumber = x;
            },
            end,
            0.3f
            );
        
        // 音楽の変更
        StartCoroutine(audioPlayer.SetMusic(songList[number].id, songList[number].chorus));
    }

    /// <summary>
    ///     縦Scrollの位置を調整
    /// </summary>
    /// <param name="e"></param>
    /// <param name="isV"></param>
    public void AdjustDifficulty(int e, bool isV)
    {
        if (isV)
        {
            verticalTweener = DOTween.To(
                () => cubeRotate,
                (x) =>
                {
                    cubeRotate = x;
                    for (int i = 0; i < boxes.Count; i++)
                    {
                        Quaternion localAngle = Quaternion.AngleAxis(cubeRotate, Vector3.right);
                        var lot = boxes[i].transform.localRotation;
                        boxes[i].transform.localRotation = localAngle;
                        boxes[i].transform.Rotate(0, i * boxAngle, 0, Space.World);
                    }
                },
                e,
                0.3f
            );
        }
        else
        {
            cubeRotate = e;
            for (int i = 0; i < boxes.Count; i++)
            {
                Quaternion localAngle = Quaternion.AngleAxis(cubeRotate, Vector3.right);
                var lot = boxes[i].transform.localRotation;
                boxes[i].transform.localRotation = localAngle;
                boxes[i].transform.Rotate(0, i * boxAngle, 0, Space.World);
            }
        }
    }

    /// <summary>
    ///     cubeの縦回転（x軸回転）
    /// </summary>
    /// <param name="del"></param>
    public void CubeRotation(float del)
    {
        cubeRotate += del;
        for (int i = 0; i < boxes.Count; i++)
        {
            // 一度回転軸を合わせてから回転
            Quaternion localAngle = Quaternion.AngleAxis(cubeRotate, Vector3.right);
            var lot = boxes[i].transform.localRotation;
            boxes[i].transform.localRotation = localAngle;
            boxes[i].transform.Rotate(0, i * boxAngle, 0, Space.World);
        }
    }

    private void Update()
    {
        if (isScrolling) // (Math.Abs(inertia) > 0f)
        {
            scrollbar.value -= leng - 1 == 0 ? 0f :inertia * (1f / boxAngle) / (leng - 1);
            inertia /= 1f + friction;

            // スクロールスピードが一定以下になったら止め、位置を調整
            if (Math.Abs(inertia) < stopForce)
            {
                inertia = 0f;
                isScrolling = false;
                AdjustPosition();
            }
        }
    }

    /// <summary>
    ///     Difficultyの色を取得
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    public Color GetColor(DifficultyMode m)
    {
        Color color = new Color();
        switch (m)
        {
            case DifficultyMode.Free:
                color = new Color(100f / 255f, 255f / 255f, 100f / 255f);
                break;
            case DifficultyMode.Normal:
                color = new Color(100f / 255f, 100f / 255f, 255f / 255f);
                break;
            case DifficultyMode.Busy:
                color = new Color(255f / 255f, 255f / 255f, 100f / 255f);
                break;
            case DifficultyMode.Impossible:
                color = new Color(255f / 255f, 100f / 255f, 100f / 255f);
                break;
            default:
                color = new Color(100f / 255f, 100f / 255f, 100f / 255f);
                break;
        }

        return color;
    }

    private void SetCube(Material m, DifficultyMode d, Texture t, float s)
    {
        m.SetColor(Color1, GetColor(d));
        m.SetTexture(Image1, t);
        m.SetFloat(ImageSize, s);
    }

    public void PlayButtonPush()
    {
        bool isPlayOk = isScrolling || isScrollDragging || isFieldDragging;
        if (!isPlayOk)
        {
            // スクロールの途中ではない
            var song = songList[number];
            if (song.constant[(int)difficulty] != 0)
            {
                // 指定した曲の難易度の譜面が存在する
                GameData.title = song.title;
                GameData.id = song.id;
                GameData.difficult = difficulty;
                GameData.difficulty = song.constant[(int)difficulty];
                SelectData.number = number;
                
                selectDirector.MoveGame();

                var s = subCube.transform;
                Material m;
                Texture t;
                for (int i = 0; i < 6; i++)
                {
                    m = s.GetChild(i).GetComponent<Renderer>().material;
                    if (i == 1) t = song.image;
                    else t = defaultImage;
                    SetCube(m, difficulty, t, 0.9f);
                }
                
                // データのセーブ
                int saveIndex = SaveData.song.item.FindIndex(x => x.Id == song.id);
                if (saveIndex == -1)
                {
                    SongData n = new SongData(song.id);
                    n.detail = new SongDetail[4];
                    for (int i = 0; i < 4; i++)
                    {
                        n.detail[i] = new SongDetail();
                    }
                    SaveData.song.item.Add(n);
                    saveIndex = SaveData.song.item.Count - 1;
                }
                SaveData.song.item[saveIndex].detail[(int)difficulty].tryCount++;
                SaveDataSave.ScoreWrite();

                IEnumerator corutine = subDirector.MoveGameFromSelect(song);
                StartCoroutine(corutine);
            }
        }
    }
}
