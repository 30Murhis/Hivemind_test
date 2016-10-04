using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generates a background from chosen gameobjects.
/// <para>Width is defined with chunks. Chunks needs to be uneven number right now.</para>
/// <para>First 2 childs need to be border colliders used with world wrapping.</para>
/// <para>The 3rd child is the ground object and the 4th is a killzone.</para>
/// </summary>
public class BackgroundGenerator : MonoBehaviour {

    public int chunks = 5;
    public bool randomOrder = true;
    public float borderOffset = 0;
    public float borderColliderOffset = 10.0f;
    public GameObject[] backgrounds;
    public GameObject floor;
    
    bool generateFloors = false;
    float totalWidth = 0;

    List<int> availableBackgrounds;
    
	void Awake () {
        if (transform.childCount < 5) GenerateBackground();
    }
	
	/// <summary>
    /// Generates a background.
    /// </summary>
    public void GenerateBackground()
    {
        if (transform.childCount > 4) RemoveBackground();

        NewAvailableBackgroundsList();

        int counter = 0;
        float height = 0;
        float nextPos = 0;
        float totalWidth = 0;
        if (floor) generateFloors = true;

        for (int i = 0; i < chunks; i++)
        {
            GameObject go;

            if (!randomOrder)
            {
                counter++;
                if (counter == backgrounds.Length) counter = 0;
                go = (GameObject)Instantiate(backgrounds[counter], Vector3.zero, Quaternion.identity);
            }
            else
            {
                // Completely random, possible repeating
                // go = (GameObject)Instantiate(backgrounds[(int)Random.Range(0f, (chunks - 1))], Vector3.zero, Quaternion.identity);

                // Shuffled random, no repeating until all backgrounds have been used once
                if (availableBackgrounds.Count <= 0)
                    NewAvailableBackgroundsList();

                int r = availableBackgrounds[Random.Range(0, availableBackgrounds.Count)];
                go = (GameObject)Instantiate(backgrounds[r], Vector3.zero, Quaternion.identity);
                availableBackgrounds.Remove(r);
            }

            go.transform.parent = transform;
            go.name = "Background-" + i.ToString();

            float width = go.GetComponent<SpriteRenderer>().bounds.size.x;
            height = go.GetComponent<SpriteRenderer>().bounds.size.y;

            if (i == 0)
            {
                go.transform.localPosition = new Vector3(0, 0);
            }
            else if (i % 2 == 0)
            {
                nextPos *= -1;
            }
            else
            {
                nextPos *= -1;
                nextPos += width;
            }

            go.transform.localPosition = new Vector2(nextPos, 0);

            totalWidth += width;

            // Floor generation
            if (generateFloors)
            {
                GameObject gof = (GameObject)Instantiate(floor, Vector3.zero, Quaternion.identity);
                gof.transform.parent = transform;
                gof.name = "Floor-" + i.ToString();
                gof.transform.localPosition = new Vector3(go.transform.localPosition.x, 0);
            }
        }

        totalWidth /= 2;

        // Right border collider
        Transform child = transform.GetChild(0);
        child.localPosition = new Vector2(totalWidth + borderOffset, 0);
        child.GetComponent<Collider2D>().offset = new Vector2(-borderColliderOffset, 0);
        child.name = "RightBorder";

        // Left border collider
        child = transform.GetChild(1);
        child.localPosition = new Vector2(-totalWidth - borderOffset, 0);
        child.GetComponent<Collider2D>().offset = new Vector2(borderColliderOffset, 0);
        child.name = "LeftBorder";

        totalWidth *= 2;

        // Ground
        child = transform.GetChild(2);
        child.localPosition = new Vector2(0, -height / 3);
        child.localScale = new Vector3(totalWidth + 5, 1);

        // Killzone
        child = transform.GetChild(3);
        child.localPosition = new Vector2(0, -height / 2);
        child.localScale = new Vector3(totalWidth + 50, 2);
    }

    /// <summary>
    /// Deletes a generated background, if one is found.
    /// </summary>
    public void RemoveBackground()
    {
        for (int i = transform.childCount - 1; i > 0; i--)
        {
            if (transform.GetChild(i).name.Contains("Background") || transform.GetChild(i).name.Contains("Floor"))
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// Generates a new list of numbers from backrounds array.
    /// </summary>
    void NewAvailableBackgroundsList()
    {
        availableBackgrounds = new List<int>();
        for (int i = 0; i < backgrounds.Length; i++)
        {
            availableBackgrounds.Add(i);
        }
    }

    /// <summary>
    /// Returns the total width of the generated background.
    /// </summary>
    public float GetBackgroundWidth()
    {
        if (totalWidth == 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.Contains("Background"))
                    totalWidth += transform.GetChild(i).GetComponent<SpriteRenderer>().bounds.size.x;
            }
        }

        return totalWidth;
    }
}
