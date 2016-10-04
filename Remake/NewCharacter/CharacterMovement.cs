using UnityEngine;
using System.Collections;

public class CharacterMovement : MonoBehaviour {

    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float runMultiplier = 2.0f;
    [SerializeField] bool allowJump = true;
    [SerializeField] float jumpForce = 200.0f;
    [Range(0, 1)] [SerializeField] float crouchSpeed = .4f;
    [SerializeField] LayerMask m_WhatIsGround;

    Rigidbody2D _rigidBody2D;
    Animator _animator;

    Transform _ceilingCheck;
    Transform _groundCheck;

    bool _grounded;
    bool _facingRight = true;

    const float _ceilingRadius = .1f;
    const float _groundedRadius = .2f;

    void Start()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _groundCheck = transform.Find("GroundCheck");
        _ceilingCheck = transform.Find("CeilingCheck");
    }

    private void FixedUpdate()
    {
        _grounded = false;
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_groundCheck.position, _groundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
                _grounded = true;
        }
        _animator.SetBool("Ground", _grounded);
        
        _animator.SetFloat("vSpeed", _rigidBody2D.velocity.y);
    }

    public void Move(float move, bool run, bool crouch, bool jump)
    {
        if (!crouch && _animator.GetBool("Crouch"))
            if (Physics2D.OverlapCircle(_ceilingCheck.position, _ceilingRadius, m_WhatIsGround))
                crouch = true;
        _animator.SetBool("Crouch", crouch);

        if (_grounded) // || allowAirControl)
        {
            move = (crouch ? move * crouchSpeed : move);

            move = (run ? move * runMultiplier : move);

            _animator.SetBool("Run", run);
            
            _animator.SetFloat("Speed", Mathf.Abs(move));

            // Move the character
            _rigidBody2D.velocity = new Vector2(move * moveSpeed, _rigidBody2D.velocity.y);

            if (move != 0)
            {
                //if (transform.localScale.x > 0) _facingRight = true;
                //else _facingRight = false;
            }
            
            if (move > 0 && !_facingRight)
                Flip();
            else if (move < 0 && _facingRight)
                Flip();
        }
    }
    
     void Flip()
    {
        _facingRight = !_facingRight;
        
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

}
