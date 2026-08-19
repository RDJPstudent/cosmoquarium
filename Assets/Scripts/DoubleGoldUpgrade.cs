using UnityEngine;

// Doubles the gold multiplier of the GoldFish that eats it - permanent for that fish's lifetime.
// Inherits all falling/floating/attraction/eating logic from Upgrade unchanged.
public class DoubleGoldUpgrade : Upgrade
{
    public int multiplierIncrease = 2; // how much to multiply the fish's current goldMultiplier by

    protected override void ApplyUpgradeEffect(Fish eater)
    {
        base.ApplyUpgradeEffect(eater); // still applies the fishColorOnConsume tint

        GoldFish goldFish = eater as GoldFish;
        if (goldFish != null)
        {
            goldFish.goldMultiplier *= multiplierIncrease;
            Debug.Log($"[DoubleGoldUpgrade] {goldFish.gameObject.name}'s gold multiplier is now {goldFish.goldMultiplier}");
        }
    }
}