using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets._2D;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class AdvancedHivemind : MonoBehaviour
{
    public List<GameObject> characters;
    public GameObject ui;

    GameObject currentCharacter;
    int currentCharacterI = 0;
    float scroll = 0;

    public List<InfectedCharacter> hivemind = new List<InfectedCharacter>();
    Cameras cameraManager;

    // Singleton
    static AdvancedHivemind instance;
    
    public static AdvancedHivemind GetInstance()
    {
        return instance;
    }

    void Start()
    {
        // Checking for existing singleton
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Initializes the hivemind
        for (int i = 0; i < transform.childCount; i++)
        {
#if UNITY_5_3_OR_NEWER
            hivemind.Add(new InfectedCharacter() { Character = transform.GetChild(i).gameObject, Floor = SceneManager.GetActiveScene().buildIndex, InPlayerControl = i == 0, Life = 100 });
#else
            hivemind.Add(new InfectedCharacter() { Character = transform.GetChild(i).gameObject, Floor = Application.loadedLevel, InPlayerControl = i == 0, Life = 100 });
#endif
        }

        // Sets the currently active character
        currentCharacter = hivemind[0].Character;
        currentCharacter.GetComponent<PlayerCharacter>().enabled = true;
        currentCharacter.GetComponent<PlayerCharacter>().SetActiveState(true);
        currentCharacter.GetComponent<Platformer2DUserControl>().enabled = true;

        cameraManager = Camera.main.transform.parent.gameObject.GetComponent<Cameras>();
        cameraManager.target = currentCharacter.transform;

        // Finds stuff
        if (ui == null) ui = GameObject.FindGameObjectWithTag("UI");
    }

    void Update()
    {
        // Mouse scrollwheel (changes character), no console key yet
        scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (hivemind.Count < 2) return;

            if (scroll > 0)
            {

                if (currentCharacterI < hivemind.Count - 1) currentCharacterI++;
                else currentCharacterI = 0;
            }

            if (scroll < 0)
            {
                if (currentCharacterI > 0) currentCharacterI--;
                else currentCharacterI = hivemind.Count - 1;
            }
            
            SwitchCharacter();
            FindObjectOfType<DebugDisplay>().SetText("Currently controlling " + currentCharacter.name);
        }
    }

    void SwitchCharacter()
    {
        // Probably needs optimization
        currentCharacter.GetComponent<PlatformerCharacter2D>().Move(0, false, false);
        currentCharacter.GetComponent<Platformer2DUserControl>().enabled = false;
        currentCharacter.GetComponent<PlayerCharacter>().SetActiveState(false);
        currentCharacter = hivemind[currentCharacterI].Character;
        currentCharacter.GetComponent<Platformer2DUserControl>().enabled = true;
        currentCharacter.GetComponent<PlayerCharacter>().SetActiveState(true);

        ui.transform.FindChild("TriggerIndicator").gameObject.SetActive(false);
        
        cameraManager.ChangeTargetSmooth(currentCharacter);
    }

    /// <summary>
    /// Adds a character to the hivemind.
    /// </summary>
    /// <param name="character"></param>
    public void AddCharacter(GameObject character)
    {
#if UNITY_5_3_OR_NEWER
        hivemind.Add(new InfectedCharacter() { Character = character, Floor = SceneManager.GetActiveScene().buildIndex, InPlayerControl = false, Life = 100 });
#else
        hivemind.Add(new InfectedCharacter() { Character = character, Floor = Application.loadedLevel, InPlayerControl = false, Life = 100 });
#endif
    }
}

public class InfectedCharacter
{
    public GameObject Character { get; set; }
    public bool InPlayerControl { get; set; }
    public int Floor { get; set; }
    public int Life { get; set; } // Decay time/life time
}
