using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static SelectData;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private GameObject CenterBoxes;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private GameObject songData;
    
    [SerializeField] private ImportScore importScore;
    [SerializeField] private List<GameObject> boxes;
    private int[] boxDisplay = Enumerable.Repeat<int>(-2, 12).ToArray();
    private List<SongDataList> songList;

    [SerializeField] private Texture defaultImage;

    private float _scrollNumber;
    private int number = 0;
    private int leng;
    public Tweener t;

    public float friction = 0.1f;
    public float stopForce = 1f;
    public float inertia = 0;

    public bool isScrollDragging = false;
    public bool isFieldDragging = false;
    
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
            n.difficult = s.difficult;
            n.constant = s.constant;
            n.division = s.division;
            n.number = s.number;
            
            IEnumerator corutine = importScore.ImportJacket(n.id);
            yield return StartCoroutine(corutine);
            var t = (Texture)corutine.Current;
            n.image = t;
            n.score = new int[]{0, 0, 0, 0}; // Score反映
            
            songList.Add(n);
        }

        number = 0; // いずれどの場所にいたかを保存できるようにしたい
        leng = songList.Count;
        _scrollNumber = number;
        Debug.Log($"number: {number}, leng: {leng}");
        scrollbar.size = 1f / (leng - 1);
        scrollbar.value = (float)number / (leng - 1);

        ListSort();
    }

    // Sort mode is changed
    private void sortList(int m)
    {
        SortMode modeEnum = (SortMode)Enum.ToObject(typeof(SortMode), m);
        mode = modeEnum;
        ListSort();
    }

    // Difficulty is changed
    private void ChangeDifficulty(int d)
    {
        difficulty = (DifficultyMode)Enum.ToObject(typeof(DifficultyMode), d);
        ListSort();
    }

    // Play sort
    void ListSort()
    {
        string nowSong = songList[number].id;
        
        // sort
        IOrderedEnumerable<SongDataList> s;
        switch (mode)
        {
            case SortMode.Default:
                s = songList.OrderBy(x => x.number);
                break;
            case SortMode.Name:
                s = songList.OrderBy(x => x.title);
                break;
            case SortMode.Difficulty:
                s = songList.OrderBy(x => x.difficult[(int)difficulty]);
                break;
            case SortMode.Score:
                s = songList.OrderBy(x => x.id); // Scoreの反映
                break;
            default:
                s = songList.OrderBy(x => x.id);
                break;
        }
        songList = new List<SongDataList>(s);
        
        ReflectDisplay();
    }

    // 難易度変更、ソートを反映
    public void ReflectDisplay()
    {
        for (int i = number - 5; i < number + 7; i++)
        {
            int c = i % 12;
            CubeChange(c, i);
        }
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

        front.SetColor(Color1, new Color(100f / 255f, 255f / 255f, 100f / 255f));
        top.SetColor(Color1, new Color(100f / 255f, 100f / 255f, 255f / 255f));
        back.SetColor(Color1, new Color(255f / 255f, 255f / 255f, 100f / 255f));
        bottom.SetColor(Color1, new Color(255f / 255f, 100f / 255f, 100f / 255f));
        
        front.SetTexture(Image1, image);
        top.SetTexture(Image1, image);
        back.SetTexture(Image1, image);
        bottom.SetTexture(Image1, image);
        
        front.SetFloat(ImageSize, size);
        top.SetFloat(ImageSize, size);
        back.SetFloat(ImageSize, size);
        bottom.SetFloat(ImageSize, size);

        boxDisplay[cube] = number;
    }
    
    void SongChange()
    {
        for (int i = number - 5; i < number + 7; i++)
        {
            int c = i % 12;
            CubeChange(c, i);
        }
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

    public void adjustPosition()
    {
        float ini = _scrollNumber / (leng - 1);
        t = DOTween.To(
            () => ini,
            (x) =>
            {
                scrollbar.value = x;
                _scrollNumber = x;
            },
            number / (float)(leng - 1),
            0.3f
            );
    }

    private void Update()
    {
        if (Math.Abs(inertia) > 0f)
        {
            scrollbar.value -= inertia * (1f / 30f) / (leng - 1);
            inertia /= 1f + friction;

            if (Math.Abs(inertia) < stopForce)
            {
                inertia = 0f;
                adjustPosition();
            }
        }
    }
}
