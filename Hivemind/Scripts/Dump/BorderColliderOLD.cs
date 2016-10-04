using UnityEngine;

public class BorderColliderOLD : MonoBehaviour {

    GameObject character;
    GameObject leftB;
    GameObject rightB;
    Rigidbody2D rigidBody;

    void Start()
    {
        //character = transform.parent.gameObject;
        rigidBody = transform.GetComponent<Rigidbody2D>();
        leftB = GameObject.Find("LeftBorder");
        rightB = GameObject.Find("RightBorder");
    }

    void OnTriggerExit2D(Collider2D col)
    {
        Debug.Log("Collider exit, position: " + transform.position);
        if (col.name == "RightBorder")
        {
            float difference = rigidBody.position.x - col.transform.position.x;
            if (difference < 0)
                rigidBody.position = new Vector3(leftB.transform.position.x - difference, rigidBody.position.y);
            else//if (difference > 0)
                rigidBody.position = new Vector3(leftB.transform.position.x + difference, rigidBody.position.y);
        }
        if (col.name == "LeftBorder")
        {
            float difference = rigidBody.position.x - col.transform.position.x;
            if (difference < 0)
                rigidBody.position = new Vector3(rightB.transform.position.x + difference, rigidBody.position.y);
            else//if (difference > 0)
                rigidBody.position = new Vector3(rightB.transform.position.x - difference, rigidBody.position.y);
        }
    }
}
