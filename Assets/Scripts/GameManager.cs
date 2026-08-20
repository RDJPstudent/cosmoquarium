using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class GameManager
{
    public static int totalGold = 0;
    public static int currentNight = 1;

    // Tracks owned upgrades by ID -> count. Shop purchases call AddUpgrade,
    // and Upgrade.cs calls RemoveUpgrade the moment a fish actually eats one.
    public static Dictionary<string, int> ownedUpgrades = new Dictionary<string, int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        totalGold = 0;
        currentNight = 1;
        ownedUpgrades.Clear();

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "Aquarium")
        {
            currentNight += 1;
            Debug.Log($"[GameManager] Left Aquarium - now Night {currentNight}");
        }
    }

    public static bool TrySpendGold(int amount)
{
    if (amount <= 0 || totalGold < amount)
    {
        return false;
    }

    totalGold -= amount;
    return true;
}

    public static void AddGold(int amount)
    {
        totalGold += amount;
        Debug.Log($"[GameManager] Gold added: +{amount}. Total gold: {totalGold}");
    }

    // Called by the shop when the player purchases an upgrade
    public static void AddUpgrade(string upgradeId, int amount = 1)
    {
        if (ownedUpgrades.ContainsKey(upgradeId))
        {
            ownedUpgrades[upgradeId] += amount;
        }
        else
        {
            ownedUpgrades[upgradeId] = amount;
        }

        Debug.Log($"[GameManager] Added upgrade '{upgradeId}'. Now own: {ownedUpgrades[upgradeId]}");
    }

    // Called by Upgrade.cs when a fish successfully eats an upgrade instance.
    // Returns true if removal succeeded (i.e. the player actually owned one).
    public static bool RemoveUpgrade(string upgradeId, int amount = 1)
    {
        if (ownedUpgrades.ContainsKey(upgradeId) && ownedUpgrades[upgradeId] >= amount)
        {
            ownedUpgrades[upgradeId] -= amount;

            if (ownedUpgrades[upgradeId] <= 0)
            {
                ownedUpgrades.Remove(upgradeId);
            }

            Debug.Log($"[GameManager] Removed upgrade '{upgradeId}' (eaten by fish).");
            return true;
        }

        Debug.LogWarning($"[GameManager] Tried to remove upgrade '{upgradeId}' but it wasn't in inventory.");
        return false;
    }
}