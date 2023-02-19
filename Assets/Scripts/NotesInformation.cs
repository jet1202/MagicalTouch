using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using NoteData;

namespace NoteData
{
    public class Note
    {
        public GameObject noteObject;
        public char Type { get; }
        public float JustTime { get; }

        public Note(char type, float justTime)
        {
            noteObject = null;
            Type = type;
            JustTime = justTime;
        }

        public void setObj(GameObject obj)
        {
            this.noteObject = obj;
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

    public static List<Note>[] InitNoteData(string songName)
    {
        CsvReader(songName);

        List<Note>[] noteData = new List<Note>[6];
        for (int i = 0; i < 6; i++)
        {
            noteData[i] = new List<Note>();
        }
        
        int beatTotal = csvData[2].Length;
        
        for (int i = 2; i < 8; i++)
        {
            for (int j = 0; j < beatTotal; j++)
            {
                if (csvData[i][j] == "n") continue;
            
                noteData[i - 2].Add(new Note(csvData[i][j][0], Tempo * j));
            }
        }

        return noteData;
    }

    public static float GetTempo()
    {
        return Tempo;
    }
}
