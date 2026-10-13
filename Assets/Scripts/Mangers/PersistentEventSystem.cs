using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to the "EventSystem" object in your FIRST scene only
/// (MainMenu). Makes it survive scene loads instead of each new scene
/// creating its own fresh EventSystem + Input Module - which was causing
/// clicks to be unresponsive right after a scene load until some other
/// input (like moving the stick) "woke up" the newly created module.
///
/// Going BACK to MainMenu loads MainMenu's own EventSystem again; that copy
/// is switched off and removed immediately, and the original is made the
/// active EventSystem again so clicks keep working.
///
/// IMPORTANT: delete the EventSystem object from every OTHER scene
/// (BuildingMap, Tour360, SplashScreen, etc.) - having two EventSystems
/// active at once causes a conflict.
/// </summary>
[DefaultExecutionOrder(-10000)]
public class PersistentEventSystem : MonoBehaviour
{
    public static PersistentEventSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);

            EventSystem original = Instance.GetComponent<EventSystem>();
            if (original != null) EventSystem.current = original;
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
