using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static ScoreData;

public class InputController : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private GameObject mask;
    
    [SerializeField] private TextMeshProUGUI noteSpeedText;
    [SerializeField] private TextMeshProUGUI songOffsetText;
    [SerializeField] private TextMeshProUGUI tapOffsetText;
    [SerializeField] private TextMeshProUGUI noteThicknessText;
    
    public void NoteSpeedLeft()
    {
        setting.Game.NoteSpeed = Math.Clamp(setting.Game.NoteSpeed - 1, 20, 100);
        noteSpeedText.text = (setting.Game.NoteSpeed / 10f).ToString("F1");
        
        gameController.ChangePosition();
    }
    
    public void NoteSpeedRight()
    {
        setting.Game.NoteSpeed = Math.Clamp(setting.Game.NoteSpeed + 1, 30, 100);
        noteSpeedText.text = (setting.Game.NoteSpeed / 10f).ToString("F1");
        
        gameController.ChangePosition();
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
        
        gameController.ChangeMusicOffset();
    }
    
    public void MusicOffsetRight()
    {
        setting.Game.SongOffset = Math.Clamp(setting.Game.SongOffset + 5, -300, 300);
        songOffsetText.text = (setting.Game.SongOffset / 10f).ToString("F1");
        
        gameController.ChangeMusicOffset();
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
        
        gameController.ChangeThickness();
    }
    
    public void NoteThicknessRight()
    {
        setting.Game.NoteThickness = Math.Clamp(setting.Game.NoteThickness + 1, 1, 10);
        noteThicknessText.text = (setting.Game.NoteThickness / 10f).ToString("F1");
        
        gameController.ChangeThickness();
    }

    public void FPSMode(bool isOn)
    {
        setting.Game.FPSMode = isOn;
    }

    public void BackButton()
    {
        mask.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        mask.SetActive(true);
        mask.GetComponent<Image>().DOFade(1f, 0.7f)
            .OnComplete(() => { SceneManager.LoadScene(SettingData.fromScene); });
    }
}
