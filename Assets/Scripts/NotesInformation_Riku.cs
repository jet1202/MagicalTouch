using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using NoteData;

public static class NotesInformation_Riku
{
    public static int beat = 4;
    private static TextAsset csvFile;
    private static List<string[]> csvData = new List<string[]>();
    private static int Bpm;
    private static float Tempo = 0;
    private static float MeasureTempo;

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
        MeasureTempo = Tempo * beat;
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

        float MeasureBeat;
        string Measure;
        for (int i = 2; i < 8; i++)
        {
            for (int j = 0; j < beatTotal; j++)
            {
                Measure = csvData[i][j];
                MeasureBeat = MeasureTempo / Measure.Length;

                for (int k = 0; k < Measure.Length; k++)
                {
                    if (Measure[k] == 'n') continue;
                    
                    noteData[i - 2].Add(new Note(Measure[k], MeasureTempo * j + MeasureBeat * k));
                }
            }
        }

        return noteData;
    }
    
    public static float GetTempo()
    {
        return Tempo;
    }
}
