using UnityEngine;

/// Which gameplay stat this upgrade affects. Add more here if your group designs additional upgrades.
public enum FishingUpgradeType
{
    BaitQuality,   // Biases catch odds toward rarer fish, stacking with the minigame success bias.
    CastRange,     // Reduces the Delay phase duration between casting and the minigame starting.
    LineStrength   // Widens the timing minigame's hit zone (see FishingMinigameController).
}

/// Represents one purchasable permanent fishing upgrade. Create instances via
/// Assets > Create > Fishing > Upgrade Data, and store them in Assets/Data/Upgrades/.
/// This holds only the template info (cost curve, effect type) - actual current level
/// is tracked at runtime by FishingUpgradeManager, not here.
[CreateAssetMenu(fileName = "NewFishingUpgrade", menuName = "Fishing/Upgrade Data")]
public class FishingUpgradeData : ScriptableObject
{
    [Header("Identity")]
    public string upgradeName;
    [TextArea] public string description;

    [Header("Effect")]
    public FishingUpgradeType upgradeType;
    [Tooltip("How much the effect changes per level. Meaning depends on upgradeType (e.g. seconds removed for CastRange, hit zone width added for LineStrength).")]
    public float effectPerLevel = 0.1f;

    [Header("Cost Curve")]
    [Tooltip("Cost to purchase level 1.")]
    public int baseCost = 10;
    [Tooltip("Cost multiplier per level. cost = baseCost * growthRate ^ currentLevel")]
    public float growthRate = 1.5f;

    [Tooltip("Optional cap. Set to 0 for no max level.")]
    public int maxLevel = 0;

    /// Calculates the cost to go from currentLevel to currentLevel + 1.
    public int GetCostForLevel(int currentLevel)
    {
        return Mathf.CeilToInt(baseCost * Mathf.Pow(growthRate, currentLevel));
    }
}
