using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingDirector : MonoBehaviour
{
    [SerializeField] private GameObject back;

    private void Start()
    {
        var m = back.GetComponent<Renderer>().material;
        m.SetFloat("_Number_of_cell", 50f);
        m.SetColor("_BaseColor", new Color(150f / 255f, 255f / 255f, 255f / 255f, 1f));
        m.SetFloat("_width", 0.4f);
    }
}
