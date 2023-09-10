using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Note_oldData;

namespace Note_oldData
{
    public class Note_old
    {
        public GameObject NoteObject;
        public char Type { get; }
        public float JustTime { get; }

        public Note_old(char type, float justTime)
        {
            NoteObject = null;
            Type = type;
            JustTime = justTime;
        }

        public void setObj(GameObject obj)
        {
            this.NoteObject = obj;
        }
    }
}

public static class NotesInformation
{
    private static TextAsset csvFile;
    private static List<string[]> csvData = new List<string[]>();
    private static int Bpm;
    private static float Tempo = 0;

    static void CsvReader(string songName)
    {
        csvFile = Resources.Load($"CSV/{songName}") as TextAsset;
        StringReader reader = new StringReader(csvFile.text);
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvData.Add(line.Split(','));
        }

        Bpm = int.Parse(csvData[0][1]);
        Tempo = 60f / Bpm;
    }

    public static List<Note_old>[] InitNote_oldData(string songName)
    {
        CsvReader(songName);

        List<Note_old>[] NoteData = new List<Note_old>[6];
        for (int i = 0; i < 6; i++)
        {
            NoteData[i] = new List<Note_old>();
        }
        
        int beatTotal = csvData[2].Length;
        
        for (int i = 2; i < 8; i++)
        {
            for (int j = 0; j < beatTotal; j++)
            {
                if (csvData[i][j] == "n") continue;
            
                NoteData[i - 2].Add(new Note_old(csvData[i][j][0], Tempo * j));
            }
        }

        return NoteData;
    }

    public static float GetTempo()
    {
        return Tempo;
    }
}
