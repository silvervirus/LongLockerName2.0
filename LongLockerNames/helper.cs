using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class CoroutineStarter : MonoBehaviour
{
    private static CoroutineStarter instance;

    private void Awake()
    {
        // Ensure there's only one instance of CoroutineStarter
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void StartCoroutineStatic(IEnumerator coroutine)
    {
        if (instance != null)
        {
            instance.StartCoroutine(coroutine);
        }
        else
        {
            Debug.LogError("CoroutineStarter instance is null.");
        }
    }
}