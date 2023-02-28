using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesDirector : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
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

        int len = notesSheet.Count;
        for (int i = 0; i < len; i++)
        {
            GameObject ins = Instantiate(normalNotes, this.transform);
            _notesData = new KeyValuePair<GameObject, Note>(ins, notesSheet[i]);
            NoteSettings(_notesData);
            NotesData.Add(_notesData);
        }
    }

    private void NoteSettings(KeyValuePair<GameObject, Note> noteData)
    {
        float posx = -6f + (noteData.Value.GetEndLane() + noteData.Value.GetStartLane()) * 0.5f;
        float sizex = noteData.Value.GetEndLane() - noteData.Value.GetStartLane();
        float time = noteData.Value.GetTime() * Speed;
        
        noteData.Key.transform.localPosition = new Vector3(posx, 0f, time);
        noteData.Key.transform.localScale = new Vector3(sizex, 0.1f, 1f);
        noteData.Key.transform.rotation = Quaternion.identity;
    }

    public void BeginTouch(int laneNumber)
    {
        int con = NotesData.Count;
        if (con == 0) return;

        // どのノーツをタップしたか判定
        bool isGetNote = false;
        float gap = -1f;
        int i;
        for (i = 0; i < con; i++)
        {
            Note data = NotesData[i].Value;
            gap = Mathf.Abs(data.GetTime() - gameDirector.musicTime);
            if (gap > missGap)
            {
                break;
            }

            if (data.GetStartLane() - 1 <= laneNumber && laneNumber <= data.GetEndLane())
            {
                isGetNote = true;
                break;
            }
        }
        
        // タップしたノーツの判定
        if (isGetNote)
        {
            Vector3 notePos = new Vector3(-6f + (NotesData[i].Value.GetEndLane() + NotesData[i].Value.GetStartLane()) * 0.5f, 0.5f, 0);
            Destroy(NotesData[i].Key);
            NotesData.RemoveAt(i);

            if (gap < 0.05f)
            {
                Instantiate(judgePerfect, notePos, Quaternion.identity);
                _judgeMassage = "Perfect";
            }
            else if (gap < 0.1f)
            {
                Instantiate(judgeGreat, notePos, Quaternion.identity);
                _judgeMassage = "Great";
            }
            else
            {
                Instantiate(judgeGood, notePos, Quaternion.identity);
                _judgeMassage = "Good";
            }
            Debug.Log(_judgeMassage);
        }
    }
    

    private void Update()
    {
        if (NotesData.Count == 0) return;
        
        _notesData = NotesData[0];
        if (_notesData.Value.GetTime() + missGap < gameDirector.musicTime)
        { 
            Destroy(_notesData.Key);
            NotesData.RemoveAt(0);
            
            Instantiate(judgeMiss, new Vector3(-6 + (_notesData.Value.GetStartLane() + _notesData.Value.GetEndLane()), 0.5f, 0), Quaternion.identity);
            Debug.Log("Miss");
        }
    }
}
