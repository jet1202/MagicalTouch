using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Note
{
    private int Time { get; }
    private int StartLane { get; }
    private int EndLane { get; }
    private char Kind { get; }
    private int Length { get; }
    
    public Note(int time, int startLane, int endLane, char kind, int length)
    {
        this.Time = time;
        this.StartLane = startLane;
        this.EndLane = endLane;
        this.Kind = kind;
        this.Length = length;
    }
    
    public int GetTime()
    {
        return Time;
    }
    
    public int GetStartLane()
    {
        return StartLane;
    }
    
    public int GetEndLane()
    {
        return EndLane;
    }

    public char GetKind()
    {
        return Kind;
    }

    public int GetLength()
    {
        return Length;
    }
}

[Serializable]
public class NoteSaveData
{
    public NoteSave[] item;
    public SlideSave[] slideItem;
}

[Serializable]
public class NoteSave
{
    public int number;
    public int time100;
    public int startLane;
    public int endLane;
    public char kind;
    public int length100;
}

[Serializable]
public class SpeedItem
{
    public int time100;
    public int speed100;
    public bool isVariation;
}

[Serializable]
public class BpmItem
{
    public int time100;
    public int bpm;
}

[Serializable]
public class NoteAddition
{
    public SpeedItem[] speedItem;
    public BpmItem[] bpmItem;
}

[Serializable]
public class SlideSave
{
    public int number;
    public SlideMaintain[] item;
}

[Serializable]
public class SlideMaintain
{
    public int time100;
    public int startLine;
    public int endLine;
    public bool isJudge;
    public bool isVariation;
}
