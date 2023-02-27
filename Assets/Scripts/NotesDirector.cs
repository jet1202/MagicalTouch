using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameObject normalNotes;
    [SerializeField] private GameObject judgePerfect;
    [SerializeField] private GameObject judgeGreat;
    [SerializeField] private GameObject judgeGood;
    [SerializeField] private GameObject judgeMiss;
    private List<KeyValuePair<GameObject, Note>> NotesData = new List<KeyValuePair<GameObject, Note>>();
    private float Speed;
    private const float missGap = 0.2f;
    private KeyValuePair<GameObject, Note> _notesData;
    private string _judgeMassage;

    void Start()
    {
        Speed = GetComponent<NotesController>().Speed;

        var notesSheet = ImportData.ImportSheet("Test", "Expert");
        
        // 要修正＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿

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
            Note_old data = NotesData[laneNumber][0];
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
    
    // 要修正＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿

    private void Update()
    {
        if (NotesData.Count == 0) return;
        
        _notesData = NotesData[0];
        if (_notesData.Value.GetTime() + missGap < Time.time)
        { 
            Destroy(_notesData.Key);
            NotesData.RemoveAt(0);
            
            Instantiate(judgeMiss, new Vector3(-6 + (_notesData.Value.GetStartLane() + _notesData.Value.GetEndLane()), 0.5f, 0), Quaternion.identity);
            Debug.Log("Miss");
        }
    }
}
