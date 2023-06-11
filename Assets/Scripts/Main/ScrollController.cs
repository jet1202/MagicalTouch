using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static SelectData;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private GameObject songData;
    
    [SerializeField] private List<GameObject> boxes;
    private int[] boxDisplay = new int[12];
    private List<SongList> songList;

    private float _scrollNumber = 0f;
    private int number = 0;

    // 初期設定
    public void Setting(List<SongList> list)
    {
        songList = list;
        sortList(mode);
    }

    // Sort mode is changed
    private void sortList(int m)
    {
        SortMode modeEnum = (SortMode)Enum.ToObject(typeof(SortMode), m);
        mode = modeEnum;
        ListSort();
    }
    
    private void sortList(SortMode m)
    {
        mode = m;
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
        IOrderedEnumerable<SongList> s;
        switch (mode)
        {
            case SortMode.Default:
                s = songList.OrderBy(x => x.id);
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
        songList = new List<SongList>(s);
        
        
    }

    void BarChange(float position)
    {
        
    }
}
