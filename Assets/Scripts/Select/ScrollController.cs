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
    [SerializeField] private TextMeshProUGUI sortText;
    
    [SerializeField] private ImportScore importScore;
    [SerializeField] private List<GameObject> boxes;
    private int[] boxDisplay = Enumerable.Repeat<int>(-2, 12).ToArray();
    private List<SongDataList> songList;

    [SerializeField] private Texture defaultImage;

    [SerializeField] private GameObject subBoard;

    private float _scrollNumber;
    private int number;
    public int leng;
    public Tweener horizontalTweener;
    public Tweener verticalTweener;

    public float friction = 0.1f;
    public float stopForce = 0.3f;
    public float inertia = 0;
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
            n.score = new int[]{0, 0, 0, 0}; // Score反映
            
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

    // Sort mode is changed
    public void SortChange()
    {
        int m = ((int)mode + 1) % 4;
        SortMode modeEnum = (SortMode)Enum.ToObject(typeof(SortMode), m);
        mode = modeEnum;
        
        ListSort();
    }

    // Difficulty is changed
    public void ChangeDifficulty(int d)
    {
        difficulty = (DifficultyMode)Enum.ToObject(typeof(DifficultyMode), d);
        // Debug.Log($"difficulty = {(int)difficulty}, {difficulty.ToString()}");
        ListSort();
    }

    // Play sort
    void ListSort()
    {
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
                s = songList.OrderBy(x => x.id); // Scoreの反映
                break;
            default:
                s = songList.OrderBy(x => x.number);
                break;
        }
        songList = new List<SongDataList>(s);

        num = songList.FindIndex(n => nowSong == n.id);
        
        boxDisplay = Enumerable.Repeat<int>(-2, 12).ToArray();
        
        AdjustNumber(num);
        SongChange();
    }

    // Cubeに変更を反映
    void CubeChange(int cube, int number)
    {
        if (cube < 0) cube += 12;
        
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
            size = 0.85f;
            image = songList[number].image;
        }

        if (boxDisplay[cube] == number) return;
        
        GameObject box = boxes[cube];
        Material front = box.transform.GetChild(1).GetComponent<Renderer>().material;
        Material top = box.transform.GetChild(2).GetComponent<Renderer>().material;
        Material back = box.transform.GetChild(3).GetComponent<Renderer>().material;
        Material bottom = box.transform.GetChild(0).GetComponent<Renderer>().material;
        
        SetCube(front, DifficultyMode.Normal, image, size);
        SetCube(top, DifficultyMode.Hard, image, size);
        SetCube(back, DifficultyMode.Expert, image, size);
        SetCube(bottom, DifficultyMode.Impossible, image, size);

        boxDisplay[cube] = number;
    }
    
    void SongChange()
    {
        // 反対側のCubeの画像を変更
        for (int i = number - 5; i < number + 7; i++)
        {
            int c = i % 12;
            CubeChange(c, i);
        }
        
        // Song詳細情報を表示
        var title = songData.transform.GetChild(1);
        var difficulty2 = songData.transform.GetChild(2);
        var composer = songData.transform.GetChild(3);
        
        Color color;
        color = GetColor(difficulty);
        
        title.GetComponent<TextMeshProUGUI>().text = songList[number].title;
        difficulty2.GetChild(0).GetComponent<Image>().color = color;
        difficulty2.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            songList[number].constant[(int)difficulty] != 0
                ? (songList[number].constant[(int)difficulty] / 10).ToString()
                : "-";
        composer.GetComponent<TextMeshProUGUI>().text = songList[number].composer;
    }

    public void BarChange(float position)
    {
        if (position > 1f)
        {
            scrollbar.value = 1f;
            return;
        }
        if (position < 0f)
        {
            scrollbar.value = 0f;
            return;
        }

        _scrollNumber = position * (leng - 1);
        int Bnumber = number;
        number = (int)Math.Round(_scrollNumber);

        Quaternion rot = Quaternion.AngleAxis(-_scrollNumber * 30, Vector3.up);
        CenterBoxes.transform.rotation = rot;
        
        if (Bnumber != number)
            SongChange();
    }

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
    }

    public void AdjustDifficulty(int e, bool isV)
    {
        if (isV)
        {
            verticalTweener = DOTween.To(
                () => cubeRotate,
                (x) =>
                {
                    cubeRotate = x;
                    for (int i = 0; i < 12; i++)
                    {
                        Quaternion localAngle = Quaternion.AngleAxis(cubeRotate, Vector3.right);
                        var lot = boxes[i].transform.localRotation;
                        boxes[i].transform.localRotation = localAngle;
                        boxes[i].transform.Rotate(0, i * 30, 0, Space.World);
                    }
                },
                e,
                0.3f
            );
        }
        else
        {
            cubeRotate = e;
            for (int i = 0; i < 12; i++)
            {
                Quaternion localAngle = Quaternion.AngleAxis(cubeRotate, Vector3.right);
                var lot = boxes[i].transform.localRotation;
                boxes[i].transform.localRotation = localAngle;
                boxes[i].transform.Rotate(0, i * 30, 0, Space.World);
            }
        }
    }

    public void CubeRotation(float del)
    {
        cubeRotate += del;
        for (int i = 0; i < 12; i++)
        {
            Quaternion localAngle = Quaternion.AngleAxis(cubeRotate, Vector3.right);
            var lot = boxes[i].transform.localRotation;
            boxes[i].transform.localRotation = localAngle;
            boxes[i].transform.Rotate(0, i * 30, 0, Space.World);
        }
    }

    private void Update()
    {
        if (isScrolling) // (Math.Abs(inertia) > 0f)
        {
            scrollbar.value -= leng - 1 == 0 ? 0f :inertia * (1f / 30f) / (leng - 1);
            inertia /= 1f + friction;

            if (Math.Abs(inertia) < stopForce)
            {
                inertia = 0f;
                isScrolling = false;
                AdjustPosition();
                StartCoroutine(audioPlayer.SetMusic(songList[number].id, songList[number].chorus));
            }
        }
    }

    public Color GetColor(DifficultyMode m)
    {
        Color color = new Color();
        switch (m)
        {
            case DifficultyMode.Normal:
                color = new Color(100f / 255f, 255f / 255f, 100f / 255f);
                break;
            case DifficultyMode.Hard:
                color = new Color(100f / 255f, 100f / 255f, 255f / 255f);
                break;
            case DifficultyMode.Expert:
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
                GameData.difficult = difficulty.ToString();
                GameData.difficulty = song.constant[(int)difficulty];
                SelectData.number = number;
                
                selectDirector.MoveGame();

                Material m = subBoard.GetComponent<Renderer>().material;
                SetCube(m, difficulty, song.image, 0.85f);
                audioPlayer.StopBgm();

                IEnumerator corutine = subDirector.MoveGameFromSelect(song);
                StartCoroutine(corutine);
            }
        }
    }
}
