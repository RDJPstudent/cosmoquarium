using UnityEngine;

public static class GameManager
{
    public static int totalGold = 0;

    // RuntimeInitializeOnLoadMethod makes Unity call this automatically every time
    // Play Mode starts (or a build launches), regardless of domain reload settings -
    // guarantees totalGold is always 0 at the start of a session.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnPlay()
    {
        totalGold = 0;
    }

    public static void AddGold(int amount)
    {
        totalGold += amount;
        Debug.Log($"[GameManager] Gold added: +{amount}. Total gold: {totalGold}");
    }
}