using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CategoryData
{
    public string[] tabs;
    public Category[] item;
}

[Serializable]
public class Category
{
    public string name;
    public string sub;
    public string division;
    public int tab;
    public int document;
}
