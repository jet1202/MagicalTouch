using System;
using System.Collections;
using System.Collections.Generic;
using NoteData;
using UnityEngine;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] public bool isRikuMethod;
    [SerializeField] private GameObject normalNotes;
    [SerializeField] private GameObject judgePerfect;
    [SerializeField] private GameObject judgeGreat;
    [SerializeField] private GameObject judgeGood;
    [SerializeField] private GameObject judgeMiss;
    private List<Note>[] NotesData = new List<Note>[6];
    private float Speed;
    private const float missGap = 0.2f;
    private Note _notesData;
    private string _judgeMassage;

    void Start()
    {
        Speed = GetComponent<NotesController>().Speed;
        if (isRikuMethod)
            NotesData = NotesInformation_Riku.InitNoteData("Test");
        else
            NotesData = NotesInformation.InitNoteData("Test");

        int len;
        float pos_x;
        for (int i = 0; i < 6; i++)
        {
            len = NotesData[i].Count;
            pos_x = -5f + i * 2;
            for (int j = 0; j < len; j++)
            {
                GameObject ins = Instantiate(normalNotes, 
                    new Vector3(pos_x, 0, NotesData[i][j].JustTime * Speed),
                    Quaternion.identity, this.transform);
                NotesData[i][j].setObj(ins);
            }
        }
    }

    public void BeginTouch(int laneNumber)
    {
        if (NotesData[laneNumber].Count == 0) return;
        
        float gap = Math.Abs(NotesData[laneNumber][0].JustTime - Time.time);

        if (gap < missGap)
        {
            Note data = NotesData[laneNumber][0];
            Destroy(data.noteObject);
            NotesData[laneNumber].RemoveAt(0);

            if (gap < 0.05f)
            {
                Instantiate(judgePerfect, new Vector3(-5 + laneNumber * 2, 0.5f, 0), Quaternion.identity);
                _judgeMassage = "Perfect";
            }
            else if (gap < 0.1f)
            {
                Instantiate(judgeGreat, new Vector3(-5 + laneNumber * 2, 0.5f, 0), Quaternion.identity);
                _judgeMassage = "Great";
            }
            else
            {
                Instantiate(judgeGood, new Vector3(-5 + laneNumber * 2, 0.5f, 0), Quaternion.identity);
                _judgeMassage = "Good";
            }
            Debug.Log(_judgeMassage);
        }
    }

    private void Update()
    {
        for (int i = 0; i < 6; i++)
        {
            if (NotesData[i].Count == 0) continue;
            _notesData = NotesData[i][0];
            if (_notesData.JustTime + missGap < Time.time)
            {
                Destroy(_notesData.noteObject);
                NotesData[i].RemoveAt(0);
                
                Instantiate(judgeMiss, new Vector3(-5 + i * 2, 0.5f, 0), Quaternion.identity);
                Debug.Log("Miss");
            }
        }
    }
}
