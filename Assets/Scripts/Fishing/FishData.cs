using UnityEngine;

/// Defines the type of passive ability a fish species grants once placed in the aquarium.
/// Extend this list as new ability types are designed.
public enum FishAbilityType
{
    None,
    MoneyGeneration,
    DamageBonus,
    // Add more as your group designs them, e.g. HealthBonus, ClickSpeedBonus, etc.
}

/// Represents a single fish species. Create instances via
/// Assets > Create > Fishing > Fish Data, and store them in Assets/Data/Fish/.
[CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("Identity")]
    public string speciesName;
    public Sprite sprite;

    [Header("Ability")]
    public FishAbilityType abilityType;
    [Tooltip("Magnitude of the ability effect, e.g. money/sec or damage % bonus. Meaning depends on abilityType.")]
    public float abilityValue;

    [Header("Base Stats")]
    public float maxHP = 10f;

    [Header("Catch Odds")]
    [Tooltip("Relative weight used when randomly selecting a fish to catch. Higher = more common.")]
    public float catchWeight = 1f;
}
