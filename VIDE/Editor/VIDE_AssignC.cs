/*
 * Copyright (c) 2016 Christian Henderson
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(VIDE_Assign))]
public class VIDE_AssignC : Editor
{
    /*
     * Custom Inspector for the VIDE_Assign component
     */
    VIDE_Assign d;

    private void openVIDE_Editor(int idx)
    {
        if (!Directory.Exists(Application.dataPath + "/" + VIDE_Editor.pathToVide + "VIDE"))
        {
            Debug.LogError("Cannot find VIDE folder! If you moved the VIDE folder from the root, make sure you set the 'pathToVide' variable in VIDE_Editor.cs");
            return;
        }

        VIDE_Editor editor = EditorWindow.GetWindow<VIDE_Editor>();
        editor.Init(idx);
    }

    public override void OnInspectorGUI()
    {

        d = (VIDE_Assign)target;
		Color defColor = GUI.color;
		GUI.color = Color.yellow;

		//Create a button to open up the VIDE Editor and load the currently assigned dialogue
        if (GUILayout.Button("Open VIDE Editor"))
            openVIDE_Editor(d.assignedIndex);
		
		GUI.color = defColor;

        //Refresh dialogue list
        if (Event.current.type == EventType.MouseDown)
        {
            if (d != null)
                loadFiles();
        }
        GUILayout.BeginHorizontal();

        GUILayout.Label("Dialogue name: ");
        d.dialogueName = EditorGUILayout.TextField(d.dialogueName);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Assigned dialogue:");
        if (d.diags.Count > 0)
        {
            EditorGUI.BeginChangeCheck();
            d.assignedIndex = EditorGUILayout.Popup(d.assignedIndex, d.diags.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                if (d.assignedDialogue != d.diags[d.assignedIndex])
                {
                    d.assignedDialogue = d.diags[d.assignedIndex];
                }
            }
        }
        else
        {
            GUILayout.Label("No saved Dialogues!");

        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Interaction Count: " + d.interactionCount.ToString());
		
        GUILayout.BeginHorizontal();
        GUILayout.Label("Override Start Node: ");
		d.overrideStartNode = EditorGUILayout.IntField(d.overrideStartNode);
        GUILayout.EndHorizontal();
		
    }

        //Refresh dialogue list
    public void OnProjectChange()
    {
        if (d != null)
            loadFiles();
    }

        //Refresh dialogue list
    public void OnFocus()
    {
        if (d != null)
            loadFiles();
    }
	
	void OnEnable(){
		loadFiles ();
	}

        //Refresh dialogue list
    public void loadFiles()
    {
        d = (VIDE_Assign)target;
		
        TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
        d.diags = new List<string>();


        if (files.Length < 1) return;

        foreach (TextAsset f in files)
        {
            d.diags.Add(f.name);
        }

        if (d.assignedIndex >= d.diags.Count)
            d.assignedIndex = 0;

        d.assignedDialogue = d.diags[d.assignedIndex];
        Repaint();

    }
}
