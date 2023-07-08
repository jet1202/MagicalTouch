using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class SettingDirector : MonoBehaviour
{
    [SerializeField] private GameObject back;
    [SerializeField] private GameObject noteSpeedText;
    [SerializeField] private GameObject isSimultaneousLine;
    [SerializeField] private GameObject isAuto;
    [SerializeField] private GameObject isLateFast;
    [SerializeField] private GameObject isColor;
    [SerializeField] private GameObject songOffset;
    [SerializeField] private GameObject tapOffset;
    [SerializeField] private GameObject musicVolume;
    [SerializeField] private GameObject seVolume;
    [SerializeField] private GameObject noteThickness;
    [SerializeField] private GameObject isFPSMode;

    private void Start()
    {
        var m = back.GetComponent<Renderer>().material;
        m.SetFloat("_Number_of_cell", 50f);
        m.SetColor("_BaseColor", new Color(150f / 255f, 255f / 255f, 255f / 255f, 1f));
        m.SetFloat("_width", 0.4f);

        var set = ScoreData.setting.Game;
        noteSpeedText.GetComponent<TextMeshProUGUI>().text = (set.NoteSpeed / 10f).ToString("F1");
        isSimultaneousLine.GetComponent<Toggle>().isOn = set.IsPushLine;
        isAuto.GetComponent<Toggle>().isOn = set.IsAuto;
        isLateFast.GetComponent<Toggle>().isOn = set.IsLateFast;
        isColor.GetComponent<Toggle>().isOn = set.IsColor;
        songOffset.GetComponent<TextMeshProUGUI>().text = set.SongOffset.ToString();
        tapOffset.GetComponent<TextMeshProUGUI>().text = set.TapOffset.ToString();
        musicVolume.GetComponent<Slider>().value = set.MusicVolume / 100f;
        seVolume.GetComponent<Slider>().value = set.SeVolume / 100f;
        noteThickness.GetComponent<TextMeshProUGUI>().text = (set.NoteThickness / 10f).ToString("F1");
        isFPSMode.GetComponent<Toggle>().isOn = set.FPSMode;
    }
}
