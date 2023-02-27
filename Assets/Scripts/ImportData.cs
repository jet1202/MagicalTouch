using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class ImportData
{
    private static KeyValuePair<string, KeyValuePair<int, float>> _baseData;
    private static List<Note> _notesData;

    public static KeyValuePair<string, KeyValuePair<int, float>> ImportBase(string name)
    {
        string url = Application.streamingAssetsPath + $"\\SongData\\{name}\\base.bin";

        var formatter = new BinaryFormatter();
        FileStream fs = new FileStream(url, FileMode.Open);
        _baseData = (KeyValuePair<string, KeyValuePair<int, float>>)formatter.Deserialize(fs);
        fs.Close();

        return _baseData;
    }

    public static List<Note> ImportSheet(string name, string difficulty)
    {
        _notesData = new List<Note>();
        string url = Application.streamingAssetsPath + $"\\SongData\\{name}\\{difficulty}.bin";

        var formatter = new BinaryFormatter();
        FileStream fs = new FileStream(url, FileMode.Open);
        _notesData = (List<Note>)formatter.Deserialize(fs);

        return _notesData;
    }
}
