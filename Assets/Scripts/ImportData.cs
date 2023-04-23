using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class ImportData : MonoBehaviour
{
    private List<Note> _notesData;
    private NoteAddition _additionData;
    private Dictionary<int, SlideMaintain[]> _slideMaintainData;
    private Dictionary<Note, SlideMaintain[]> _slideData;

    public IEnumerator ImportSheet(string name, string difficulty)
    {
        _notesData = new List<Note>();
        string url = Application.streamingAssetsPath + $"/SongData/{name}/{difficulty}.json";

        // var formatter = new BinaryFormatter();
        // FileStream fs = new FileStream(url, FileMode.Open);
        // _notesData = (List<Note>)formatter.Deserialize(fs);
        yield return StartCoroutine(getData(url));

        yield return _notesData;
    }

    public IEnumerator ImportSlide()
    {
        yield return _slideData;
    }

    public IEnumerator ImportAddition(string name, string difficulty)
    {
        _additionData = new NoteAddition();
        string url = Application.streamingAssetsPath + $"/SongData/{name}/{difficulty}Addition.json";
        
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.ConnectionError)
        {
            string jsonStr = req.downloadHandler.text;

            NoteAddition saveData = JsonUtility.FromJson<NoteAddition>(jsonStr);

            _additionData = saveData;
        }
        
        yield return _additionData;
    }

    IEnumerator getData(string filename)
    {
        UnityWebRequest req = UnityWebRequest.Get(filename);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.ConnectionError)
        {
            string jsonStr = req.downloadHandler.text;

            NoteSaveData saveData = JsonUtility.FromJson<NoteSaveData>(jsonStr);

            _slideMaintainData = new Dictionary<int, SlideMaintain[]>();
            if (saveData.slideItem != null)
            {
                foreach (var ss in saveData.slideItem)
                {
                    _slideMaintainData.Add(ss.number, ss.item);
                }
            }

            // notesDataとslidesDataにデータを格納
            Note note;
            foreach (var n in saveData.item)
            {
                note = new Note(n.time100, n.startLane, n.endLane, n.kind, n.length100);
                if (n.kind == 'S')
                    _slideData.Add(note, _slideMaintainData[n.number]);
                else
                    _notesData.Add(note);
            }
        }
        else
        {
            throw new Exception();
        }
    }
}
