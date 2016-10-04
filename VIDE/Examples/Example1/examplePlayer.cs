using UnityEngine;
using System.Collections;

public class examplePlayer : MonoBehaviour
{
    //This script handles player movement and interaction with other NPC game objects

    //Reference to our diagUI script for quick access
    public exampleUI diagUI;

    void Update()
    {

        //Only allow player to move and turn if there are no dialogs loaded
        if (!diagUI.dialogue.isLoaded)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * 5, 0);
            float move = Input.GetAxisRaw("Vertical");
            transform.position += transform.forward * 5 * move * Time.deltaTime;
        }
        //Interact with NPCs when hitting spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryInteract();
        }
    }

    //Casts a ray to see if we hit an NPC and, if so, we interact
    void TryInteract()
    {
        RaycastHit rHit;

        if (Physics.Raycast(transform.position, transform.forward, out rHit, 2))
        {
            //In this example, any Gameobject name containing 'NPC' is considered an NPC
            if (rHit.collider.name.Contains("NPC"))
            {
                //Lets grab the NPC's DialogueAssign script...
                VIDE_Assign assigned = rHit.collider.GetComponent<VIDE_Assign>();

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
            }
        }
    }
}
