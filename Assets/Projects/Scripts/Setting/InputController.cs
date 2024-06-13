using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SaveData;

public class InputController : MonoBehaviour
{
    [SerializeField] private SettingDirector settingDirector;
    
    [SerializeField] private TextMeshProUGUI noteSpeedText;
    [SerializeField] private TextMeshProUGUI songOffsetText;
    [SerializeField] private TextMeshProUGUI tapOffsetText;
    [SerializeField] private TextMeshProUGUI noteThicknessText;
    
    public void NoteSpeedLeft()
    {
        setting.Game.NoteSpeed = Math.Clamp(setting.Game.NoteSpeed - 1, 10, 110);
        noteSpeedText.text = (setting.Game.NoteSpeed / 10f).ToString("F1");
    }
    
    public void NoteSpeedRight()
    {
        setting.Game.NoteSpeed = Math.Clamp(setting.Game.NoteSpeed + 1, 10, 110);
        noteSpeedText.text = (setting.Game.NoteSpeed / 10f).ToString("F1");
    }
    
    public void SimultaneousLine(bool isOn)
    {
        setting.Game.IsPushLine = isOn;
    }
    
    public void AutoMode(bool isOn)
    {
        setting.Game.IsAuto = isOn;
    }
    
    public void LateFast(bool isOn)
    {
        setting.Game.IsLateFast = isOn;
    }
    
    public void LineColor(bool isOn)
    {
        setting.Game.IsColor = isOn;
    }
    
    public void MusicOffsetLeft()
    {
        setting.Game.SongOffset = Math.Clamp(setting.Game.SongOffset - 5, -300, 300);
        songOffsetText.text = (setting.Game.SongOffset / 10f).ToString("F1");
    }
    
    public void MusicOffsetRight()
    {
        setting.Game.SongOffset = Math.Clamp(setting.Game.SongOffset + 5, -300, 300);
        songOffsetText.text = (setting.Game.SongOffset / 10f).ToString("F1");
    }
    
    public void TapOffsetLeft()
    {
        setting.Game.TapOffset = Math.Clamp(setting.Game.TapOffset - 5, -300, 300);
        tapOffsetText.text = (setting.Game.TapOffset / 10f).ToString("F1");
    }
    
    public void TapOffsetRight()
    {
        setting.Game.TapOffset = Math.Clamp(setting.Game.TapOffset + 5, -300, 300);
        tapOffsetText.text = (setting.Game.TapOffset / 10f).ToString("F1");
    }

    public void MusicVolume(float value)
    {
        setting.Game.MusicVolume = (int)(value * 100);
    }

    public void SeVolume(float value)
    {
        setting.Game.SeVolume = (int)(value * 100);
    }
    
    public void NoteThicknessLeft()
    {
        setting.Game.NoteThickness = Math.Clamp(setting.Game.NoteThickness - 1, 1, 10);
        noteThicknessText.text = (setting.Game.NoteThickness / 10f).ToString("F1");
    }
    
    public void NoteThicknessRight()
    {
        setting.Game.NoteThickness = Math.Clamp(setting.Game.NoteThickness + 1, 1, 10);
        noteThicknessText.text = (setting.Game.NoteThickness / 10f).ToString("F1");
    }

    public void FPSMode(bool isOn)
    {
        setting.Game.FPSMode = isOn;
    }

    public void BackButton()
    {
        settingDirector.Back();
    }
}
