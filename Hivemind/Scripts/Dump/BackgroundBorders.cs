using UnityEngine;
using System.Collections;

public class BackgroundBorders : MonoBehaviour {

	public float distanceFromCorners = 1.25f;

	// Use this for initialization
	void Start () {
		float width = transform.localScale.x;
		
		width = width / 2;
		
		transform.GetChild(0).localPosition = new Vector3(transform.position.x + width - distanceFromCorners, -1.37f, 0);
		transform.GetChild(1).localPosition = new Vector3(transform.position.x - width + distanceFromCorners, -1.37f, 0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
