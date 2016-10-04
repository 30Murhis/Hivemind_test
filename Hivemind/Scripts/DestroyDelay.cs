using UnityEngine;
using System.Collections;

public class DestroyDelay : MonoBehaviour {

    public float delay = 5.0f;

	// Use this for initialization
	void Start () {
        Destroy(gameObject, delay);
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}
