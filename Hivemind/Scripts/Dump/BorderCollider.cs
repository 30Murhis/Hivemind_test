using UnityEngine;

/// <summary>
/// Handles the moving of player from border to border.
/// </summary>
public class BorderCollider : MonoBehaviour {
    
    //GameObject leftBorder;
    //GameObject rightBorder;

    //bool outOfTrigger = true;

    void Start()
    {
        //character = transform.parent.gameObject;
        //leftBorder = GameObject.Find("LeftBorder");
        //rightBorder = GameObject.Find("RightBorder");
    }
    void OnTriggerEnter2D(Collider2D col)
    {

        if (col.name == "RightBorder")
        {
            //float difference = transform.position.x - col.transform.position.x;
            //if (difference < 0)
            //  MoveCharacterTo(leftBorder.transform.position, difference);
        }
        if (col.name == "LeftBorder")
        {
            //float difference = transform.position.x - col.transform.position.x;
            //if (difference > 0)
            //  MoveCharacterTo(rightBorder.transform.position, difference);
        }
        
    }

    /// <summary>
    /// Gets the distance between player and border collider, and warps the player to the other border.
    /// </summary>
    /// <param name="col"></param>
    /*
    void OnTriggerExit2D(Collider2D col)
    {
        if (!outOfTrigger) return;

        if (col.name == "RightBorder")
        {
            float difference = transform.position.x - col.transform.position.x;
            if (difference > 0)
                MoveCharacterTo(leftBorder.transform.position, difference);
        }
        if (col.name == "LeftBorder")
        {
            float difference = transform.position.x - col.transform.position.x;
            if (difference < 0)
                MoveCharacterTo(rightBorder.transform.position, difference);
        }

        outOfTrigger = true;
        Debug.Log("Trigger Exit");
    }
    */
    void MoveCharacterTo(Vector3 position, float offset)
    {
        transform.GetComponent<Rigidbody2D>().position = new Vector3(position.x + offset, transform.position.y, transform.position.z);

        Debug.Log("Object warped");
    }
}
