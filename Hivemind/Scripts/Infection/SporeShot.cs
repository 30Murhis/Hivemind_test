using UnityEngine;

public class SporeShot : MonoBehaviour {

    public float speed = 2.0f;
    public float lifeTime = 3.0f;
    public GameObject sporeCloud;

	Vector2 direction;

    void Start () {
        // Spawns cloud if wanted, and destroys object always.
        if (sporeCloud != null) Invoke("SpawnCloud", lifeTime);
        else Destroy(gameObject, lifeTime);
	}

	public void SetDirection(Vector3 direction) {
		GetComponent<Rigidbody2D>().velocity = direction * speed;
	}

    void SpawnCloud()
    {
        Instantiate(sporeCloud, transform.position, Quaternion.identity);
        transform.GetChild(0).SetParent(null);
        Destroy(gameObject);
    }

    /* Old test code.
    void ReverseScale()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void SetHorizontalDirection(Vector3 vector)
    {
        if ((vector.x < 0 && speed > 0) || (vector.x > 0 && speed < 0))
            speed *= -1;

        if (speed < 0)
        {
            ReverseScale();
        }
    }

    public void SetHorizontalDirection(float direction)
    {
        if ((direction < 0 && speed > 0) || (direction > 0 && speed < 0))
            speed *= -1;

        if (speed < 0)
        {
            ReverseScale();
        }
    }
    */
}
