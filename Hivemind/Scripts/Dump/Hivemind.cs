using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityStandardAssets._2D;

public class Hivemind : MonoBehaviour {
    
    public List<GameObject> characters;
    public GameObject commonCamera;
    public GameObject ui;
    
    GameObject currentCharacter;
    int currentCharacterI = 0;
    
	void Start () {
        // Initializes the hivemind.
        for (int i = 0; i < transform.childCount; i++)
        {
            characters.Add(transform.GetChild(i).gameObject);
        }

        // Sets the currently active character.
        currentCharacter = characters[0];
        currentCharacter.GetComponent<PlayerCharacter>().enabled = true;
        //currentCharacter.GetComponent<PlatformerCharacter2D>().enabled = true;
        currentCharacter.GetComponent<Platformer2DUserControl>().enabled = true;
        currentCharacter.GetComponent<PlayerCharacter>().SetActiveState(true);

        if (Camera.main.GetComponent<CustomCamera>())
            Camera.main.GetComponent<CustomCamera>().target = currentCharacter.transform;
        
        if (Camera.main.transform.parent.GetComponent<BorderStopCamera>())
            Camera.main.transform.parent.GetComponent<BorderStopCamera>().target = currentCharacter.transform;

        // Finds stuff.
        if (ui == null) ui = GameObject.Find("UI");
        commonCamera = Camera.main.gameObject;
	}
	
	void Update () {
        // Mouse scrollwheel (changes character).
	    if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (currentCharacterI < characters.Count - 1) currentCharacterI++;
            else currentCharacterI = 0;

            SwitchCharacter();

			GameObject.Find ("DebugDisplay").GetComponent<DebugDisplay>().SetText("Current Character: " + currentCharacterI);
        }
    }

    void SwitchCharacter()
    {
        // Probably needs optimization.
        currentCharacter.GetComponent<PlatformerCharacter2D>().Move(0, false, false);
        currentCharacter.GetComponent<Platformer2DUserControl>().enabled = false;
        currentCharacter.GetComponent<PlayerCharacter>().SetActiveState(false);
        currentCharacter = characters[currentCharacterI];
        currentCharacter.GetComponent<Platformer2DUserControl>().enabled = true;
        currentCharacter.GetComponent<PlayerCharacter>().SetActiveState(true);

        ui.transform.FindChild("TriggerIndicator").gameObject.SetActive(false);
        /*
        if (commonCamera.GetComponent<CustomCamera>() != null)
            commonCamera.GetComponent<CustomCamera>().target = currentCharacter.transform;
        if (Camera.main.GetComponent<BorderStopCamera>())
            Camera.main.GetComponent<BorderStopCamera>().target = currentCharacter.transform;
            */

        if (Camera.main.transform.parent.GetComponent<Cameras>())
        {
            //Camera.main.transform.parent.GetComponent<Cameras>().target = currentCharacter.transform;
        }
    }

    /// <summary>
    /// Adds a character to the hivemind.
    /// </summary>
    /// <param name="character"></param>
    public void AddCharacter(GameObject character)
    {
        characters.Add(character);
    }
}
