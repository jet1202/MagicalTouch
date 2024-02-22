using UnityEngine;
using UnityEditor;
using System.Linq;
using System;


[CustomEditor(typeof(MeshRenderer))]
public class MyMeshRendererInspector : Editor
{
    Editor defaultEditor;

    private void OnEnable()
    {
        this.defaultEditor = Editor.CreateEditor(targets, Type.GetType("UnityEditor.MeshRendererEditor, UnityEditor"));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.BeginHorizontal();

        // sorting order
        var sortOrderProperty = serializedObject.FindProperty("m_SortingOrder");
        sortOrderProperty.intValue = EditorGUILayout.IntField("Sort Order", sortOrderProperty.intValue);

        // sorting layer
        var layerIDProperty = serializedObject.FindProperty("m_SortingLayerID");
        var index = System.Array.FindIndex(SortingLayer.layers, layer => layer.id == layerIDProperty.intValue);
        index = EditorGUILayout.Popup(index, (from layer in SortingLayer.layers select layer.name).ToArray());
        layerIDProperty.intValue = SortingLayer.layers[index].id;

        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();

        this.defaultEditor.OnInspectorGUI();
    }
}