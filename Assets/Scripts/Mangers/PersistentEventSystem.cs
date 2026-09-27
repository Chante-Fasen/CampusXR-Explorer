using UnityEngine;

/// <summary>
/// Attach this to the "EventSystem" object in your FIRST scene only
/// (MainMenu). Makes it survive scene loads instead of each new scene
/// creating its own fresh EventSystem + Input Module - which was causing
/// clicks to be unresponsive right after a scene load until some other
/// input (like moving the stick) "woke up" the newly created module.
///
/// IMPORTANT: delete the EventSystem object from every OTHER scene
/// (BuildingMap, Tour360, SplashScreen, etc.) once this is in place -
/// having two EventSystems active at once causes a conflict.
/// </summary>
public class PersistentEventSystem : MonoBehaviour
{
    public static PersistentEventSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
