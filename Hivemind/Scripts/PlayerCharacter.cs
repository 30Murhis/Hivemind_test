using System.Collections;
using UnityEngine;
using UnityStandardAssets._2D;

/// <summary>
/// Own player character stuff.
/// </summary>
public class PlayerCharacter : MonoBehaviour {

    public GameObject projectile;
	public GameObject userInterface;

	GameObject triggerIndicator;
	bool inTrigger = false;
	Collider2D inCollider;

	GameObject shot;
    Rigidbody2D shotRig;
    Vector2 pos;

    Vector2 transitionTarget;
    Rigidbody2D rb;
    public float transitionSpeed = 0.1f;

    bool isCharacterActive = false;

	void Start () {
        if (userInterface == null)
            userInterface = GameObject.Find("UI");

        triggerIndicator = userInterface.transform.FindChild("TriggerIndicator").gameObject;
        rb = GetComponent<Rigidbody2D>();
    }

	void Update () {
        // If this character is the currently selected in the hivemind.
        if (isCharacterActive)
        {
            // Left click (shoot spore)
	        if (Input.GetButton("Fire1") && shot == null)
		    {
                Shoot();
            }

            // E (default activate key), no console key yet
            if (Input.GetKey(KeyCode.E) && inTrigger)
		    {
			    GameObject.Find("DebugDisplay").GetComponent<DebugDisplay>().SetText("'E' Clicked");
			    inCollider.GetComponent<Trigger>().Activate();
                if (inCollider.name.Contains("DoorTrigger") && inCollider.GetComponent<DoorTrigger>().smoothTransition == true)
                {
                    if (inCollider.GetComponent<DoorTrigger>().smoothTransition == true)
                    {
                        isCharacterActive = false;
                        transitionTarget = new Vector2(rb.position.x, rb.position.y - 1);
                        GetComponent<Platformer2DUserControl>().enabled = false;
                        GetComponent<SpriteRenderer>().sortingOrder = 20;
                        StartCoroutine(LevelTransition());
                    }
                    else
                    {
                        inCollider.GetComponent<DoorTrigger>().LoadScene();
                    }
                }
            }

            // Up (climb, go up in levels), no console key yet
            if (Input.GetKey(KeyCode.W))
            {
            
            }

            // Down (go down in levels), no console key yet
            if (Input.GetKey(KeyCode.S))
            {

            }
        }
    }

    /// <summary>
    /// Shoots the projectile towards the mouse.
    /// </summary>
    /// <param name="direction"></param>
    void Shoot()
	{
        // Gets mouse position from screen.
		Vector2 target = Camera.main.ScreenToWorldPoint( new Vector2(Input.mousePosition.x, Input.mousePosition.y) );
		Vector2 myPos = new Vector2(transform.position.x,transform.position.y);

        // Creates the projectile and sets its direction towards the mouse position.
        shot = (GameObject)Instantiate(projectile, transform.position, Quaternion.identity);
		shot.GetComponent<SporeShot>().SetDirection(target - myPos);
    }

	void OnTriggerEnter2D(Collider2D col)
    {
        // Trigger activation
		if (col.name.Contains("Trigger") && userInterface != null && triggerIndicator != null)
        {
			inCollider = col;
			triggerIndicator.SetActive(true);
			inTrigger = true;
		}
    }
	
	void OnTriggerExit2D(Collider2D col)
    {
        // Trigger deactivation
        if (col.name.Contains("Trigger") && userInterface != null && triggerIndicator != null)
        {
			inCollider = null;
			triggerIndicator.SetActive(false);
			inTrigger = false;
		}
    }

    /// <summary>
    /// Sets active state based on hivemind's currently chosen character.
    /// </summary>
    /// <param name="state"></param>
    public void SetActiveState(bool state)
    {
        isCharacterActive = state;
        GetComponent<Platformer2DUserControl>().enabled = state;
        if (!state) GetComponent<PlatformerCharacter2D>().Move(0, false, false);
    }

    /// <summary>
    /// Gets active state based on hivemind's currently chosen character.
    /// </summary>
    /// <returns></returns>
    public bool GetActiveState()
    {
        return isCharacterActive;
    }

    /// <summary>
    /// Smooth level transition movement.
    /// <para>Moves the character downwards while displaying walking animation.</para>
    /// </summary>
    /// <returns></returns>
    IEnumerator LevelTransition()
    {
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        while (rb.position.y > transitionTarget.y)
        {
            GetComponent<Animator>().SetFloat("Speed", 1);
            rb.MovePosition(rb.position + Vector2.down * transitionSpeed * Time.deltaTime);
            yield return null;
        }
        
        inCollider.GetComponent<DoorTrigger>().ActivateScene();
        yield break;
    }
}
