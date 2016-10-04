using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CharacterPair
{
    [HideInInspector] public string Name;
    public GameObject Original;
    public GameObject Ghost;
    [HideInInspector] public SpriteRenderer OriginalSR;
    [HideInInspector] public SpriteRenderer GhostSR;
}

public class GhostManager : MonoBehaviour {
    
    public GameObject background;
    public List<CharacterPair> characters = new List<CharacterPair>();
    
    float bgWidth;
    
	void Start ()
    {

        // Get width of the level's background
        bgWidth = background.GetComponent<BackgroundGenerator>().GetBackgroundWidth();

        // Create parent object for ghost objects
        GameObject ghosts = new GameObject("Ghosts");
        ghosts.transform.position = Vector3.zero;

        // Get all objects tagged as "NPC"
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("NPC"))
        {
            // Create ghost object from the original, send it one map width away in x-axis & remove its scripts, childs, rigidbody and animator
            GameObject ghost = (GameObject)Instantiate(go, new Vector2(bgWidth, go.transform.position.y), Quaternion.identity);
            ghost.transform.parent = ghosts.transform;
            ghost.GetComponents(typeof(MonoBehaviour)).ToList().ForEach(s => Destroy(s));
            ghost.GetComponentsInChildren(typeof(Transform)).ToList().Where(c => c.gameObject != ghost).ToList().ForEach(t => Destroy(t.gameObject));
            Destroy(ghost.GetComponent<CircleCollider2D>());
            Destroy(ghost.GetComponent<Rigidbody2D>());
            Destroy(ghost.GetComponent<Animator>());
            ghost.name = "Ghost " + go.name;

            // Add the original-ghost pair to list
            characters.Add(
                new CharacterPair {
                    Name = go.name,
                    Original = go,
                    Ghost = ghost,
                    OriginalSR = go.GetComponent<SpriteRenderer>(),
                    GhostSR = ghost.GetComponent<SpriteRenderer>()
                }
            );
        }

	}
    
    void Update () {
        
        foreach (CharacterPair character in characters)
        {
            // Sets the x position of the ghost object to the opposite side of the map from the original depending on which side of the x-axis the original currently is
            if (Mathf.Sign(character.Original.transform.position.x) > 0)
            {
                character.Ghost.transform.position = new Vector2(character.Original.transform.position.x - bgWidth, character.Original.transform.position.y);
            }
            else
            {
                character.Ghost.transform.position = new Vector2(character.Original.transform.position.x + bgWidth, character.Original.transform.position.y);
            }
            
            // Update the ghost's sprite to match the original's sprite
            character.GhostSR.sprite = character.OriginalSR.sprite;

            // Update the ghost's local scale to match the original's scale (used to turn the object)
            character.Ghost.transform.localScale = new Vector3(character.Original.transform.localScale.x, character.Original.transform.localScale.y, character.Original.transform.localScale.z);
        }

    }
}
