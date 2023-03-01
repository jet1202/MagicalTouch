using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class ImportData : MonoBehaviour
{
    private KeyValuePair<string, KeyValuePair<int, float>> _baseData;
    private List<Note> _notesData;

    public KeyValuePair<string, KeyValuePair<int, float>> ImportBase(string name)
    {
        string url = Application.streamingAssetsPath + $"\\SongData\\{name}\\base.bin";

        // var formatter = new BinaryFormatter();
        // FileStream fs = new FileStream(url, FileMode.Open);
        // _baseData = (KeyValuePair<string, KeyValuePair<int, float>>)formatter.Deserialize(fs);
        // fs.Close();

        return _baseData;
    }

    public List<Note> ImportSheet(string name, string difficulty)
    {
        _notesData = new List<Note>();
        string url = Application.streamingAssetsPath + $"/SongData/{name}/{difficulty}.bin";

        // var formatter = new BinaryFormatter();
        // FileStream fs = new FileStream(url, FileMode.Open);
        // _notesData = (List<Note>)formatter.Deserialize(fs);
        StartCoroutine(getData(url));

        return _notesData;
    }

    IEnumerator getData(string filename)
    {
        UnityWebRequest req = UnityWebRequest.Get(filename);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.ConnectionError)
        {
            var data = System.Text.Encoding.ASCII.GetBytes(req.downloadHandler.text);

            BinaryFormatter reader = new BinaryFormatter();
            var ms = new MemoryStream(data);
            _notesData = (List<Note>)reader.Deserialize(ms);
        }
    }
}
