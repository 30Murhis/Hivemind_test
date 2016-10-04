using UnityEngine;

public class CustomCamera : MonoBehaviour {

    public GameObject bg;
    public Transform target;
    public float offsetX = 0;
    public float offsetY = 0;

    public Transform supportCamera;

    float originalZ;
    public float width;

	void Start () {
        originalZ = transform.position.z;

        // If support camera not set, tries to get the child
        if (!supportCamera) supportCamera = transform.GetChild(0);

        // If background object not set, finds one
        if (bg == null) bg = GameObject.Find("Background");

        // Get width of the background
        width = bg.GetComponent<BackgroundGenerator>().GetBackgroundWidth();

        // Set the position of support camera
        supportCamera.localPosition = new Vector3(width, supportCamera.localPosition.y, supportCamera.localPosition.z);
    }

	void LateUpdate () {

        // Sets the main camera's position to target's position
        transform.position = new Vector3(target.transform.position.x - offsetX, target.transform.position.y + offsetY, originalZ);

        // Sets the support camera's x-position to the opposite side of the map.
        if (Mathf.Sign(transform.position.x) == Mathf.Sign(supportCamera.localPosition.x))
        {
            supportCamera.localPosition = new Vector3(supportCamera.localPosition.x * -1, supportCamera.localPosition.y, supportCamera.localPosition.z);
        }
    }
}
