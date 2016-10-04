using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugDisplay : MonoBehaviour {

	Text debugText;
	float displayTime = 5.0f;

	void Start () {
		debugText = transform.GetComponent<Text>();
	}
	
	void Hide() {
		debugText.text = "";
	}

	/// <summary>
	/// Displays a text in the debug display.
	/// </summary>
	/// <param name="text">Text.</param>
	public void SetText(string text) {
		CancelInvoke();

		debugText.text = text;

		Invoke ("Hide", displayTime);
	}
	
	/// <summary>
	/// Adds a text to the debug display.
	/// </summary>
	/// <param name="text">Text.</param>
	public void AddText(string text, bool newLine = true) {
		CancelInvoke();

		if (newLine) debugText.text += "\n";
		debugText.text += text;
		
		Invoke ("Hide", displayTime);
	}

	/// <summary>
	/// Sets the time, that determines how long debug text is displayed.
	/// </summary>
	/// <param name="text">Text.</param>
	public void SetDisplayTime(float time) {
		this.displayTime = time;
	}
}
