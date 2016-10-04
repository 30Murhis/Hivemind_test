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
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class VIDE_Data : MonoBehaviour
{
    /*
     * This component is the source of all data you'll be needing to populate your dialogue interface.
     * It will manage the flow of the conversation based on the current node's data stored in a variable called nodeData.
     * All you need is to attach this component to the game object that will manage your dialogue UI.
     * Then call BeginDialogue() on it to begin the conversation with an NPC. 
     * The rest is up to the Next() method to advance in the conversation up until you call EndDialogue()
     */
    public List<string> diags = new List<string>();
    public int assignedIndex = 0;

    private List<CommentSet> playerDiags = new List<CommentSet>();
    private List<Answer> npcDiags = new List<Answer>();
    private Answer currentNPCStep;
    private CommentSet currentPlayerStep;
    private bool jumped = false;
    private int startPoint;
    public VIDE_Assign assigned;
    public bool isLoaded;
    public NodeData nodeData;


    //The class that contains all of the node variables
    public class NodeData
    {
        public bool currentIsPlayer;
        public bool isEnd;
        public int nodeID;
        public string[] playerComments;
        public string[] npcComment;
        public int npcCommentIndex;
        public int selectedOption;
        public string extraData;
        public string tag;

        public NodeData(bool isP, bool isE, int id, string[] pt, string[] npt, string exData, string tagt)
        {
            currentIsPlayer = isP;
            isEnd = isE;
            nodeID = id;
            playerComments = pt;
            npcComment = npt;
            npcCommentIndex = 0;
            selectedOption = 0;
            extraData = exData;
            tag = tagt;
        }

    }

    /// <summary>
    /// Ignores current nodeData state and jumps directly to the specified Node, be it Player or NPC Node. 
    /// </summary>
    /// <returns>
    /// The node.
    /// </returns>
    /// <param name='id'>
    /// The ID of your Node. Get it from the Dialogue Editor.
    /// </param>
    public NodeData SetNode(int id)
    {
        //Look for Node with given ID
        bool foundID = false;
        bool isPl = false;
        for (int i = 0; i < playerDiags.Count; i++)
        {
            if (playerDiags[i].ID == id)
            {
                currentPlayerStep = playerDiags[i];
                isPl = true;
                foundID = true;
            }
        }
        if (!foundID)
        {
            for (int i = 0; i < npcDiags.Count; i++)
            {
                if (npcDiags[i].ID == id)
                {
                    currentNPCStep = npcDiags[i];
                    foundID = true;
                }
            }
        }
        if (!foundID)
        {
            Debug.LogError("Could not find a Node with ID " + id.ToString());
            return null;
        }

        if (isPl)
        {
            nodeData = new NodeData(true, false, currentPlayerStep.ID, GetOptions(), null, null, null);
            jumped = true;
            return nodeData;
        }
        else
        {
            List<string> ns = new List<string>();

            string[] rawSplit = Regex.Split(currentNPCStep.text, "<br>");
            foreach (string s in rawSplit)
            {
                if (s != "" && s != " ") ns.Add(s.Trim());
            }

            nodeData = new NodeData(isPl, false, id, null, ns.ToArray(), currentNPCStep.extraData, currentNPCStep.tag);
            return nodeData;
        }
    }
    /// <summary>
    /// Populates nodeData with the data from next Node based on the current nodeData.
    /// </summary>
    /// <returns></returns>
    public NodeData Next()
    {

        int option = nodeData.selectedOption;

        if (!isLoaded)
        {
            Debug.LogError("You must call the 'BeginDialogue()' method before calling the 'Next()' method!");
            return null;
        }

        if (!jumped)
        {
            if (!nodeData.currentIsPlayer)
                if (currentNPCStep.endConversation && nodeData.npcCommentIndex == nodeData.npcComment.Length - 1)
                {
                    nodeData.isEnd = true;
                    return nodeData;
                }
        }

        jumped = false;


        bool nextIsPlayer = true;

        if (!nodeData.currentIsPlayer)
        {
            nextIsPlayer = true;
            if (currentNPCStep.outputSet == null)
            {
                nextIsPlayer = false;
            }
        }
        else
        {
            nextIsPlayer = false;
        }

        if (!nodeData.currentIsPlayer)
        {

            if (nodeData.npcCommentIndex != nodeData.npcComment.Length - 1)
            {
                nodeData.npcCommentIndex++;
                return nodeData;
            }

            if (nextIsPlayer)
            {
                currentPlayerStep = currentNPCStep.outputSet;
                nodeData = new NodeData(true, false, currentPlayerStep.ID, GetOptions(), null, null, null);
            }
            else
            {
                currentNPCStep = currentNPCStep.outputNPC;
                List<string> ns = new List<string>();

                string[] rawSplit = Regex.Split(currentNPCStep.text, "<br>");
                foreach (string s in rawSplit)
                { if (s != "" && s != " ") ns.Add(s.Trim()); }

                nodeData = new NodeData(false, false, currentNPCStep.ID, null, ns.ToArray(), currentNPCStep.extraData, currentNPCStep.tag);
            }

            return nodeData;
        }
        else
        {
            //Pick option 0 as default as we passed no params
            currentNPCStep = currentPlayerStep.comment[option].outputAnswer;

            List<string> ns = new List<string>();

            string[] rawSplit = Regex.Split(currentNPCStep.text, "<br>");
            foreach (string s in rawSplit)
            { if (s != "" && s != " ") ns.Add(s.Trim()); }

            nodeData = new NodeData(false, false, currentNPCStep.ID, null, ns.ToArray(), currentNPCStep.extraData, currentNPCStep.tag);
            return nodeData;
        }
    }

    /// <summary>
    /// Loads up the dialogue just sent. Populates the nodeData variable with the first Node based on the Start Node. Also returns the current NodeData package.
    /// </summary>
    /// <param name="diagToLoad"></param>
    /// <returns>NodeData</returns>
    public NodeData BeginDialogue(VIDE_Assign diagToLoad)
    {
        //First we load the dialogue we requested
        if (Load(diagToLoad.diags[diagToLoad.assignedIndex]))
        {
            isLoaded = true;
        }
        else
        {
            isLoaded = false;
            Debug.LogError("Failed to load '" + diagToLoad.diags[diagToLoad.assignedIndex] + "'");
            return null;
        }

        //Make sure that variables were correctly reset after last conversation
        if (nodeData != null)
        {
            Debug.LogError("You forgot to call 'EndDialogue()' on last conversation!");
            return null;
        }

        assigned = diagToLoad;

        if (assigned.overrideStartNode != -1)
            startPoint = assigned.overrideStartNode;

        int startIndex = -1;
        bool isPlayer = false;

        for (int i = 0; i < npcDiags.Count; i++)
            if (startPoint == npcDiags[i].ID) { startIndex = i; isPlayer = false; break; }
        for (int i = 0; i < playerDiags.Count; i++)
            if (startPoint == playerDiags[i].ID) { startIndex = i; isPlayer = true; break; }

        if (startIndex == -1)
        {
            Debug.LogError("Start point not found! Check your IDs!");
            return null;
        }

        if (isPlayer)
        {
            currentPlayerStep = playerDiags[startIndex];

            nodeData = new NodeData(true, false, currentPlayerStep.ID, GetOptions(), null, null, null);
            return nodeData;

        }
        else
        {
            currentNPCStep = npcDiags[startIndex];

            List<string> ns = new List<string>();

            string[] rawSplit = Regex.Split(currentNPCStep.text, "<br>");
            foreach (string s in rawSplit)
            { if (s != "" && s != " ") ns.Add(s.Trim()); }


            nodeData = new NodeData(false, false, currentNPCStep.ID, null, ns.ToArray(), currentNPCStep.extraData, currentNPCStep.tag);

            return nodeData;
        }


    }
    /// <summary>
    /// Wipes out all data and unloads the current DialogueAssign, raising its interactionCount.
    /// </summary>
    public void EndDialogue()
    {
        nodeData = null;
        assigned.interactionCount++;
        assigned = null;
        playerDiags = new List<CommentSet>(); ;
        npcDiags = new List<Answer>(); ;
        isLoaded = false;
        currentNPCStep = null;
        currentPlayerStep = null;
    }

    private string[] GetOptions()
    {
        List<string> op = new List<string>();

        if (currentPlayerStep == null)
        {
            return op.ToArray();
        }

        for (int i = 0; i < currentPlayerStep.comment.Count; i++)
        {
            op.Add(currentPlayerStep.comment[i].text);
        }

        return op.ToArray();
    }



    //The following are all of the classes and methods we need for constructing the nodes

    class SerializeHelper
    {
        //static string fileDataPath = Application.dataPath + "/VIDE/dialogues/";
        public static object ReadFromFile(string filename)
        {
            string jsonString = Resources.Load<TextAsset>("Dialogues/" + filename).text;
            return MiniJSON_VIDE.DiagJson.Deserialize(jsonString);
        }
    }

    class CommentSet
    {
        public List<Comment> comment;
        public int ID;

        public CommentSet(int comSize, int id)
        {
            comment = new List<Comment>();
            ID = id;
            for (int i = 0; i < comSize; i++)
                comment.Add(new Comment());
        }
    }

    class Comment
    {
        public string text;
        public CommentSet inputSet;
        public Answer outputAnswer;

        public Comment()
        {
            text = "";
        }
        public Comment(CommentSet id)
        {
            outputAnswer = null;
            inputSet = id;
            text = "Comment...";
        }
    }

    class Answer
    {
        public string text;
        public CommentSet outputSet;
        public Answer outputNPC;

        public bool endConversation;
        public string extraData;
        public string tag;

        public int ID;

        public Answer(bool endC, string t, int id, string exD, string tagt)
        {
            text = t;
            outputSet = null;
            outputNPC = null;
            endConversation = endC;
            extraData = exD;
            tag = tagt;
            ID = id;
        }

    }

    void addComment(CommentSet id)
    {
        id.comment.Add(new Comment(id));
    }

    void addAnswer(bool endC, string t, int id, string exD, string tagt)
    {
        npcDiags.Add(new Answer(endC, t, id, exD, tagt));
    }

    void addSet(int cSize, int id)
    {
        playerDiags.Add(new CommentSet(cSize, id));
    }

    //This method will load the dialogue from the DialogueAssign component sent.
    bool Load(string dName)
    {
        playerDiags = new List<CommentSet>();
        npcDiags = new List<Answer>();

        if (Resources.Load("Dialogues/" + dName) == null) return false;

        Dictionary<string, object> dict = SerializeHelper.ReadFromFile(dName) as Dictionary<string, object>;

        int pDiags = (int)((long)dict["playerDiags"]);
        int nDiags = (int)((long)dict["npcDiags"]);
        startPoint = (int)((long)dict["startPoint"]);


        //Create first...
        for (int i = 0; i < pDiags; i++)
        {
            addSet(
                (int)((long)dict["pd_comSize_" + i.ToString()]),
                (int)((long)dict["pd_ID_" + i.ToString()])
                );
        }

        for (int i = 0; i < nDiags; i++)
        {
            string tagt = "";

            if (dict.ContainsKey("nd_tag_" + i.ToString()))
                tagt = (string)dict["nd_tag_" + i.ToString()];

            addAnswer(
                (bool)dict["nd_endc_" + i.ToString()],
                (string)dict["nd_text_" + i.ToString()],
                (int)((long)dict["nd_ID_" + i.ToString()]),
                (string)dict["nd_extraData_" + i.ToString()],
                tagt
                );
        }

        //Connect now...
        for (int i = 0; i < playerDiags.Count; i++)
        {
            for (int ii = 0; ii < playerDiags[i].comment.Count; ii++)
            {
                playerDiags[i].comment[ii].text = (string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "text"];
                int index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "iSet"]);
                if (index != -1)
                    playerDiags[i].comment[ii].inputSet = playerDiags[index];
                index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "oAns"]);
                if (index != -1)
                    playerDiags[i].comment[ii].outputAnswer = npcDiags[index];
            }
        }

        for (int i = 0; i < npcDiags.Count; i++)
        {
            int index = (int)((long)dict["nd_oSet_" + i.ToString()]);
            if (index != -1)
                npcDiags[i].outputSet = playerDiags[index];

            if (dict.ContainsKey("nd_oNPC_" + i.ToString()))
            {
                int index2 = (int)((long)dict["nd_oNPC_" + i.ToString()]);
                if (index2 != -1)
                    npcDiags[i].outputNPC = npcDiags[index2];
            }

        }

        return true;

    }

    //Return the default start node
    public int startNode
    {
        get
        {
            return startPoint;
        }
    }



}



