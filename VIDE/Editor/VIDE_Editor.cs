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
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using MiniJSON_VIDE;


public class VIDE_Editor : EditorWindow
{

    //This script will draw the VIDE Editor window and all of its content
    //It comunicates with VIDE_EditorDB to store the data


    //IMPORTANT! If you move the VIDE folder from the root of your project, make sure you set this variable to the updated path
    //For example, if you have a path like this: 'Assets/myFolder/myPlugins/VIDE'
    //Then set pathToVide to 'myFolder/myPlugins/'
    public const string pathToVide = ""; 

    VIDE_EditorDB db; //This is the connection to VIDE_EditorDB, all variables and classes are temporarily stored there
    GameObject dbObj;
    Color defaultColor;
    Color32[] colors;

    VIDE_EditorDB.Comment draggedCom;
    VIDE_EditorDB.Answer draggedAns;

    Vector2 dragStart;
    Rect fWin = new Rect();
    Rect startDiag;
    int startID = 0;
    int curFocusID = 0;
    int currentDiag = 0;
    int fileIndex = 0;
    int areYouSureIndex = 0;
    int focusedPlayerText = 0;

    bool draggingLine = false;
    bool dragnWindows = false;
    bool repaintLines = false;
    bool autosaveON = true;
    bool editEnabled = true;
    bool newFile = false;
    bool overwritePopup = false;
    bool deletePopup = false;
    bool needSave = false;
    bool npcReady = false;
    bool playerReady = false;
    bool areYouSure = false;
    bool showError = false;
    bool hasID = false;

    string newFileName = "My Dialogue";
    string errorMsg = "";
    string lastTextFocus;

    List<string> saveNames = new List<string>() { };

    //Add VIDE Editor to Window...
    [MenuItem("Window/VIDE Editor")]
    static void ShowEditor()
    {
        if (!Directory.Exists(Application.dataPath + "/" + pathToVide + "VIDE"))
        {
            Debug.LogError("Cannot find VIDE folder at '" + Application.dataPath + "/" + pathToVide + "VIDE" + "'! If you moved the VIDE folder from the root, make sure you set the 'pathToVide' variable in VIDE_Editor.cs");
            return;
        }

        VIDE_Editor editor = EditorWindow.GetWindow<VIDE_Editor>();
        editor.Init(0);
    }

    //Save progress if autosave is on
    void OnLostFocus()
    {
        if (npcReady && playerReady)
            Save();
    }

    //For safety reasons, let's re-link and repaint
    void OnFocus()
    {
        dbObj = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/" + pathToVide + "VIDE/Editor/db.prefab", typeof(GameObject));
        db = dbObj.GetComponent<VIDE_EditorDB>();
        Repaint();
    }

    //Set all start variables
    public void Init(int idx)
    {
        #if UNITY_5_0
        EditorWindow.GetWindow<VIDE_Editor>().title = "VIDE Editor";
        #else
        Texture2D icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/" + pathToVide + "VIDE/Data/assignIcon.png", typeof(Texture2D));
        GUIContent titleContent = new GUIContent(" VIDE Editor", icon);
        EditorWindow.GetWindow<VIDE_Editor>().titleContent = titleContent;
        #endif

        dbObj = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/" + pathToVide + "VIDE/Editor/db.prefab", typeof(GameObject));
        db = dbObj.GetComponent<VIDE_EditorDB>();
        startDiag = new Rect(20, 50, 300, 50);
        fileIndex = idx;
        loadFiles(idx);
        Load(true);
    }

    public class SerializeHelper
    {
        static string fileDataPath = Application.dataPath + "/" + pathToVide + "VIDE/Resources/dialogues/";
        public static void WriteToFile(object data, string filename)
        {
            string outString = DiagJson.Serialize(data);
            File.WriteAllText(fileDataPath + filename, outString);
        }
        public static object ReadFromFile(string filename)
        {
            string jsonString = File.ReadAllText(fileDataPath + filename);
            return DiagJson.Deserialize(jsonString);
        }
    }

    //Methods that manage node creation and deletion 
    public void addComment(VIDE_EditorDB.CommentSet id)
    {
        id.comment.Add(new VIDE_EditorDB.Comment(id));
    }

    public void addAnswer(Vector2 rPos, bool endC, string t, int id, string exD, string tagt)
    {
        db.npcDiags.Add(new VIDE_EditorDB.Answer(rPos, endC, t, id, exD, tagt));
    }

    public void addSet(Vector2 rPos, int cSize, int id)
    {
        db.playerDiags.Add(new VIDE_EditorDB.CommentSet(rPos, cSize, id));
    }

    public void removeSet(VIDE_EditorDB.CommentSet id)
    {
        db.playerDiags.Remove(id);
        for (int i = 0; i < db.npcDiags.Count; i++)
        {
            if (db.npcDiags[i].outputSet == id)
            {
                db.npcDiags[i].outputSet = null;
            }
        }
    }

    public void removeComment(VIDE_EditorDB.Comment idx)
    {
        Undo.RecordObject(db, "Removed Comment");
        idx.inputSet.comment.Remove(idx);
    }

    public void removeAnswer(VIDE_EditorDB.Answer id)
    {
        db.npcDiags.Remove(id);

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
            {
                if (db.playerDiags[i].comment[ii].outputAnswer == id)
                {
                    db.playerDiags[i].comment[ii].outputAnswer = null;
                }
            }
        }

        for (int i = 0; i < db.npcDiags.Count; i++)
        {
            if (db.npcDiags[i].outputNPC == id)
            {
                db.npcDiags[i].outputNPC = null;
            }
        }
    }

    //This will break the node connections
    public void breakConnection(int type, VIDE_EditorDB.Comment commID, VIDE_EditorDB.Answer ansID)
    {
        //Type 0 = VIDE_EditorDB.Comment -> VIDE_EditorDB.Answer
        //Type 1 = VIDE_EditorDB.Answer -> Set	

        if (type == 0)
        {
            Undo.RecordObject(db, "Broke connection");
            commID.outputAnswer = null;
        }
        if (type == 1)
        {
            Undo.RecordObject(db, "Broke connection");
            if (ansID.outputNPC == null)
            {
                Undo.RecordObject(db, "Broke connection");
                ansID.outputSet = null;
            }
            else
            {
                Undo.RecordObject(db, "Broke connection");
                ansID.outputNPC = null;
            }

        }

    }

    //Connect player node to NPC node
    //Create node if released on empty space
    public void TryConnectToAnswer(Vector2 mPos, VIDE_EditorDB.Comment commID)
    {
        if (commID == null) return;

        for (int i = 0; i < db.npcDiags.Count; i++)
        {
            if (db.npcDiags[i].rect.Contains(mPos))
            {
                Undo.RecordObject(db, "Connected Node");
                commID.outputAnswer = db.npcDiags[i];
                Repaint();
                return;
            }
        }
        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            if (db.playerDiags[i].rect.Contains(mPos))
            {
                return;
            }
        }
        int id = setUniqueID();
        Undo.RecordObject(db, "Added Node");
        db.npcDiags.Add(new VIDE_EditorDB.Answer(new Rect(mPos.x - 150, mPos.y - 200, 0, 0), id));
        commID.outputAnswer = db.npcDiags[db.npcDiags.Count - 1];
        repaintLines = true;
        Repaint();
        GUIUtility.hotControl = 0;
    }

    //Connect NPC node to Player/NPC node
    //Create node if released on empty space
    public void TryConnectToSet(Vector2 mPos, VIDE_EditorDB.Answer ansID)
    {
        if (ansID == null) return;

        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            if (db.playerDiags[i].rect.Contains(mPos))
            {
                Undo.RecordObject(db, "Connected Node");
                ansID.outputSet = db.playerDiags[i];
                Repaint();
                return;
            }
        }
        for (int i = 0; i < db.npcDiags.Count; i++)
        {
            if (db.npcDiags[i].rect.Contains(mPos))
            {
                if (db.npcDiags[i] == ansID) { return; }

                Undo.RecordObject(db, "Connected Node");
                ansID.outputNPC = db.npcDiags[i];
                Repaint();
                return;
            }
        }
        int id = setUniqueID();
        Undo.RecordObject(db, "Added Node");
        db.playerDiags.Add(new VIDE_EditorDB.CommentSet(new Rect(mPos.x - 150, mPos.y - 200, 0, 0), id));
        ansID.outputSet = db.playerDiags[db.playerDiags.Count - 1];
        repaintLines = true;
        Repaint();
        GUIUtility.hotControl = 0;
    }

    //Sets a unique ID for the node
    public int setUniqueID()
    {
        int tempID = 0;
        while (searchIDs(tempID) == false)
        {
            tempID++;
        }
        return tempID;
    }

    //Searches for a unique ID
    public bool searchIDs(int id)
    {
        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            if (db.playerDiags[i].ID == id) return false;
        }
        for (int i = 0; i < db.npcDiags.Count; i++)
        {
            if (db.npcDiags[i].ID == id) return false;
        }
        return true;
    }

    //This will save the current data base status
    public void Save()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("npcDiags", db.npcDiags.Count);
        dict.Add("playerDiags", db.playerDiags.Count);
        dict.Add("startPoint", startID);
        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            dict.Add("pd_rect_" + i.ToString(), new int[] { (int)db.playerDiags[i].rect.x, (int)db.playerDiags[i].rect.y });
            dict.Add("pd_comSize_" + i.ToString(), db.playerDiags[i].comment.Count);
            dict.Add("pd_ID_" + i.ToString(), db.playerDiags[i].ID);
            for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
            {
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "iSet", db.playerDiags.FindIndex(idx => idx == db.playerDiags[i].comment[ii].inputSet));
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "oAns", db.npcDiags.FindIndex(idx => idx == db.playerDiags[i].comment[ii].outputAnswer));
                dict.Add("pd_" + i.ToString() + "_com_" + ii.ToString() + "text", db.playerDiags[i].comment[ii].text);
            }
        }
        for (int i = 0; i < db.npcDiags.Count; i++)
        {
            dict.Add("nd_rect_" + i.ToString(), new int[] { (int)db.npcDiags[i].rect.x, (int)db.npcDiags[i].rect.y });
            dict.Add("nd_endc_" + i.ToString(), db.npcDiags[i].endConversation);
            dict.Add("nd_extraData_" + i.ToString(), db.npcDiags[i].extraData);
            dict.Add("nd_tag_" + i.ToString(), db.npcDiags[i].tag);
            dict.Add("nd_text_" + i.ToString(), db.npcDiags[i].text);
            dict.Add("nd_ID_" + i.ToString(), db.npcDiags[i].ID);
            dict.Add("nd_oSet_" + i.ToString(), db.playerDiags.FindIndex(idx => idx == db.npcDiags[i].outputSet));
            dict.Add("nd_oNPC_" + i.ToString(), db.npcDiags.FindIndex(idx => idx == db.npcDiags[i].outputNPC));
        }
        SerializeHelper.WriteToFile(dict as Dictionary<string, object>, saveNames[currentDiag] + ".json");
    }

    //Loads from dialogues
    public void Load(bool clear)
    {
        if (clear)
        {
            db.playerDiags = new List<VIDE_EditorDB.CommentSet>();
            db.npcDiags = new List<VIDE_EditorDB.Answer>();
        }

        if (saveNames.Count < 1)
            return;

        if (!File.Exists(Application.dataPath + "/" + pathToVide + "VIDE/Resources/dialogues/" + saveNames[currentDiag] + ".json"))
        {
            return;
        }

        Dictionary<string, object> dict = SerializeHelper.ReadFromFile(saveNames[currentDiag] + ".json") as Dictionary<string, object>;
        int pDiags = (int)((long)dict["playerDiags"]);
        int nDiags = (int)((long)dict["npcDiags"]);
        startID = (int)((long)dict["startPoint"]);
        //Create first...
        for (int i = 0; i < pDiags; i++)
        {
            string k = "pd_rect_" + i.ToString();
            List<object> rect = (List<object>)(dict[k]);
            addSet(new Vector2((float)((long)rect[0]), (float)((long)rect[1])),
                (int)((long)dict["pd_comSize_" + i.ToString()]),
                (int)((long)dict["pd_ID_" + i.ToString()])
                );
        }
        for (int i = 0; i < nDiags; i++)
        {
            string k = "nd_rect_" + i.ToString();
            List<object> rect = (List<object>)(dict[k]);

            string tagt = "";

            if (dict.ContainsKey("nd_tag_" + i.ToString()))
                tagt = (string)dict["nd_tag_" + i.ToString()];

            addAnswer(new Vector2((float)((long)rect[0]), (float)((long)rect[1])),
                (bool)dict["nd_endc_" + i.ToString()],
                (string)dict["nd_text_" + i.ToString()],
                (int)((long)dict["nd_ID_" + i.ToString()]),
                (string)dict["nd_extraData_" + i.ToString()],
                tagt
                );
        }
        //Connect now...
        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
            {
                db.playerDiags[i].comment[ii].text = (string)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "text"];
                int index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "iSet"]);
                if (index != -1)
                    db.playerDiags[i].comment[ii].inputSet = db.playerDiags[index];
                index = (int)((long)dict["pd_" + i.ToString() + "_com_" + ii.ToString() + "oAns"]);
                if (index != -1)
                    db.playerDiags[i].comment[ii].outputAnswer = db.npcDiags[index];
            }
        }
        for (int i = 0; i < db.npcDiags.Count; i++)
        {
            int index = (int)((long)dict["nd_oSet_" + i.ToString()]);
            if (index != -1)
                db.npcDiags[i].outputSet = db.playerDiags[index];

            if (dict.ContainsKey("nd_oNPC_" + i.ToString()))
            {
                int index2 = (int)((long)dict["nd_oNPC_" + i.ToString()]);
                if (index2 != -1)
                    db.npcDiags[i].outputNPC = db.npcDiags[index2];
            }
        }
        repaintLines = true;
        Repaint();
    }

    //Try create a new dialogue file
    public bool tryCreate(string fName)
    {
        if (File.Exists(Application.dataPath + "/" + pathToVide +"VIDE/Resources/dialogues/" + fName + ".json"))
        {
            return false;
        }
        else
        {
            saveNames.Add(fName);
            currentDiag = saveNames.Count - 1;
            return true;
        }
    }

    //Refreshes file list
    public void loadFiles(int focused)
    {
        string[] files = Directory.GetFiles(Application.dataPath + "/" + pathToVide + "VIDE/Resources/dialogues/", "*json");
        saveNames = new List<string>();
        currentDiag = focused;
        foreach (string f in files)
        {
            saveNames.Add(Path.GetFileNameWithoutExtension(f));
        }
    }

    //Deletes dialogue
    public void DeleteDiag()
    {
        File.Delete(Application.dataPath + "/" + pathToVide + "VIDE/Resources/dialogues/" + saveNames[currentDiag] + ".json");
        loadFiles(0);
        Load(true);
    }

    public Rect IDExists()
    {
        int higherID = 0;
        foreach (VIDE_EditorDB.Answer a in db.npcDiags)
        {
            if (a.ID > higherID) { higherID = a.ID; }
        }
        foreach (VIDE_EditorDB.CommentSet c in db.playerDiags)
        {
            if (c.ID > higherID) { higherID = c.ID; }
        }
        for (int i = 0; i < 99999; i++)
        {
            if (curFocusID > higherID) curFocusID = 0;
            foreach (VIDE_EditorDB.Answer a in db.npcDiags)
            {
                if (a.ID == curFocusID) { return a.rect; }
            }
            foreach (VIDE_EditorDB.CommentSet c in db.playerDiags)
            {
                if (c.ID == curFocusID) { return c.rect; }
            }
            curFocusID++;
        }
        return new Rect(0, 0, 0, 0);
    }

    //Centers nodes
    public void CenterAll(bool cen)
    {
        Vector2 nodesCenter;
        Rect f = new Rect(0, 0, 0, 0);
        if (!cen)
        {
            curFocusID++;
            f = IDExists();
        }
        nodesCenter = new Vector2(f.x + 150, f.y + 50);
        Vector2 center = new Vector2(position.width / 2, position.height / 2);
        Vector2 offset = new Vector2();
        if (cen)
        {
            foreach (VIDE_EditorDB.Answer a in db.npcDiags)
            {
                nodesCenter.x += a.rect.x + 150;
                nodesCenter.y += a.rect.y + 50;
            }
            foreach (VIDE_EditorDB.CommentSet c in db.playerDiags)
            {
                nodesCenter.x += c.rect.x + 150;
                nodesCenter.y += c.rect.y + 50;
            }
            nodesCenter.x /= db.npcDiags.Count + db.playerDiags.Count;
            nodesCenter.y /= db.npcDiags.Count + db.playerDiags.Count;
        }
        offset = nodesCenter - center;
        foreach (VIDE_EditorDB.Answer a in db.npcDiags)
        {
            a.rect = new Rect(a.rect.x - Mathf.Round(offset.x), a.rect.y - Mathf.Round(offset.y), a.rect.width, a.rect.height);
        }
        foreach (VIDE_EditorDB.CommentSet c in db.playerDiags)
        {
            c.rect = new Rect(c.rect.x - Mathf.Round(offset.x), c.rect.y - Mathf.Round(offset.y), c.rect.width, c.rect.height);
        }
    }

    //Here's where we actually draw everything
    void OnGUI()
    {
        Event e = Event.current;
        //Set colors we'll be using later
        colors = new Color32[]{new Color32(255,255,255,255),
            new Color32(118,180,154, 220),
            new Color32(142,172,180,220),
            new Color32(84,110,137,220),
            new Color32(198,143,137,255)
        };
        defaultColor = GUI.color;

        //handle input events
        if (editEnabled)
        {
            if (!dragnWindows)
            {
                if (e.type == EventType.MouseUp && GUIUtility.hotControl == 0 && e.button == 1)
                {
                    startDiag.x = e.mousePosition.x - 150;
                    startDiag.y = e.mousePosition.y - 25;
                    GUIUtility.keyboardControl = 0;
                    Repaint();
                }
            }
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                if (GUIUtility.hotControl == 0)
                {
                    dragnWindows = true;
                    GUI.FocusWindow(99999);
                    for (int offset1 = 0; offset1 < db.playerDiags.Count; offset1++)
                    {
                        Rect offsetAdded = db.playerDiags[offset1].rect;
                        offsetAdded.x += e.delta.x;
                        offsetAdded.y += e.delta.y;
                        db.playerDiags[offset1].rect = offsetAdded;
                    }
                    for (int offset2 = 0; offset2 < db.npcDiags.Count; offset2++)
                    {
                        Rect offsetAdded2 = db.npcDiags[offset2].rect;
                        offsetAdded2.x += e.delta.x;
                        offsetAdded2.y += e.delta.y;
                        db.npcDiags[offset2].rect = offsetAdded2;
                    }
                    Repaint();
                    repaintLines = true;
                }
            }
            if (e.type == EventType.MouseUp)
            {
                if (draggingLine)
                {
                    TryConnectToAnswer(e.mousePosition, draggedCom);
                    TryConnectToSet(e.mousePosition, draggedAns);
                    needSave = true;
                }
                if (dragnWindows)
                {
                    dragnWindows = false;
                    Repaint();
                    repaintLines = true;
                }
                draggingLine = false;
            }
        }
        //Draw connection line
        if (editEnabled)
        {
            if (draggingLine)
            {
                DrawNodeLine3(dragStart, Event.current.mousePosition);
                Repaint();
            }
        }

        //Draw all connected lines
        if (e.type == EventType.Repaint && !dragnWindows)
            DrawLines();

        //Here we'll draw all of the windows
        BeginWindows();

        int setID = 0;
        int ansID = 0;
        GUI.enabled = editEnabled;
        GUIStyle st = new GUIStyle(GUI.skin.window);
        st.fontStyle = FontStyle.Bold;
        st.fontSize = 12;
        st.richText = true;
        st.wordWrap = true;
        if (db.playerDiags.Count > 0)
        {
            for (; setID < db.playerDiags.Count; setID++)
            {
                GUI.color = colors[1];
                if (!dragnWindows)
                    db.playerDiags[setID].rect = GUILayout.Window(setID, db.playerDiags[setID].rect, DrawPlayerWindow, "Player Dialogue - <color=white>ID: " + db.playerDiags[setID].ID.ToString() + "</color>", st, GUILayout.Height(40));
                else
                    db.playerDiags[setID].rect = GUILayout.Window(setID, db.playerDiags[setID].rect, DrawEmptyWindow, "Player Dialogue - <color=white>ID: " + db.playerDiags[setID].ID.ToString() + "</color>", st, GUILayout.Height(40));

                if (e.keyCode == KeyCode.Tab) focusedPlayerText++;
            }
        }
        if (db.npcDiags.Count > 0)
        {
            for (; ansID < db.npcDiags.Count; ansID++)
            {
                GUI.color = colors[2];
                if (!dragnWindows)
                    db.npcDiags[ansID].rect = GUILayout.Window(ansID + setID, db.npcDiags[ansID].rect, DrawNPCWindow, "NPC Dialogue - <color=white>ID: " + db.npcDiags[ansID].ID.ToString() + "</color>", st, GUILayout.Height(40));
                else
                    db.npcDiags[ansID].rect = GUILayout.Window(ansID + setID, db.npcDiags[ansID].rect, DrawEmptyWindow, "NPC Dialogue - <color=white>ID: " + db.npcDiags[ansID].ID.ToString() + "</color>", st, GUILayout.Height(40));
            }
        }

        //Here we check for errors in the node structure

        npcReady = true; playerReady = true;
        hasID = false;
        for (int i = 0; i < db.npcDiags.Count; i++)
        {
            if (!db.npcDiags[i].endConversation && db.npcDiags[i].outputSet == null)
            {
                if (db.npcDiags[i].outputNPC == null)
                { npcReady = false; break; }
            }

            if (startID == db.npcDiags[i].ID)
            {
                hasID = true;
            }
        }
        for (int i = 0; i < db.playerDiags.Count; i++)
        {
            for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
            {
                if (db.playerDiags[i].comment[ii].outputAnswer == null)
                {
                    playerReady = false; break;
                }
            }
            if (startID == db.playerDiags[i].ID)
            {
                hasID = true;
            }
        }
        if (!hasID) npcReady = false;

        if (Event.current.type == EventType.Layout)
        {
            showError = false;
            if (!npcReady || !playerReady)
                showError = true;
        }
        GUI.color = colors[0];
        GUI.SetNextControlName("startD");
        startDiag = GUILayout.Window(99999, startDiag, DrawStartWindow, "Editor tools:", st);
        GUI.enabled = true;
        if (newFile)
        {
            fWin = new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 2, 0);
            fWin = GUILayout.Window(99998, fWin, DrawNewFileWindow, "New Dialogue:");
            GUI.FocusWindow(99998);
        }
        if (overwritePopup)
        {
            fWin = new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 2, 0);
            fWin = GUILayout.Window(99997, fWin, DrawOverwriteWindow, "File Already Exists!");
            GUI.FocusWindow(99997);
        }
        if (deletePopup)
        {
            fWin = new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 2, 0);
            fWin = GUILayout.Window(99996, fWin, DrawDeleteWindow, "Are you sure?");
            GUI.FocusWindow(99996);
        }
        EndWindows();

        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
        {
            areYouSure = false;
            GUIUtility.keyboardControl = 0;
            Repaint();
        }

        //Here's where we'll autosave if everything's in order

        if (autosaveON && npcReady && playerReady && Event.current.type == EventType.Repaint)
        {
            if (lastTextFocus != "startD" && lastTextFocus != "")
                if (lastTextFocus != GUI.GetNameOfFocusedControl()) { needSave = true; }
            lastTextFocus = GUI.GetNameOfFocusedControl();

            if (saveNames.Count > 0)
            {
                if (needSave)
                {
                    Save();
                    needSave = false;
                }
            }
        }
    }

    //Draw empty window while scrolling everything for better performance
    void DrawEmptyWindow(int id)
    {
        GUILayout.Box("", GUILayout.Width(300), GUILayout.Height(50));
    }

    //The player node
    void DrawPlayerWindow(int id)
    {
        GUI.enabled = editEnabled;
        bool dontDrag = false;
        if (Event.current.type == EventType.MouseUp)
        {
            draggingLine = false;
            dontDrag = true;
        }
        GUILayout.BeginHorizontal();
        GUI.color = colors[1];
        string delText = "Delete Node";
        if (areYouSureIndex == id)
            if (areYouSure) { delText = "Sure?"; GUI.color = new Color32(176, 128, 54, 255); }
        if (GUILayout.Button(delText, GUILayout.Width(80)))
        {
            if (areYouSureIndex != id) areYouSure = false;
            if (!areYouSure)
            {
                areYouSure = true;
                areYouSureIndex = id;
            }
            else
            {
                areYouSure = false;
                areYouSureIndex = 0;
                Undo.RecordObject(db, "Removed Player Node");
                removeSet(db.playerDiags[id]);
                needSave = true;
                return;
            }
        }
        if (Event.current.type == EventType.MouseDown)
        {
            areYouSure = false;
            Repaint();
        }
        GUI.color = defaultColor;
        if (GUILayout.Button("Add comment"))
        {
            areYouSure = false;
            addComment(db.playerDiags[id]);
            needSave = true;
        }
        GUILayout.EndHorizontal();
        for (int i = 0; i < db.playerDiags[id].comment.Count; i++)
        {
            if (db.playerDiags[id].comment.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label((i + 1).ToString() + ". ", GUILayout.Width(20));
                if (i == 0) GUILayout.Space(24);
                if (i != 0)
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        areYouSure = false;
                        removeComment(db.playerDiags[id].comment[i]);
                        needSave = true;
                        return;
                    }
                GUIStyle stf = new GUIStyle(GUI.skin.textField);
                stf.wordWrap = true;
                GUI.SetNextControlName("pText_" + id.ToString() + i.ToString());
                Undo.RecordObject(db, "Edited Player comment");
                db.playerDiags[id].comment[i].text = EditorGUILayout.TextArea(db.playerDiags[id].comment[i].text, stf, GUILayout.Width(230));
                if (db.playerDiags[id].comment[i].outputAnswer == null)
                {
                    Rect lr;
                    GUI.color = new Color32(176, 128, 54, 255); ;
                    if (GUILayout.RepeatButton("O", GUILayout.Width(30)))
                    {
                        areYouSure = false;
                        lr = GUILayoutUtility.GetLastRect();
                        lr = new Rect(lr.x + db.playerDiags[id].rect.x + 30, lr.y + db.playerDiags[id].rect.y + 7, 0, 0);
                        if (!draggingLine && !dontDrag)
                        {
                            draggedCom = db.playerDiags[id].comment[i];
                            draggedAns = null;
                            dragStart = new Vector2(lr.x, lr.y);
                            draggingLine = true;
                        }
                    }
                    GUI.color = defaultColor;
                }
                else
                {
                    GUI.color = defaultColor;
                    if (GUILayout.Button("x", GUILayout.Width(30)))
                    {
                        areYouSure = false;
                        breakConnection(0, db.playerDiags[id].comment[i], null);
                        needSave = true;
                    }
                    if (Event.current.type == EventType.Repaint)
                    {
                        db.playerDiags[id].comment[i].outRect = GUILayoutUtility.GetLastRect();
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
        {
            areYouSure = false;
            Repaint();
        }
        if (Event.current.commandName == "UndoRedoPerformed")
            Repaint();
        GUI.DragWindow();
    }

    //The NPC node
    void DrawNPCWindow(int id)
    {
        GUI.enabled = editEnabled;
        bool dontDrag = false;
        if (Event.current.type == EventType.MouseUp)
        {
            draggingLine = false;
            dontDrag = true;
        }
        int ansID = id - (db.playerDiags.Count);
        if (ansID < 0)
            ansID = 0;
        GUILayout.BeginHorizontal();
        GUI.color = colors[2];
        string delText = "Delete Node";
        if (areYouSureIndex == id)
            if (areYouSure) { delText = "Sure?"; GUI.color = new Color32(176, 128, 54, 255); }
        if (GUILayout.Button(delText, GUILayout.Width(80)))
        {
            if (areYouSureIndex != id) areYouSure = false;
            if (!areYouSure)
            {
                areYouSure = true;
                areYouSureIndex = id;
            }
            else
            {
                areYouSure = false;
                areYouSureIndex = 0;
                Undo.RecordObject(db, "removed node");
                removeAnswer(db.npcDiags[ansID]);
                needSave = true;
                return;
            }
        }
        if (Event.current.type == EventType.MouseDown)
        {
            areYouSure = false;
            Repaint();
        }
        GUI.color = defaultColor;
        if (db.npcDiags[ansID].endConversation)
        {
            GUI.color = Color.green;
        }
        else if (db.npcDiags[ansID].outputSet == null && db.npcDiags[ansID].outputNPC == null)
        {
            GUI.color = Color.red;
        }
        if (GUILayout.Button("End here: " + db.npcDiags[ansID].endConversation.ToString()))
        {
            areYouSure = false;
            Undo.RecordObject(db, "Changed End Point for id");
            db.npcDiags[ansID].endConversation = !db.npcDiags[ansID].endConversation;
            db.npcDiags[ansID].outputSet = null;
            db.npcDiags[ansID].outputNPC = null;
            needSave = true;
        }
        GUI.color = defaultColor;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUIStyle stf = new GUIStyle(GUI.skin.textField);
        stf.wordWrap = true;
        GUI.SetNextControlName("nText_" + id.ToString());
        Undo.RecordObject(db, "textCange");
        db.npcDiags[ansID].text = EditorGUILayout.TextArea(db.npcDiags[ansID].text, stf, GUILayout.Width(260));
        if (!db.npcDiags[ansID].endConversation)
        {
            if (db.npcDiags[ansID].outputSet == null && db.npcDiags[ansID].outputNPC == null)
            {
                Rect lr;
                if (GUILayout.RepeatButton("O", GUILayout.Width(30)))
                {
                    areYouSure = false;
                    lr = GUILayoutUtility.GetLastRect();
                    lr = new Rect(lr.x + db.npcDiags[ansID].rect.x + 30, lr.y + db.npcDiags[ansID].rect.y + 7, 0, 0);
                    if (!draggingLine && !dontDrag)
                    {
                        draggedAns = db.npcDiags[ansID];
                        draggedCom = null;
                        dragStart = new Vector2(lr.x, lr.y);
                        draggingLine = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("x", GUILayout.Width(30)))
                {
                    areYouSure = false;
                    breakConnection(1, null, db.npcDiags[ansID]);
                    needSave = true;
                }
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        GUILayout.Label("Tag: ", GUILayout.Width(30));
        GUI.color = Color.cyan;
        Undo.RecordObject(db, "changed Tag");
        db.npcDiags[ansID].tag = EditorGUILayout.TextField(db.npcDiags[ansID].tag, stf, GUILayout.Width(80));
        GUI.color = defaultColor;

        GUILayout.Label("Extra data: ", GUILayout.Width(70));
        GUI.color = Color.cyan;
        Undo.RecordObject(db, "changed extraData");
        db.npcDiags[ansID].extraData = EditorGUILayout.TextField(db.npcDiags[ansID].extraData, stf);
        GUI.color = defaultColor;

        GUILayout.EndHorizontal();
        if (Event.current.commandName == "UndoRedoPerformed")
            Repaint();
        GUI.DragWindow();
    }

    //The Editor Tools 
    void DrawStartWindow(int id)
    {
        GUI.enabled = editEnabled;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Currently editing: ", GUILayout.Width(110));
        int t_file = fileIndex;
        EditorGUI.BeginChangeCheck();
        fileIndex = EditorGUILayout.Popup(fileIndex, saveNames.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            if (t_file != fileIndex)
            {
                if (autosaveON && npcReady && playerReady)
                {
                    Save();
                }
                currentDiag = fileIndex;
                Load(true);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Add new dialogue"))
        {
            editEnabled = false;
            newFile = true;
            GUI.FocusWindow(99998);
        }
        GUI.color = defaultColor;

        if (saveNames.Count > 0)
            if (GUILayout.Button("Delete current", GUILayout.Width(100)))
            {
                editEnabled = false;
                deletePopup = true;
            }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUIStyle bb = new GUIStyle(GUI.skin.label);
        bb.fontStyle = FontStyle.Bold;
        bb.normal.textColor = Color.red;
        if (!showError)
        {
            if (needSave) GUI.color = Color.yellow;
            if (GUILayout.Button("Save"))
            {
                editEnabled = false;
                overwritePopup = true;
            }
            GUI.color = defaultColor;
            if (needSave) GUI.color = defaultColor;
            autosaveON = GUILayout.Toggle(autosaveON, "Autosave");
        }
        else
        {
            GUILayout.Label("Check your Start and End Nodes!", bb);
        }
        GUILayout.EndHorizontal();
        GUILayout.Label("Center View: ", GUILayout.Width(100));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("On All"))
        {
            CenterAll(true);
            Repaint();
        }
        if (GUILayout.Button("On Node"))
        {
            CenterAll(false);
            Repaint();
        }
        GUILayout.EndHorizontal();
        GUI.enabled = true;
        GUILayout.Label("Add nodes: ", GUILayout.Width(100));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add NPC dialogue"))
        {
            db.npcDiags.Add(new VIDE_EditorDB.Answer(startDiag, setUniqueID()));
            needSave = true;
        }
        if (GUILayout.Button("Add Player dialogue"))
        {
            db.playerDiags.Add(new VIDE_EditorDB.CommentSet(startDiag, setUniqueID()));
            needSave = true;
        }
        GUILayout.EndHorizontal();
        if (!hasID) { GUI.color = Color.red; }
        startID = EditorGUILayout.IntField("Start Node ID: ", startID);
        GUI.color = defaultColor;
        GUI.DragWindow();
    }

    void DrawNewFileWindow(int id)
    {
        GUI.FocusControl("createFile");
        GUIStyle st = new GUIStyle(GUI.skin.label);
        st.alignment = TextAnchor.UpperCenter;
        st.fontSize = 16;
        st.fontStyle = FontStyle.Bold;
        GUILayout.Label("Please name your new dialogue:", st);
        GUIStyle stf = new GUIStyle(GUI.skin.textField);
        stf.fontSize = 14;
        stf.alignment = TextAnchor.MiddleCenter;
        GUI.SetNextControlName("createFile");
        newFileName = GUILayout.TextField(newFileName, stf, GUILayout.Height(40));
        GUI.color = Color.green;
        if (GUILayout.Button("Create", GUILayout.Height(30)))
        {
            if (tryCreate(newFileName))
            {
                fileIndex = currentDiag;
                newFileName = "My Dialogue";
                editEnabled = true;
                newFile = false;
                errorMsg = "";
                needSave = true;
                Load(true);
                Repaint();
            }
            else
            {
                errorMsg = "File already exists!";
            }
        }
        if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.keyUp)
        {
            if (tryCreate(newFileName))
            {
                fileIndex = currentDiag;
                newFileName = "My Dialogue";
                editEnabled = true;
                newFile = false;
                errorMsg = "";
                needSave = true;
                Load(true);
                Repaint();
            }
            else
            {
                errorMsg = "File already exists!";
            }
        }
        GUI.color = defaultColor;
        if (GUILayout.Button("Cancel", GUILayout.Height(20)) || Event.current.keyCode == KeyCode.Escape)
        {
            newFileName = "My Dialogue";
            editEnabled = true;
            newFile = false;
            errorMsg = "";
            Repaint();
        }
        st.normal.textColor = Color.red;
        GUILayout.Label(errorMsg, st);
    }

    void DrawOverwriteWindow(int id)
    {
        GUIStyle st = new GUIStyle(GUI.skin.label);
        st.alignment = TextAnchor.UpperCenter;
        st.fontSize = 16;
        st.fontStyle = FontStyle.Bold;

        if (saveNames.Count > 0)
        {
            if (File.Exists(Application.dataPath + "/" + pathToVide + "VIDE/Resources/dialogues/" + saveNames[currentDiag] + ".json"))
            {
                GUILayout.Label("File Already Exists! Overwrite?", st);
                if (GUILayout.Button("Yes!", GUILayout.Height(30)))
                {
                    Save();
                    needSave = false;
                    newFileName = "My Dialogue";
                    editEnabled = true;
                    overwritePopup = false;
                    newFile = false;
                    errorMsg = "";
                }
                if (GUILayout.Button("No", GUILayout.Height(20)))
                {
                    newFileName = "My Dialogue";
                    editEnabled = true;
                    overwritePopup = false;
                    newFile = false;
                    errorMsg = "";
                }
                GUILayout.Space(10);
            }
        }
        GUILayout.Label("Save as new...", st);
        GUIStyle stf = new GUIStyle(GUI.skin.textField);
        stf.fontSize = 14;
        stf.alignment = TextAnchor.MiddleCenter;
        newFileName = GUILayout.TextField(newFileName, stf, GUILayout.Height(40));
        if (GUILayout.Button("Save", GUILayout.Height(20)))
        {
            if (tryCreate(newFileName))
            {
                fileIndex = currentDiag;
                Load(false);
                newFileName = "My Dialogue";
                editEnabled = true;
                newFile = false;
                overwritePopup = false;
                errorMsg = "";
                needSave = true;
            }
            else
            {
                errorMsg = "File already exists!";
            }
        }
        if (GUILayout.Button("Cancel", GUILayout.Height(20)))
        {
            newFileName = "My Dialogue";
            editEnabled = true;
            overwritePopup = false;
            newFile = false;
            errorMsg = "";
        }
        st.normal.textColor = Color.red;
        GUILayout.Label(errorMsg, st);
    }

    void DrawDeleteWindow(int id)
    {
        GUIStyle st = new GUIStyle(GUI.skin.label);
        st.alignment = TextAnchor.UpperCenter;
        st.fontSize = 16;
        st.fontStyle = FontStyle.Bold;
        GUILayout.Label("Are you sure you want to delete " + "'" + saveNames[fileIndex] + "'?", st);
        if (GUILayout.Button("Yes", GUILayout.Height(30)) || Event.current.keyCode == KeyCode.Return)
        {
            DeleteDiag();
            fileIndex = 0;
            editEnabled = true;
            deletePopup = false;
            newFile = false;
        }
        if (GUILayout.Button("No", GUILayout.Height(20)) || Event.current.keyCode == KeyCode.Escape)
        {
            editEnabled = true;
            deletePopup = false;
            newFile = false;
        }
    }

    void DrawLines()
    {
        Handles.color = colors[3];
        if (editEnabled)
        {
            if (draggingLine)
            {
                DrawNodeLine3(dragStart, Event.current.mousePosition);
                Repaint();
            }
            for (int i = 0; i < db.playerDiags.Count; i++)
            {
                for (int ii = 0; ii < db.playerDiags[i].comment.Count; ii++)
                {
                    if (db.playerDiags[i].comment[ii].outputAnswer != null)
                    {
                        DrawNodeLine(db.playerDiags[i].comment[ii].outRect,
                        db.playerDiags[i].comment[ii].outputAnswer.rect, db.playerDiags[i].rect);
                    }
                }
            }
            for (int i = 0; i < db.npcDiags.Count; i++)
            {
                if (db.npcDiags[i].outputSet != null)
                {
                    DrawNodeLine2(db.npcDiags[i].rect,
                    db.npcDiags[i].outputSet.rect);
                }

                if (db.npcDiags[i].outputNPC != null)
                {
                    DrawNodeLine2(db.npcDiags[i].rect,
                    db.npcDiags[i].outputNPC.rect);
                }
            }
        }
    }

    void DrawNodeLine(Rect start, Rect end, Rect sPos)
    {
        Color nc = Color.black;
        Color nc2 = colors[1];
        nc.a = 0.25f;
        nc2.r -= 0.15f;
        nc2.g -= 0.15f;
        nc2.b -= 0.15f;
        nc2.a = 1;
        Handles.DrawBezier(
           new Vector3(start.x + sPos.x + 30, start.y + sPos.y + 12, 0),
           new Vector3(end.x, end.y + (end.height / 2) + 2, 0),
           new Vector3(end.x, end.y + (end.height / 2) + 2, 0),
           new Vector3(start.x + sPos.x + 30, start.y + sPos.y + 12, 0),
           nc,
           null,
           7
           );
        Handles.DrawBezier(
            new Vector3(start.x + sPos.x + 30, start.y + sPos.y + 10, 0),
            new Vector3(end.x, end.y + (end.height / 2), 0),
            new Vector3(end.x, end.y + (end.height / 2), 0),
            new Vector3(start.x + sPos.x + 30, start.y + sPos.y + 10, 0),
            colors[1],
            null,
            5
            );
    }

    void DrawNodeLine2(Rect start, Rect end)
    {
        Color nc = Color.black;
        Color nc2 = colors[2];
        nc.a = 0.25f;
        nc2.r -= 0.15f;
        nc2.g -= 0.15f;
        nc2.b -= 0.15f;
        nc2.a = 1;
        Handles.DrawBezier(
       new Vector3(start.x + 295, start.y + 52, 0),
       new Vector3(end.x, end.y + (end.height / 2) + 2, 0),
       new Vector3(end.x, end.y + (end.height / 2) + 2, 0),
       new Vector3(start.x + 295, start.y + 52, 0),
       nc,
       null,
       7
       );
        Handles.DrawBezier(
            new Vector3(start.x + 295, start.y + 50, 0),
            new Vector3(end.x, end.y + (end.height / 2), 0),
            new Vector3(end.x, end.y + (end.height / 2), 0),
            new Vector3(start.x + 295, start.y + 50, 0),
            nc2,
            null,
            5
            );

        if (repaintLines)
        {
            Repaint();
            repaintLines = false;
        }
    }

    void DrawNodeLine3(Vector2 start, Vector2 end)
    {
        Handles.DrawBezier(
            new Vector3(start.x, start.y, 0),
            new Vector3(end.x, end.y, 0),
            new Vector3(end.x, end.y, 0),
            new Vector3(start.x, start.y, 0),
            colors[0],
            null,
            5
            );
    }

    //Clean the database
    void ClearAll()
    {
        db.npcDiags = new List<VIDE_EditorDB.Answer>();
        db.playerDiags = new List<VIDE_EditorDB.CommentSet>();
    }
}