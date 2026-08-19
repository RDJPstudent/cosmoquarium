using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// Tracks the player's current level for each FishingUpgradeData, and handles purchasing.
/// Persists across scene loads (singleton + DontDestroyOnLoad), since upgrades are permanent
/// and the Fishing scene itself gets reloaded each cycle.
///
/// Other Fishing scripts (FishingManager, FishingMinigameController) pull their current
/// effect values from here on Start() - this manager doesn't push effects to them directly,
/// keeping the coupling one-directional and simple.
public class FishingUpgradeManager : MonoBehaviour
{
    public static FishingUpgradeManager Instance { get; private set; }

    [Header("Upgrades")]
    [Tooltip("Drag all 3 FishingUpgradeData assets here (Bait Quality, Cast Range, Line Strength).")]
    public List<FishingUpgradeData> upgrades;

    // Runtime level tracking. Key = the upgrade template, Value = current purchased level (0 = not purchased yet).
    private Dictionary<FishingUpgradeData, int> levels = new Dictionary<FishingUpgradeData, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (FishingUpgradeData upgrade in upgrades)
        {
            if (!levels.ContainsKey(upgrade))
            {
                levels[upgrade] = 0;
            }
        }
    }

    private void Update()
    {
        // TEMPORARY: number-key testing until the real shop UI exists.
        // Press 1, 2, or 3 to attempt purchasing that upgrade slot.
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame && upgrades.Count > 0)
            TryPurchase(upgrades[0]);
        if (Keyboard.current.digit2Key.wasPressedThisFrame && upgrades.Count > 1)
            TryPurchase(upgrades[1]);
        if (Keyboard.current.digit3Key.wasPressedThisFrame && upgrades.Count > 2)
            TryPurchase(upgrades[2]);
    }

    public int GetLevel(FishingUpgradeData upgrade)
    {
        return levels.TryGetValue(upgrade, out int level) ? level : 0;
    }

    public int GetNextCost(FishingUpgradeData upgrade)
    {
        return upgrade.GetCostForLevel(GetLevel(upgrade));
    }

    /// Attempts to purchase the next level of the given upgrade.
    /// Returns true if it succeeded (and was deducted from SandDollarWallet).
    public bool TryPurchase(FishingUpgradeData upgrade)
    {
        int currentLevel = GetLevel(upgrade);

        if (upgrade.maxLevel > 0 && currentLevel >= upgrade.maxLevel)
        {
            Debug.Log($"[FishingUpgradeManager] {upgrade.upgradeName} is already at max level.");
            return false;
        }

        int cost = upgrade.GetCostForLevel(currentLevel);

        if (SandDollarWallet.Instance == null)
        {
            Debug.LogWarning("[FishingUpgradeManager] No SandDollarWallet found in scene.");
            return false;
        }

        if (!SandDollarWallet.Instance.TrySpendSandDollars(cost))
        {
            return false;
        }

        levels[upgrade] = currentLevel + 1;
        Debug.Log($"[FishingUpgradeManager] Purchased {upgrade.upgradeName}! Now level {levels[upgrade]}.");
        return true;
    }
}
