// CrazyGames integration preserved; excluded from the default GamePix build.
#if TURBODASH_CRAZYGAMES
using CrazyGames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrazySDKLoadingScene : MonoBehaviour
{
    public string nextSceneName;

    void Start()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Please provide a nextSceneName in " + gameObject.name);
            return;
        }

        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Init(() =>
            {
                Debug.Log("CrazySDK initialized");
                SceneManager.LoadScene(nextSceneName);
            });
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}

#else
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Kept under its original name/GUID so the existing LoadingScene remains wired.
public class CrazySDKLoadingScene : MonoBehaviour
{
    public string nextSceneName;
    private IEnumerator Start()
    {
        if (string.IsNullOrEmpty(nextSceneName)) { Debug.LogError("Set the next scene name."); yield break; }
        yield return SceneManager.LoadSceneAsync(nextSceneName);
    }
}
#endif
