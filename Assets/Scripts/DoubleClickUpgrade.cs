using UnityEngine;

// Doubles the number of damage instances dealt per click on the fish that eats this.
public class DoubleClickUpgrade : Upgrade
{
    protected override void ApplyUpgradeEffect(Fish eater)
    {
        base.ApplyUpgradeEffect(eater); // still applies the fishColorOnConsume tint

        eater.clickDamageHits *= 2;
        Debug.Log($"[DoubleClickUpgrade] {eater.gameObject.name}'s click damage hits doubled to {eater.clickDamageHits}.");
    }
}