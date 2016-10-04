using UnityEngine;

/// <summary>
/// Handles the moving of player from border to border.
/// </summary>
public class BorderColliderMap : MonoBehaviour
{
    public GameObject otherBorder;

    Vector2 cameraPos;
    
    void OnTriggerExit2D(Collider2D col)
    {
        float difference = col.transform.position.x - transform.position.x;

        // Disable the rigidbody before changing position to prevent weird movement
        Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
        Vector2 velocity = rb.velocity;
        rb.Sleep();
        rb.gameObject.SetActive(false);
        
        // Move the object to the other border
        Vector2 newPosition = new Vector2(otherBorder.transform.position.x + difference, col.transform.position.y);
        col.transform.position = newPosition;

        // Enable the rigidbody with the previous velocity
        rb.gameObject.SetActive(true);
        rb.WakeUp();
        rb.velocity = velocity;
    }
}
