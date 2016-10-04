using UnityEngine;
using System.Collections;

public class Background : MonoBehaviour {

	public GameObject background;
	public GameObject[] backgrounds = new GameObject[2];

	//float backgroundWidth;

	// Use this for initialization
	void Start () {
		GetBackgrounds();
		//backgroundWidth = backgrounds[0].transform.localScale.x;
		backgrounds[1].SetActive(false);
	}
	
	// Update is called once per frame
	void GetBackgrounds () {
		backgrounds[0] = (GameObject)Instantiate (background, background.transform.position, Quaternion.identity);
		backgrounds[1] = (GameObject)Instantiate (background, background.transform.position, Quaternion.identity);
	}

	public void ExpandBackground(string direction) {

		if (direction == "Right") {

		}
		if (direction == "Left") {

		}
	}
}
