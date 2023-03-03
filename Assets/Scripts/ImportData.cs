using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ImportData : MonoBehaviour
{
    private Base _baseData;
    private List<Note> _notesData;

    public IEnumerator ImportBase(string name)
    {
        string url = Application.streamingAssetsPath + $"\\SongData\\{name}\\base.json";
        
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.ConnectionError)
        {
            string jsonStr = req.downloadHandler.text;

            var saveData = JsonUtility.FromJson<Base>(jsonStr);

            _baseData = new Base();
            _baseData.filePath = saveData.filePath;
            _baseData.bpm = saveData.bpm;
            _baseData.offset = saveData.offset;
        }

        yield return _baseData;
    }

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

    IEnumerator getData(string filename)
    {
        UnityWebRequest req = UnityWebRequest.Get(filename);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.ConnectionError)
        {
            string jsonStr = req.downloadHandler.text;

            NoteSaveData saveData = JsonUtility.FromJson<NoteSaveData>(jsonStr);

            Note note;
            foreach (var n in saveData.item)
            {
                note = new Note(n.time, n.startLane, n.endLane, n.kind, n.length);
                _notesData.Add(note);
            }
        }
    }
}
