using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class DoorTrigger : MonoBehaviour, Trigger {

    public int loadLevel = 0;
    public bool smoothTransition = true;

    AsyncOperation async;

    /// <summary>
    /// Gets the level name prefix (level/testscene/floor) from the scene name,
    /// removes numbers from it and the loads the next level that has the same prefix.
    /// </summary>
    public void Activate()
    {

#if UNITY_5_3_OR_NEWER
        string levelNamePrefix = SceneManager.GetActiveScene().name;
        levelNamePrefix = Regex.Replace(levelNamePrefix, @"[\d-]", string.Empty);
        if (!smoothTransition)
            SceneManager.LoadScene(levelNamePrefix + loadLevel);
        else
            StartCoroutine(SmoothTransition(levelNamePrefix));
#else
        string levelNamePrefix = Application.loadedLevelName;
        levelNamePrefix = Regex.Replace(levelNamePrefix, @"[\d-]", string.Empty);
        if (!smoothTransition)
            Application.LoadLevel(levelNamePrefix + loadLevel);
        else
            StartCoroutine(SmoothTransition(levelNamePrefix));
#endif

    }

    /// <summary>
    /// Loads the scene asynchronously without activating it.
    /// </summary>
    /// <param name="namePrefix"></param>
    /// <returns></returns>
    IEnumerator SmoothTransition(string namePrefix)
    {
        Debug.LogWarning("Asynchronous scene loading started. Do not exit play mode until scene has loaded or Unity might crash.");

#if UNITY_5_3_OR_NEWER
        async = SceneManager.LoadSceneAsync(namePrefix + loadLevel);
#else
        async = Application.LoadLevelAsync(namePrefix + loadLevel);
#endif
        
        async.allowSceneActivation = false;
        yield return async;
    }

    /// <summary>
    /// Activates the scene that was loaded asynchronously.
    /// </summary>
    public void ActivateScene()
    {
        async.allowSceneActivation = true;
    }

    /// <summary>
    /// Instantly loads another scene with the same prefix name followed by a set number.
    /// </summary>
    public void LoadScene(int number = -1)
    {
        if (number > -1) loadLevel = number;

#if UNITY_5_3_OR_NEWER
        string levelNamePrefix = SceneManager.GetActiveScene().name;
        levelNamePrefix = Regex.Replace(levelNamePrefix, @"[\d-]", string.Empty);
        SceneManager.LoadScene(levelNamePrefix + loadLevel);
#else
        string levelNamePrefix = Application.loadedLevelName;
        levelNamePrefix = Regex.Replace(levelNamePrefix, @"[\d-]", string.Empty);
        Application.LoadLevel(levelNamePrefix + loadLevel);
#endif

    }
}
