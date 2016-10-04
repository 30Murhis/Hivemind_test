using UnityEngine;
using System.Collections;

public class RandomTrigger : MonoBehaviour, Trigger {

	public void Activate() {
		GameObject.Find ("DebugDisplay").GetComponent<DebugDisplay>().AddText("Test Trigger Activated");
	}

}
