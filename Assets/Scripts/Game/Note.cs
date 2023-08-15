using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Note
{
    private int Number { get; }
    private int Time { get; }
    private int StartLane { get; }
    private int EndLane { get; }
    private char Kind { get; }
    private int Length { get; }
    private int Field { get; }
    
    public Note(int number, int time, int startLane, int endLane, char kind, int length, int field)
    {
        this.Number = number;
        this.Time = time;
        this.StartLane = startLane;
        this.EndLane = endLane;
        this.Kind = kind;
        this.Length = length;
        this.Field = field;
    }

    public int GetNumber()
    {
        return Number;
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

    public int GetField()
    {
        return Field;
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
    public int time;
    public int startLane;
    public int endLane;
    public char kind;
    public int length;
    public int field;
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
    public int time;
    public int startLane;
    public int endLane;
    public bool isJudge;
    public bool isVariation;
}

[Serializable]
public class BpmSave
{
    public BpmItem[] bpmItem;
}

[Serializable]
public class BpmItem
{
    public int time;
    public int bpm;
}

[Serializable]
public class FieldSave
{
    public Field[] item;
}

[Serializable]
public class Field
{
    public int field;
    public SpeedItem[] speedItem;
    public AngleWork[] angleWork;
    public int[] activeTime;
}

[Serializable]
public class SpeedItem
{
    public int time;
    public int speed;
    public bool isVariation;
}

[Serializable]
public class AngleWork
{
    public int time;
    public int angle;
    public int variation;
}
