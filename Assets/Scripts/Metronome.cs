using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Metronome : MonoBehaviour
{
    [SerializeField] private NotesDirector _notesDirector;
    [SerializeField] private AudioClip SE;
    private AudioSource _audio;
    private float Tempo;
    private int multiple = 1;
    private bool isRiku;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
        isRiku = _notesDirector.isRikuMethod;
    }

    void Update()
    {
        if (Tempo == 0)
        {
            if (isRiku) Tempo = NotesInformation_Riku.GetTempo();
            else Tempo = NotesInformation.GetTempo();
        }
        else
        {
            if (Time.time >= Tempo * multiple - 0.1f)
            {
                _audio.Stop();
                _audio.PlayOneShot(SE);
                multiple++;
            }
        }
    }
}
