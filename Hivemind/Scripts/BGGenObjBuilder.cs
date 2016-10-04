using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(BackgroundGenerator))]
public class BGGenObjBuilder : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BackgroundGenerator bgGenScript = (BackgroundGenerator)target;
        if (GUILayout.Button("Generate Background"))
        {
            bgGenScript.GenerateBackground();
        }
        if (GUILayout.Button("Delete Background"))
        {
            bgGenScript.RemoveBackground();
        }
    }
}