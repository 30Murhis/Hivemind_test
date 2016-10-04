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
using System.IO;

public class VIDE_Assign : MonoBehaviour
{
    /*
     * This script component should be attached to every game object you will be interacting with.
     * When interacting with the NPC or object, you will have to call the BeginDialogue() method on
     * the DialogueData component and pass this script.
     * It will safely load the assigned script and keep track of the amount of times you've interacted with it.
     * It will also allow you to set a start point override and to modify the assigned dialogue.
     */

    public List<string> diags = new List<string>();
    public int assignedIndex = 0;
    public string assignedDialogue = "";

    public int interactionCount = 0;
    public string dialogueName = "";

    public int overrideStartNode = -1;

    /// <summary>
    /// Returns the name of the currently assigned dialogue.
    /// </summary>
    /// <returns></returns>
    public string GetAssigned()
    {
        return diags[assignedIndex];
    }

    /// <summary>
    /// Assigns a new dialogue to these component.
    /// </summary>
    /// <param name="Dialogue name"></param>
    /// <returns></returns>
    public bool AssignNew(string newFile)
    {
        loadFiles();

        if (!diags.Contains(newFile))
        {
            Debug.LogError("Dialogue not found! Make sure the name is correct and has no extension");
            return false;
        }

        assignedIndex = diags.IndexOf(newFile);
        assignedDialogue = diags[assignedIndex];

        return true;
    }

    private void loadFiles()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>("Dialogues");
        diags = new List<string>();
        assignedIndex = 0;

        if (files.Length < 1) return;

        foreach (TextAsset f in files)
        {
            diags.Add(f.name);
        }

    }

}
