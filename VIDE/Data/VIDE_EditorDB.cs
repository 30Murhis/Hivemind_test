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
using System;
using System.Collections;
using System.Collections.Generic;

public class VIDE_EditorDB : MonoBehaviour, ISerializationCallbackReceiver
{

    /*
     * Here is were we store all of the temporal data generated on the VIDE Editor. 
     * When the VIDE Editor saves, it will save all of the data we store here into a json file.
     * Since the VIDE Editor allows the creation of endless structures, this script also handles
     * serialization and deserialization of data in order to avoid object composition cycles and
     * to be able to implement Undo/Redo. 
     */
	
	public class CommentSet
    {
		[NonSerialized]
        public List<Comment> comment;
        public Rect rect;
        public int ID;

        public CommentSet()
        {
            comment = new List<Comment>();
            rect = new Rect(20, 200, 300, 100);
        }

        public CommentSet(Rect pos, int id)
        {
            comment = new List<Comment>();
            comment.Add(new Comment());
            rect = new Rect(pos.x, pos.y + 200, 300, 100);
			ID = id;
        }

        public CommentSet(Vector2 rectPos, int comSize, int id)
        {
            rect = new Rect(rectPos.x, rectPos.y, 300, 100);
            comment = new List<Comment>();
            ID = id;
            for (int i = 0; i < comSize; i++)
                comment.Add(new Comment());
        }
    }
	
    public class Comment
    {
        public string text;
        public CommentSet inputSet; 
        public Answer outputAnswer;
        public Rect outRect;

        public Comment()
        {
            outputAnswer = null;
            inputSet = null;
            text = "Comment...";
			
        }
        public Comment(CommentSet id)
        {
            outputAnswer = null;
            inputSet = id;
            text = "Comment...";
        }
    }

    public class Answer
    {
        public string text;
		[NonSerialized]
        public CommentSet outputSet;
        public Answer outputNPC;

        public Rect rect;

        public bool endConversation;
		public string extraData;
		public string tag;
		
        public int ID;

        public Answer()
        {
            text = "NPC's comment...";
            outputSet = null;
        	outputNPC = null;
            endConversation = true;
            rect = new Rect(20, 160, 300, 50);
			extraData = "";
			tag = "";
        }

        public Answer(Vector2 rPos, bool endC, string t, int id, string exData, string tagt)
        {
            text = t;
            outputSet = null;
            outputNPC = null;
            endConversation = endC;
            rect = new Rect(rPos.x, rPos.y, 300, 50);
			extraData = exData;
			tag = tagt;
            ID = id;
        }
        public Answer(Rect pos, int id)
        {
			extraData = "";
			tag = "";
            text = "NPC's comment...";
            outputSet = null;
            outputNPC = null;
			ID = id;
            endConversation = true;
            rect = new Rect(pos.x, pos.y + 200, 300, 50);
        }
    }
	
	public List<CommentSet> playerDiags = new List<CommentSet>();
    public List<Answer> npcDiags = new List<Answer>();

    //SERIALIZATION...

    public List<Serialized_playerDiags> S_playerDiags;
    public List<Serialized_npcDiags> S_npcDiags;


    public void OnBeforeSerialize()
    {
        npcSerialize();
        playerSerialize();
    }

    public void OnAfterDeserialize()
    {
        if (S_npcDiags.Count > 0)
            npcDiags = npcDeserialize();
        else
            npcDiags = new List<Answer>();
		
		 if (S_playerDiags.Count > 0)
            playerDiags = playerDeserialize();
        else
            playerDiags = new List<CommentSet>();
		
		ConnectNodes();
    }

    [Serializable]
    public struct Serialized_npcDiags
    {
        public string extraData;
        public string tag;
        public string text;
        public int ID;
        public bool endConversation;
        public Rect rect;
        public int outNPCIndex;
        public int outSetIndex;
    }
	[Serializable]
    public struct Serialized_playerDiags
    {
        public int commentCount;
		public List<Serialized_comment> s_comment;
        public int ID;
        public Rect rect;
    }
	[Serializable]
    public struct Serialized_comment
    {
        public string text;
        public int inputSetIndex;
        public int outputAnswerIndex;
        public Rect outRect;
    }

    void npcSerialize()
    {
        List<Serialized_npcDiags> S_npcDiag = new List<Serialized_npcDiags>();
        foreach (var child in npcDiags)
        {
            Serialized_npcDiags np = new Serialized_npcDiags() {
                extraData = child.extraData,
                tag = child.tag,
                text = child.text,
                ID = child.ID,
                endConversation = child.endConversation,
                rect = child.rect,
				outSetIndex = playerDiags.IndexOf(child.outputSet),
				outNPCIndex = npcDiags.IndexOf(child.outputNPC)
            };
            S_npcDiag.Add(np);
        }
        S_npcDiags = S_npcDiag;
    }
	
	  void playerSerialize()
    {
        List<Serialized_playerDiags> S_playerDiag = new List<Serialized_playerDiags>();
		
		//Serialize commentSets
        foreach (var child in playerDiags)
        {
            Serialized_playerDiags np = new Serialized_playerDiags() {
                commentCount = child.comment.Count,
                ID = child.ID,
                rect = child.rect,
            };
			//Serialize comments inside this set
			np.s_comment = new List<Serialized_comment>();			
			for (int i = 0; i < np.commentCount; i++)
       		{
				Serialized_comment sc = new Serialized_comment() {
					text = child.comment[i].text,
					outRect = child.comment[i].outRect,
					outputAnswerIndex = npcDiags.IndexOf(child.comment[i].outputAnswer),
					inputSetIndex = playerDiags.IndexOf(child)
				};			
				np.s_comment.Add(sc);
			}
			
            S_playerDiag.Add(np);			
        }

        S_playerDiags = S_playerDiag;
    }

    List<Answer> npcDeserialize()
    {
        List<Answer> temp_npcDiags = new List<Answer>();
        foreach (var child in S_npcDiags)
        {
            temp_npcDiags.Add(new Answer());
            temp_npcDiags[temp_npcDiags.Count - 1].text = child.text;
            temp_npcDiags[temp_npcDiags.Count - 1].endConversation = child.endConversation;
            temp_npcDiags[temp_npcDiags.Count - 1].ID = child.ID;
            temp_npcDiags[temp_npcDiags.Count - 1].rect = child.rect;
            temp_npcDiags[temp_npcDiags.Count - 1].extraData = child.extraData;
            temp_npcDiags[temp_npcDiags.Count - 1].tag = child.tag;
        }

        return temp_npcDiags;
    }
	
	List<CommentSet> playerDeserialize()
    {
        List<CommentSet> temp_playerDiags = new List<CommentSet>();
        foreach (var child in S_playerDiags)
        {
            temp_playerDiags.Add(new CommentSet());
            temp_playerDiags[temp_playerDiags.Count - 1].ID = child.ID;
            temp_playerDiags[temp_playerDiags.Count - 1].rect = child.rect;
			for (int i = 0; i < child.commentCount; i++)
			{	
				CommentSet s = temp_playerDiags[temp_playerDiags.Count - 1];
				s.comment.Add (new Comment());
				s.comment[i].text = child.s_comment[i].text;
				s.comment[i].outRect = child.s_comment[i].outRect;		
			}
        }

        return temp_playerDiags;
    }
	
    //Now we can connect all of the nodes 
	void ConnectNodes(){
		
		for (int i = 0; i <playerDiags.Count; i++) {
			for	(int ii = 0; ii < playerDiags[i].comment.Count; ii++){
				playerDiags[i].comment[ii].inputSet = playerDiags[i];
				if (S_playerDiags[i].s_comment[ii].outputAnswerIndex >= 0)
					playerDiags[i].comment[ii].outputAnswer = npcDiags[S_playerDiags[i].s_comment[ii].outputAnswerIndex];
				else
					playerDiags[i].comment[ii].outputAnswer = null;
			}
		}
		
		for (int i = 0; i <npcDiags.Count; i++) {
			if (S_npcDiags[i].outSetIndex >= 0){
				npcDiags[i].outputSet = playerDiags[S_npcDiags[i].outSetIndex];				
			}
			else {
				npcDiags[i].outputSet = null;																		
			}
			
			if (S_npcDiags[i].outNPCIndex >= 0){
				npcDiags[i].outputNPC = npcDiags[S_npcDiags[i].outNPCIndex];				
			}
			else {
				npcDiags[i].outputNPC = null;																		
			}
		}	
		
	}





}
