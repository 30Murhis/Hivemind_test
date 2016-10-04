using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles player movement and interaction with other NPC game objects
/// </summary>
public class CharacterInteraction : MonoBehaviour
{
    //Reference to our diagUI script for quick access
    public exampleUI diagUI;
    public float reachDistance = 2.5f;

    PlayerCharacter pc;
    NPC discussionPartner;

    // Remote controlled NPC check for testing purposes
    bool isControlledNPC;

    void Start()
    {
        pc = GetComponent<PlayerCharacter>();
    }

    void Update()
    {

        // If dialog is on, disable movement
        if (diagUI.dialogue.isLoaded && pc.GetActiveState() == true)
        {
            pc.SetActiveState(false);
        }
        // Otherwise allow movement to characters involved in discussion
        else if (!diagUI.dialogue.isLoaded && discussionPartner)
        {
            if (discussionPartner)
            {
                discussionPartner.SetAIBehaviourActive(!isControlledNPC);
                discussionPartner = null;
            }

            pc.SetActiveState(true);
        }

        // Interact with NPCs when hitting spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryInteract();
        }

        Debug.DrawRay(transform.position, transform.right * reachDistance * Mathf.Sign(transform.localScale.x), Color.red);

    }

    /// <summary>
    /// Casts a ray to see if we hit an NPC and, if so, we interact
    /// </summary>
    void TryInteract()
    {
        // Multi ray
        RaycastHit2D[] rHit = Physics2D.RaycastAll(transform.position, Vector2.right * Mathf.Sign(transform.localScale.x), reachDistance, -1);
        if (rHit.Length > 0)
        {
            foreach (RaycastHit2D hit in rHit)
            {
                if (hit.collider.tag == "NPC")
                {
                    // Check for a ghost object
                    if (hit.collider.name.StartsWith("Ghost"))
                    {
                        // Get the ghost's original from the character pair list
                        CharacterPair cp = FindObjectOfType<GhostManager>().characters.Find(c => c.Ghost == hit.collider.gameObject);
                        discussionPartner = cp.Original.GetComponent<NPC>();
                        discussionPartner.TurnTowards(transform, true);
                    }
                    else
                    {
                        discussionPartner = hit.collider.GetComponent<NPC>();
                        discussionPartner.TurnTowards(transform);
                    }

                    discussionPartner.SetAIBehaviourActive(false);

                    //Lets grab the NPC's DialogueAssign script...
                    VIDE_Assign assigned = discussionPartner.gameObject.GetComponent<VIDE_Assign>();

                    if (!diagUI.dialogue.isLoaded)
                    {
                        //... and use it to begin the conversation
                        diagUI.Begin(assigned);
                    }
                    else
                    {
                        //If conversation already began, let's just progress through it
                        diagUI.NextNode();
                    }

                    isControlledNPC = discussionPartner.name.Contains("Controlled");

                    // Break the loop so that only one conversation is active in case many NPC's got raycasted
                    break;
                }
            }
        }

        if (discussionPartner) diagUI.npcName.text = discussionPartner.name;
    }
}
