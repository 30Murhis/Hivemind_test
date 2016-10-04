using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

    CharacterMovement _characterMovement;
    bool jump;

	// Use this for initialization
	void Start () {
        _characterMovement = GetComponent<CharacterMovement>();
	}

    void Update()
    {
        jump = Input.GetButton("Jump");
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        bool crouch = Input.GetKey(KeyCode.LeftControl);
        float h = Input.GetAxis("Horizontal");
        bool run = Input.GetButton("Run");
        _characterMovement.Move(h, run, crouch, jump);
        jump = false;
	}
}
