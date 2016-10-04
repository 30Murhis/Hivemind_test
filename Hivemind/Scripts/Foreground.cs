using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Foreground transparency changer.
/// </summary>
public class Foreground : MonoBehaviour {

    public float revealRadius = 2.0f;
    public float xOffset = 0.0f;

    List<SpriteRenderer> childSprites = new List<SpriteRenderer>();

    void Start()
    {
        // Fills the list with direct childs.
        foreach (Transform transf in gameObject.transform)
        {
            childSprites.Add(transf.GetComponent<SpriteRenderer>());
        }
    }
    
	void Update () {

        if (FindObjectOfType<Cameras>().lockedToTarget)
        {

            // Gets the distance between the character and the foreground door. xOffset = 0 if the door is in the center of the sprite.
            float distance = Camera.main.transform.position.x - transform.GetChild(0).position.x + xOffset;
            distance = Mathf.Abs(distance);

            // Changes the opacity of the foreground based on distance.
            foreach (SpriteRenderer sr in childSprites)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1 - distance / revealRadius);
            }

        }
	}
}
