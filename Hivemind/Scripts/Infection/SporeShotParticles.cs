using UnityEngine;
using System.Collections;

public class SporeShotParticles : MonoBehaviour {

    ParticleSystem ps;

	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
        if (transform.parent == null)
        {
            ps.loop = false;
            if (!ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
