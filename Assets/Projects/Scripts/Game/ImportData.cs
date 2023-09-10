using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class ImportData : MonoBehaviour
{
    private List<Note> _notesData;
    private BpmSave _bpmData;
    private FieldSave _fieldData;
    private Dictionary<int, SlideMaintain[]> _slideMaintainData;
    private List<KeyValuePair<Note, SlideMaintain[]>> _slideData;

    public IEnumerator ImportSheet(string name, string difficulty)
    {
        _notesData = new List<Note>();
        string url = Application.streamingAssetsPath + $"/SongData/{name}/{difficulty}/Data.json";

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

    public IEnumerator ImportBpm(string name, string difficulty)
    {
        _bpmData = new BpmSave();
        string url = Application.streamingAssetsPath + $"/SongData/{name}/{difficulty}/Bpm.json";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.ConnectionError)
            {
                string jsonStr = req.downloadHandler.text;

                BpmSave saveData = JsonUtility.FromJson<BpmSave>(jsonStr);

                _bpmData = saveData;
            }

            yield return _bpmData;
        }
    }

    public IEnumerator ImportField(string name, string difficulty)
    {
        _fieldData = new FieldSave();
        string url = Application.streamingAssetsPath + $"/SongData/{name}/{difficulty}/Field.json";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.ConnectionError)
            {
                string jsonStr = req.downloadHandler.text;

                FieldSave saveData = JsonUtility.FromJson<FieldSave>(jsonStr);

                _fieldData = saveData;

                yield return _fieldData;
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator getData(string filename)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(filename))
        {
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
                _slideData = new List<KeyValuePair<Note, SlideMaintain[]>>();
                foreach (var n in saveData.item)
                {
                    note = new Note(n.number, n.time, n.startLane, n.endLane, n.kind, n.length, n.field);
                    if (n.kind == 'S')
                    {
                        var data = _slideMaintainData[n.number];
                        _slideData.Add(new KeyValuePair<Note, SlideMaintain[]>(note, data));
                    }
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
}
