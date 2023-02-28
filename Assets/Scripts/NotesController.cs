using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotesController : MonoBehaviour
{
    [SerializeField] private GameDirector gameDirector;
    [SerializeField] public float Speed;
    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
    }

    void Update()
    {
        transform.position = new Vector3(0, 0, -gameDirector.musicTime * Speed);
    }
}
