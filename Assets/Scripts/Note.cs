using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Note
{
    private float Time { get; }
    private int StartLane { get; }
    private int EndLane { get; }
    private char Kind { get; }
    private float Length { get; }
    
    public Note(float time, int startLane, int endLane, char kind, float length)
    {
        this.Time = time;
        this.StartLane = startLane;
        this.EndLane = endLane;
        this.Kind = kind;
        this.Length = length;
    }
    
    public float GetTime()
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

    public float GetLength()
    {
        return Length;
    }
}

public class Base
{
    public string Name;
    public string Url;
    public int Bpm;
    public float Offset;
}
