using UnityEngine;
using System.Collections;

public class SporeCloud : MonoBehaviour {

    public float reachRadius = 0.5f;

    ParticleSystem ps;
    
	void Start () {
        ps = GetComponent<ParticleSystem>();
	}
	
	void FixedUpdate () {
        // Infects NPC's withing certain radius.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, reachRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag == "NPC" && !colliders[i].name.Contains("Ghost"))
            {
                Debug.Log("Infected");
                colliders[i].GetComponent<NPC>().Infect();
            }
        }
        
        // Destroys the cloud when last particle dies.
        if (ps)
        {
            if (!ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
    /*
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "NPC")
        {
            Debug.Log("Hit");
            col.gameObject.GetComponent<NPC>().Turn();
        }
    }
    */

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, reachRadius);
    }
}
