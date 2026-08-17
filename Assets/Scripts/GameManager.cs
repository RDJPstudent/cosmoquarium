using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameManager
{
    public static int totalGold = 0;
    public static int currentNight = 1; // starts at 1 for the player's first night in the Aquarium

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        totalGold = 0;
        currentNight = 1;

        // Unsubscribe first in case this somehow runs more than once, to avoid double-subscribing
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // Automatically called by Unity every time ANY scene finishes unloading -
    // i.e. the moment the player leaves that scene for another one
    private static void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "Aquarium")
        {
            currentNight += 1;
            Debug.Log($"[GameManager] Left Aquarium - now Night {currentNight}");
        }
    }

    public static void AddGold(int amount)
    {
        totalGold += amount;
        Debug.Log($"[GameManager] Gold added: +{amount}. Total gold: {totalGold}");
    }
}