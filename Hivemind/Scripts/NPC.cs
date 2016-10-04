using UnityEngine;
using UnityStandardAssets._2D;

public class NPC : MonoBehaviour {

    public bool enableSimpleAI = true;
    public float moveSpeed = 0.5f;

    public bool enableStateLogging = false;

    enum State
    {
        Infected,
        Idle,
        Wandering,
        Seeking
    }

    PlayerCharacter pc;
    PlatformerCharacter2D m_Character;
    State currentState = State.Idle;
    
    float walkDirection = 1;

    private void Awake()
    {
        m_Character = GetComponent<PlatformerCharacter2D>();
        pc = GetComponent<PlayerCharacter>();

        //InvokeRepeating("StateRandomization", 1, 1);
    }

    void Update()
    {
        /* Reminder about random chances.
        float randValue = Random.value;
        if (randValue < .45f) // 45% of the time
        {
            // Do Normal Attack 1
        }
        else if (randValue < .9f) // 45% of the time
        {
            // Do Normal Attack 2
        }
        else // 10% of the time
        {
            // Do Special Attack
        }
        */

        if (enableSimpleAI)
        {
            // If FPS=30, the chance of changing state is 6% per second.
            if (Random.value < .002f)
                SwitchState();
            
            switch (currentState)
            {
                case State.Idle:
                    m_Character.Move(0, false, false);
                    break;

                case State.Wandering:
                    m_Character.Move(moveSpeed * walkDirection, false, false);
                    break;

                case State.Seeking:
                    // Player seeking.
                    break;
            }
        }
    }

    void StateRandomization()
    {
        // The chance of changing state is 10% on every call.
        if (Random.value < .10f)
            SwitchState();
    }

    /// <summary>
    /// Randomizes the new state and possible walking direction.
    /// </summary>
    void SwitchState()
    {
        // Simple for now
        if (currentState == State.Idle)
        {
            currentState = State.Wandering;

            if (Random.value < 0.5f) walkDirection = 1;
            else walkDirection = -1;
        }
        else currentState = State.Idle;

        /* Random chance for all
        float randValue = Random.value;
        if (randValue < .75f) // 75% chance of idle
        {
            currentState = State.Idle;
        }
        else // 25% chance of walking
        {
            currentState = State.Wandering;
            if (Random.value < 0.5f) walkDirection = 1;
            else walkDirection = -1;
        }
        */

        // Logging code.
        if (!enableStateLogging) return;
        string npcAction = gameObject.name + " is now " + currentState.ToString();
        if (currentState == State.Wandering)
        {
            npcAction += " to ";
            if (walkDirection < 0) npcAction += "left";
            else npcAction += "right";
        }
        Debug.Log(npcAction);
    }

    /// <summary>
    /// Infects the NPC, who then becomes part of the hivemind.
    /// </summary>
    public void Infect()
    {
        // Guess this needs optimization.
        enableSimpleAI = false;
        m_Character.Move(0, false, false); // Changes the animation state to idle.
        tag = "Player";
        name = "Infected " + gameObject.name;
        pc.enabled = true;
        pc.SetActiveState(false);
        transform.parent = GameObject.Find("HIVEMIND").transform;
        pc.userInterface = transform.parent.GetComponent<AdvancedHivemind>().ui;
        //transform.parent.GetComponent<Hivemind>().characters.Add(gameObject);
        enabled = false;

        FindObjectOfType<AdvancedHivemind>().AddCharacter(gameObject);
    }

    /// <summary>
    /// Activates/Deactivates AI behaviour.
    /// </summary>
    /// <param name="active">Set active.</param>
    public void SetAIBehaviourActive(bool active)
    {
        enableSimpleAI = active;
        if (!active)
        {
            currentState = State.Idle;
            GetComponent<PlatformerCharacter2D>().Move(0, false, false);
        }
    }

    /// <summary>
    /// Turns the NPC to face a target.
    /// </summary>
    /// <param name="target">Target to face.</param>
    /// <param name="flipped">If turning a ghost on the other side of the map.</param>
    public void TurnTowards(Transform target, bool flipped = false)
    {
        Vector3 ls = transform.localScale;
        float side = transform.position.x - target.position.x;

        if (!flipped)
        {
            if (Mathf.Sign(ls.x) == Mathf.Sign(side))
                transform.localScale = new Vector3(ls.x * -1, ls.y, ls.z);
        }
        else
        {
            if (Mathf.Sign(ls.x) == Mathf.Sign(target.position.x))
                transform.localScale = new Vector3(ls.x * -1, ls.y, ls.z);
        }
        
    }
}
