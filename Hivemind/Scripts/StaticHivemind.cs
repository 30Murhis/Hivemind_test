using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets._2D;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class StaticHivemind : MonoBehaviour
{
    protected static GameObject currentCharacter;
    protected static int currentCharacterI = 0;

    protected static List<InfectedCharacters> hivemind = new List<InfectedCharacters>();
    Cameras cameraManager;

    static void SwitchCharacter()
    {
        if (currentCharacterI < hivemind.Count - 1) currentCharacterI++;
        else currentCharacterI = 0;

        // Probably needs optimization
        currentCharacter.GetComponent<PlatformerCharacter2D>().Move(0, false, false);
        currentCharacter.GetComponent<Platformer2DUserControl>().enabled = false;
        currentCharacter.GetComponent<PlayerCharacter>().SetActiveState(false);
        currentCharacter = hivemind[currentCharacterI].Character;
        currentCharacter.GetComponent<Platformer2DUserControl>().enabled = true;
        currentCharacter.GetComponent<PlayerCharacter>().SetActiveState(true);
    }

    /// <summary>
    /// Adds a character to the hivemind.
    /// </summary>
    /// <param name="character"></param>
    static void AddCharacter(GameObject character)
    {
#if UNITY_5_3_OR_NEWER
        hivemind.Add(new InfectedCharacters() { Character = character, Floor = SceneManager.GetActiveScene().buildIndex, InPlayerControl = false, Life = 100 });
#else
        hivemind.Add(new InfectedCharacters() { Character = character, Floor = Application.loadedLevel, InPlayerControl = false, Life = 100 });
#endif
    }

    /// <summary>
    /// Removes a character from the hivemind.
    /// </summary>
    /// <param name="character"></param>
    static void RemoveCharacter(GameObject character)
    {
        hivemind.RemoveAll(i => i.Character == character);
    }
}

public class InfectedCharacters
{
    public GameObject Character { get; set; }
    public bool InPlayerControl { get; set; }
    public int Floor { get; set; }
    public int Life { get; set; } // Replaceable with time
}
