using UnityEngine;

public class BorderStopCamera : MonoBehaviour
{
    public GameObject background;
    public Transform target;

    public Transform mainCamera;

    public float offsetX = 0;
    public float offsetY = 0;

    float offsetZ = -10;
    float width;

    float mainHeight;
    float mainWidth;

    void Start()
    {
        // If main camera not set, gets it
        if (!mainCamera) mainCamera = Camera.main.transform;

        // If background object not set, finds one
        if (background == null) background = GameObject.Find("Background");

        // Setting the support camera distance from the main camera based on background's width.
        for (int i = 0; i < background.transform.childCount; i++)
        {
            if (background.transform.GetChild(i).name.Contains("Background"))
                width += background.transform.GetChild(i).GetComponent<SpriteRenderer>().bounds.size.x;
        }

        // Setting the main camera to center
        mainCamera.localPosition = new Vector3(0, mainCamera.localPosition.y, offsetZ);

        mainHeight = 2f * mainCamera.GetComponent<Camera>().orthographicSize;
        mainWidth = mainHeight * mainCamera.GetComponent<Camera>().aspect;
    }

    void LateUpdate()
    {
        // Moves camera if it's borders are withing the level boundaries
        if ((target.position.x + (mainWidth / 2) < (width / 2)) && (target.position.x - (mainWidth / 2) > -(width / 2)))
        {
            transform.position = new Vector3(target.transform.position.x + offsetX, target.transform.position.y + offsetY, 0);
        }
        else
        {
            // Sets the support camera's x-position to the opposite side of the map if needed
            if (Mathf.Sign(transform.position.x) != Mathf.Sign(target.position.x))
            {
                transform.position = new Vector3(transform.position.x * -1, transform.position.y, offsetZ);
            }
        }
    }
}
